using System;
using UnityEngine;

namespace View.Components.Buffs
{
    public class FreezeBuff : Buff
    {
        public override event Action<Buff, IBuffTarget> OnTargetPickedUpBuffEvent;
 
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _second = 5f;
        
        public override float TimeOfAction => _second;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<PlayerController>();
                player.PickedUpFreezeBuff(_speed, _second);
                OnTargetPickedUpBuffEvent?.Invoke(this, player);
            }
        }
    }
}