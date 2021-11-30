using System;
using Game.Configs;
using Photon.Pun;
using UnityEngine;
using View.Components.Buffs;
using Random = UnityEngine.Random;

namespace Game
{
    public class BuffManager
    {
        public event Action<IBuffTarget> OnTargetPickedUpBuffEvent;

        private float _tempCooldown;
        
        private readonly GameConfig _config;
        private readonly string[] _pathsToBuffPrefabsMap;
        private readonly NetworkManager _networkManager;
        private bool _isRoomCreated;
        private IBuffTarget _target;
        private float _buffDuration;

        public BuffManager(GameConfig config, NetworkManager networkManager)
        {
            _config = config;
            _pathsToBuffPrefabsMap = new string[] {_config.SpeedBuffPrefab.Path};
            _networkManager = networkManager;
            networkManager.RoomJoinEvent += RoomJoin;
        }
        
        public void Tick(float deltaTime)
        {
            if (_target == null)
            {
                _tempCooldown -= deltaTime;
                if(_tempCooldown <= 0f && _buffDuration <= 0f && _isRoomCreated) CreateBuff();
            }
            else
            {
                _buffDuration -= deltaTime;
                if (_buffDuration <= 0f) _target = null;
            }
        }
        
        private void CreateBuff()
        {
            var buff = PhotonNetwork.Instantiate(_pathsToBuffPrefabsMap[Random.Range(0, _pathsToBuffPrefabsMap.Length)], 
                _config.BuffPosition, 
                Quaternion.identity).GetComponent<Buff>();
            buff.OnTargetPickedUpBuffEvent += TargetPickedUpBuff;
            _buffDuration = buff.TimeOfAction;
            _tempCooldown = _config.СooldownBuffs;
            _target = null;
        }
        
        private void TargetPickedUpBuff(Buff buff, IBuffTarget target)
        {
            _target = target;
            Remove(buff);
            OnTargetPickedUpBuffEvent?.Invoke(target);
        }

        private void RoomJoin(bool isRoomCreated, string none)
        {
            _isRoomCreated = isRoomCreated;
        }

        private void Remove(Buff buff)
        {
            buff.OnTargetPickedUpBuffEvent -= TargetPickedUpBuff;
            PhotonNetwork.Destroy(buff.gameObject);
        }

        ~BuffManager()
        {
            _networkManager.RoomJoinEvent -= RoomJoin;
        }
        
    }
}