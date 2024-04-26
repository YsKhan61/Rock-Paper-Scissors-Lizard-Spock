using System.Collections.Generic;
using System.Linq;
using RPSLS.GameData;
using RPSLS.Services.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace RPSLS.Services
{
    public class AssetService : ServiceBase
    {
        private readonly Dictionary<AssetReference, List<GameObject>> _spawnedAssetMap =
            new Dictionary<AssetReference, List<GameObject>>();

        private readonly Dictionary<AssetReference, Queue<InstantiationParameters>> _queuedRequestMap =
            new Dictionary<AssetReference, Queue<InstantiationParameters>>();

        private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _operationsMap =
            new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

        [SerializeField] private List<AddressableAssetScriptableObject> assetReferences;

        private Dictionary<string, AssetReference> _assetReferenceMap;

        /// <summary>
        /// Load and instantiate an asset using the asset key.
        /// </summary>
        /// <param name="subAssetKey">key of the asset</param>
        /// <param name="instantiationParameters">aditional parameters</param>
        /// <returns>true if success, false otherwise</returns>
        internal bool LoadAndInstantiate(string subAssetKey, InstantiationParameters instantiationParameters)
        {
            if (!_assetReferenceMap.ContainsKey(subAssetKey))
                return false;

            var asset = _assetReferenceMap[subAssetKey];

            if (!asset.RuntimeKeyIsValid())
            {
                Debug.LogError($"Runtime Key is not valid for {asset.Asset.name} with asset key {subAssetKey}");
                return false;
            }

            if (_operationsMap.ContainsKey(asset))
            {
                if (_operationsMap[asset].IsDone)
                    SpawnAsset(asset, instantiationParameters);
                else EnqueueAssetRequest(asset, instantiationParameters);
            }
            else
                LoadAndSpawnAsset(asset, instantiationParameters);

            return true;
        }

        /// <summary>
        /// Load and instantiate an asset using the asset reference.
        /// </summary>
        /// <param name="asset">reference of the asset</param>
        /// <param name="instantiationParameters">additional parameters</param>
        /// <returns>true if success, false otherwise</returns>
        internal bool LoadAndInstantiate(AssetReference asset, InstantiationParameters instantiationParameters)
        {
            if (!asset.RuntimeKeyIsValid())
            {
                Debug.LogError($"Runtime Key is not valid for {asset.Asset.name}");
                return false;
            }

            if (_operationsMap.ContainsKey(asset))
            {
                if (_operationsMap[asset].IsDone)
                    SpawnAsset(asset, instantiationParameters);
                else EnqueueAssetRequest(asset, instantiationParameters);
            }
            else
                LoadAndSpawnAsset(asset, instantiationParameters);

            return true;
        }

        /// <summary>
        /// Load and instantiate an asset using the asset reference of type T.
        /// </summary>
        /// <typeparam name="T">the type of the asset reference</typeparam>
        /// <param name="asset">the asset reference to load and instantiate</param>
        /// <param name="instantiationParameters">additional parameters</param>
        /// <returns>true if success, false otherwise</returns>
        internal bool LoadAndInstantiate<T>(AssetReferenceT<T> asset, InstantiationParameters instantiationParameters)
            where T : Object
        {
            if (!asset.RuntimeKeyIsValid())
            {
                Debug.LogError($"Runtime Key is not valid for {asset.Asset.name}");
                return false;
            }

            if (_operationsMap.ContainsKey(asset))
            {
                if (_operationsMap[asset].IsDone)
                    SpawnAsset(asset, instantiationParameters);
                else EnqueueAssetRequest(asset, instantiationParameters);
            }
            else
                LoadAndSpawnAsset(asset, instantiationParameters);

            return true;
        }

        protected override void RegisterService() =>
            Bootstrap.RegisterService(this);

        protected override void Awake()
        {
            base.Awake();
            _assetReferenceMap = assetReferences
                .ToDictionary(assets => assets.AssetKey,
                    assets => assets.AssetReference);
        }

        private void OnValidate() =>
            _assetReferenceMap = assetReferences
                .ToDictionary(assets => assets.AssetKey,
                    assets => assets.AssetReference);


        private void LoadAndSpawnAsset(AssetReference assetReference, InstantiationParameters parameters)
        {
            var operation = assetReference.LoadAssetAsync<GameObject>();

            _operationsMap[assetReference] = operation;
            operation.Completed += handle =>
            {
                SpawnAsset(assetReference, parameters);

                if (!_queuedRequestMap.ContainsKey(assetReference)) return;
                while (_queuedRequestMap[assetReference]?.Any() == true)
                {
                    var queuedOperation = _queuedRequestMap[assetReference].Dequeue();
                    SpawnAsset(assetReference, queuedOperation);
                }
            };
        }

        private void EnqueueAssetRequest(AssetReference assetReference, InstantiationParameters parameters)
        {
            if (!_queuedRequestMap.ContainsKey(assetReference))
                _queuedRequestMap.Add(assetReference, new Queue<InstantiationParameters>());
            _queuedRequestMap[assetReference].Enqueue(parameters);
        }

        private void SpawnAsset(AssetReference assetReference, InstantiationParameters parameters) =>
            assetReference.InstantiateAsync(parameters.Position, parameters.Rotation, parameters.Parent)
                .Completed += operation =>
            {
                if (!_spawnedAssetMap.ContainsKey(assetReference))
                    _spawnedAssetMap.Add(assetReference, new List<GameObject>());
                _spawnedAssetMap[assetReference].Add(operation.Result);
                var memoryReleaseCallback = operation.Result.AddComponent<MemoryRelease>();
                memoryReleaseCallback.CurrentAssetReference = assetReference;
                memoryReleaseCallback.OnObjectDestroyed += ReleaseMemory;
            };

        private void ReleaseMemory(AssetReference assetReference, MemoryRelease releaseObj)
        {
            Addressables.ReleaseInstance(releaseObj.gameObject); // Release the spawned instance
            _spawnedAssetMap[assetReference].Remove(releaseObj.gameObject);

            if (_spawnedAssetMap[assetReference].Count > 1) return;

            if (_operationsMap.ContainsKey(assetReference) && _operationsMap[assetReference].IsValid())
            {
                Addressables.Release(_operationsMap[assetReference]); // Release the memory from the loaded asset
                _operationsMap.Remove(assetReference); // Finally remove from the dictionary 
            }
        }

        private void OnApplicationQuit() =>
            Resources.UnloadUnusedAssets(); // Force check to release the memory        
    }
}