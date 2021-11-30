using Game.Configs;
using UnityEngine;
using View.Components.Buffs;
using Random = UnityEngine.Random;

namespace Game
{
    public class BuffManager
    {
        private readonly GameConfig _config;
        private readonly GameObject[] _buffPrefabsMap;
        private readonly NetworkManager _networkManager;
        private bool _isRoomCreated;
        private IBuffTarget _target;
        private float _buffDuration;
        private float _tempCooldown;

        public BuffManager(GameConfig config, NetworkManager networkManager)
        {
            _config = config;
            _buffPrefabsMap = new GameObject[] {_config.SpeedBuffPrefab.Prefab, _config.FreezeBuffPrefab.Prefab};
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
            var buff = _networkManager.CreateObject<Buff>(_buffPrefabsMap[Random.Range(0, _buffPrefabsMap.Length)], 
                _config.BuffPosition, 
                Quaternion.identity);
            buff.OnTargetPickedUpBuffEvent += TargetPickedUpBuff;
            _buffDuration = buff.TimeOfAction;
            _tempCooldown = _config.СooldownBuffs;
            _target = null;
        }
        
        private void TargetPickedUpBuff(Buff buff, IBuffTarget target)
        {
            _target = target;
            Remove(buff);
        }

        private void RoomJoin(bool isRoomCreated, string none)
        {
            _isRoomCreated = isRoomCreated;
        }

        private void Remove(Buff buff)
        {
            buff.OnTargetPickedUpBuffEvent -= TargetPickedUpBuff;
            _networkManager.RemoveObject(buff.gameObject);
        }

        ~BuffManager()
        {
            _networkManager.RoomJoinEvent -= RoomJoin;
        }
        
    }
}