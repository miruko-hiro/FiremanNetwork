using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Configs
{
    [CreateAssetMenu(menuName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Settings")]
        public float BulletLifeTime;
        public float PlayerHelth;
        public float FirePeriod;
        public float –°ooldownBuffs;
        public Vector3 BuffPosition;
        
        [Header("Prefabs")]
        public ResourceLink PlayerPrefab;
        public ResourceLink BulletPrefab;
        public ResourceLink SpeedBuffPrefab;
        public ResourceLink FreezeBuffPrefab;

        [Header("Player View")]
        public LayerMaskAsInt CurrentPlayerLayer;
        public LayerMaskAsInt RemotePlayerLayer;

        public void OnValidate()
        {
#if UNITY_EDITOR
            PlayerPrefab.OnValidate();
            BulletPrefab.OnValidate();
            SpeedBuffPrefab.OnValidate();
            FreezeBuffPrefab.OnValidate();
#endif
        }
    }

    [Serializable]
    public class ResourceLink
    {
        [SerializeField] private GameObject _resource;
        [SerializeField] private string _resourcePath;

        public GameObject Prefab => _resource;
        public string Path => _resourcePath;
#if UNITY_EDITOR
        public void OnValidate()
        {
            _resourcePath = UnityEditor.AssetDatabase.GetAssetPath(_resource)
                .Replace("Assets/Resources/", "")
                .Replace(".prefab", "");
        }
#endif
    }
    
    [Serializable]
    public class LayerMaskAsInt
    {
        [SerializeField] private LayerMask _currentPlayerLayer;
        
        public static implicit operator int(LayerMaskAsInt x)
        {
            return (int) Mathf.Log(x._currentPlayerLayer.value, 2);
        }
    }
}