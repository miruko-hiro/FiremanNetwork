using System;
using System.Collections.Generic;
using Game.Configs;
using UnityEngine;
using View;
using View.Components;
using Random = UnityEngine.Random;

namespace Game
{
    public class PlayersManager
    {
        private readonly NetworkManager _networkManager;
        private readonly BulletManager _bulletManager;
        private readonly NetworkEvents _networkEvents;
        private readonly GameConfig _config;
        private readonly GameplayView _gameplayView;

        private float _health;
        private float _fireTimer;
        private PlayerController _playerController;
        private readonly List<PlayerController> _players = new List<PlayerController>();

        public bool IsFireman => _playerController != null && _playerController.Id == _networkManager.FiremanId;

        public PlayersManager(
            NetworkManager networkManager, 
            BulletManager bulletManager,
            NetworkEvents networkEvents, 
            GameConfig config, 
            GameplayView gameplayView)
        {
            _networkManager = networkManager;
            _networkEvents = networkEvents;
            _config = config;
            _gameplayView = gameplayView;
            _bulletManager = bulletManager;

            _health = _config.PlayerHelth;

            _networkManager.ModelChangedEvent += OnModelChanged;
            _networkEvents.PlayerControllerCreatedEvent += AddPlayer;
            _bulletManager.OnTargetReachedEvent += OnTargetReached;
        }

        public void Release()
        {
            _networkManager.ModelChangedEvent -= OnModelChanged;
            _networkEvents.PlayerControllerCreatedEvent -= AddPlayer;
            _bulletManager.OnTargetReachedEvent -= OnTargetReached;
        }
        
        public void CreateLocalPlayer()
        {
            var points = _gameplayView.SpawnPoints;
            var spawnPoint = points[Random.Range(0, points.Length)];
            
            _playerController = _networkManager.CreatePlayer(_config.PlayerPrefab.Path, spawnPoint.position, spawnPoint.rotation);
            _playerController.ShootEvent += OnShoot;
            
            _gameplayView.SetLocalPlayer(_playerController);
            _gameplayView.AddPlayer(_playerController);
            _playerController.HitpointsView.SetValue(1);
        }

        public void SetRandomFireman()
        {
            var idx = Random.Range(0, _players.Count);
            _networkManager.SetFireman(_players[idx].Id);
        }

        public void Tick(float deltaTime)
        {
            _fireTimer -= Time.deltaTime;
            
            if (!IsFireman || _health <= 0)
                return;

            _health -= deltaTime;
            _playerController.HitpointsView.SetValue(Math.Max(_health / _config.PlayerHelth, 0));

            if (_health <= 0)
            {
                _networkManager.EndGame();
            }
        }
        
        private void OnShoot(Vector3 point, Quaternion rotation)
        {
            if (_fireTimer > 0 || !IsFireman || _health <= 0 || _networkManager.GameState != GameState.Play)
                return;
            
            _bulletManager.CreateBullet(point, rotation);
            _fireTimer = _config.FirePeriod;
        }
        
        private void OnTargetReached(IBulletTarget target)
        {
            if (target is ZombieComponent zombie)
            {
                zombie.SetState(false);
            }
            else if (target is PlayerController player)
            {
                _networkManager.SetFireman(player.Id);
            }
        }
        
        private void OnModelChanged()
        {
            var id = _networkManager.FiremanId;
            foreach (var player in _players)
            {
                player.IsFireman = player.Id == id;
            }
        }
        
        private void AddPlayer(PlayerController player)
        {
            if (_players.Contains(player))
                return;
            
            _players.Add(player);
            _gameplayView.AddPlayer(player);
            player.IsFireman = player.Id == _networkManager.FiremanId;
            player.SetLayer(player.Id == _playerController.Id ? _config.CurrentPlayerLayer : _config.RemotePlayerLayer);
        }
    }
}