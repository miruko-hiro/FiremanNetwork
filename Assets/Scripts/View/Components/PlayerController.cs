using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using View.Components.Buffs;
using View.Input;

namespace View.Components
{
    public class PlayerController : MonoBehaviour, IBulletTarget, IBuffTarget
    {
        public event Action<Vector3, Quaternion> ShootEvent;
        public event Action<int, float, float> OnPickedUpFreezeBuffEvent;

        [SerializeField] private PhotonView _photonView;
        
        public Rigidbody Rigidbody;
        public Renderer BodyRenderer;
        public Transform BulletSpawnPoint;

        [Header("Views")]
        public HitpointsView HitpointsView;

        [Header("Gameplay")] 
        public Material NormalBodyMaterial;
        public Material FiremanBodyMaterial;
        public float Speed = 5f;

        [Header("Network")] 
        [SerializeField] private NetworkEvents _events;

        private IPlayerInput _playerInput;

        public int Id => _photonView.ViewID;
        
        public bool IsFireman
        {
            set => BodyRenderer.material = value ? FiremanBodyMaterial : NormalBodyMaterial;
        }

        public void SetInput(IPlayerInput playerInput)
        {
            _playerInput = playerInput;
        }

        public void SetLayer(int layerMask)
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach(var c in colliders)
            {
                c.gameObject.layer = layerMask;
            }
        }

        public void Start()
        {
            _events.RaisePlayerControllerCreated(this);
        }

        private void OnEnable()
        {
            IsFireman = true;
            HitpointsView.SetPlayerName(_photonView.Controller.CustomProperties["PlayerName"].ToString());
        }

        private void Update()
        {
            if (_playerInput == null)
                return;

            var (moveDirection, viewDirection, shoot) = _playerInput.CurrentInput();
            ProcessShoot(shoot);
            Rigidbody.velocity = moveDirection.normalized * Speed;
            transform.rotation = viewDirection;
        }

        private void ProcessShoot(bool isShoot)
        {
            if (isShoot)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            ShootEvent?.Invoke(BulletSpawnPoint.position, transform.rotation);
        }

        [PunRPC]
        public void ChangeSpeedForCertainTime(float newSpeed, float second)
        {
            StartCoroutine(SpeedBuffCoroutine(newSpeed, second));
        }

        public void PickedUpFreezeBuff(float newSpeed, float second)
        {
            OnPickedUpFreezeBuffEvent?.Invoke(Id, newSpeed, second);
        }

        public void ChangeSpeedForCertainTimeRcp(float newSpeed, float time)
        {
            _photonView.RPC(nameof(ChangeSpeedForCertainTime), RpcTarget.All, newSpeed, time);
        }

        private IEnumerator SpeedBuffCoroutine(float newSpeed, float second)
        {
            var temp = Speed;
            Speed = newSpeed;
            yield return new WaitForSeconds(second);
            Speed = temp;
        }
    }
}