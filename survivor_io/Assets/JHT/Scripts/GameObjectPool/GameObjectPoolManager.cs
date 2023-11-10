using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Text;
using JHT.Scripts.Common;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace JHT.Scripts.GameObjectPool
{
	[Flags]
	public enum PoolableObjectFlag
	{
		None = 0,
		
		ParentHold = 1 << 0,
		ManualReturnToPool = 1 << 1,
		ResetTransformReturnToPool = 1 << 2,
	}
	
	public class GameObjectPoolManager : Singleton<GameObjectPoolManager>
	{
		private const string StrDisabledPool = "(DisabledPool)";

		private bool IsClearing { get; set; }

		private Transform _poolableParent;
		private Transform PoolableParent
		{
			get
			{
				if (false == _poolableParent.IsNull(false))
				{
					return _poolableParent;
				}

				var go = new GameObject();
				go.name = PoolableParentName;
				go.transform.localPosition = Vector3.zero;
				go.transform.localScale = Vector3.one;
				go.SetActive(false);

				UnityEngine.Object.DontDestroyOnLoad(go);

				_poolableParent = go.transform;

				return _poolableParent;
			}
		}
		
		private int _lastAssetId = -1;
		private int _lastInstanceId = -1;
		
		private readonly Dictionary<GameObject, int> _assetIdByAsset = new();
		private readonly Dictionary<int, int> _dictCurPoolSizeByAssetId = new();
		private readonly Dictionary<int, int> _dictMaxPoolSizeByAssetId = new();
		private readonly Dictionary<int, Dictionary<int, Dictionary<int ,Dictionary<int, PoolableObject>>>> _dictEnabledObjectPoolByAssetId = new();
		private readonly Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, PoolableObject>>>> _dictDisabledObjectPoolByAssetId = new();
		
		private const string PoolableParentName = "PoolableObject";
		
		private void AddEnabledObject(int assetId, PoolableObject poolObject)
		{
			if (poolObject.IsNull())
			{
				return;
			}

			if (false == _dictEnabledObjectPoolByAssetId.TryGetValue(assetId, out var dictEnabledObjectPoolByVariantId))
			{
				dictEnabledObjectPoolByVariantId = new ();
				_dictEnabledObjectPoolByAssetId.Add(assetId, dictEnabledObjectPoolByVariantId);
			}
			
			if (false == dictEnabledObjectPoolByVariantId.TryGetValue(poolObject.VariantId, out var dictEnabledObjectPoolByParentId))
			{
				dictEnabledObjectPoolByParentId = new ();
				dictEnabledObjectPoolByVariantId.Add(poolObject.VariantId, dictEnabledObjectPoolByParentId);
			}
			
			if (false == dictEnabledObjectPoolByParentId.TryGetValue(poolObject.ParentId, out var dictEnabledObjectPoolByInstanceId))
			{
				dictEnabledObjectPoolByInstanceId = new ();
				dictEnabledObjectPoolByParentId.Add(poolObject.ParentId, dictEnabledObjectPoolByInstanceId);
			}

			dictEnabledObjectPoolByInstanceId.Add(poolObject.InstanceId, poolObject);
			poolObject.SetIsInUse(true);

#if UNITY_EDITOR
			if (poolObject.name.Contains(StrDisabledPool))
			{
				poolObject.name = poolObject.name.Replace(StrDisabledPool, "");
			}
#endif
		}

		private void AddDisabledObject(int assetId, PoolableObject poolObject)
		{
			if (poolObject.IsNull())
			{
				return;
			}

			// 객체가 삭제중이면 disable object pool에 담지 않는다.
			if (poolObject.gameObject.IsDestroyed())
			{
				return;
			}

			if (_dictMaxPoolSizeByAssetId.TryGetValue(assetId, out var maxPoolSize))
			{
				if (_dictCurPoolSizeByAssetId.TryGetValue(assetId, out var curPoolSize))
				{
					if (maxPoolSize <= curPoolSize)
					{
						poolObject.gameObject.DestroySafe();
						return;
					}
				}
			}

			if (false == _dictDisabledObjectPoolByAssetId.TryGetValue(assetId, out var dictDisabledObjectPoolByVariantId))
			{
				dictDisabledObjectPoolByVariantId = new();
				_dictDisabledObjectPoolByAssetId.Add(assetId, dictDisabledObjectPoolByVariantId);
			}

			if (false == dictDisabledObjectPoolByVariantId.TryGetValue(poolObject.VariantId, out var dictDisabledObjectPoolByParentId))
			{
				dictDisabledObjectPoolByParentId = new();
				dictDisabledObjectPoolByVariantId.Add(poolObject.VariantId, dictDisabledObjectPoolByParentId);
			}

			if (false == dictDisabledObjectPoolByParentId.TryGetValue(poolObject.ParentId, out var dictDisabledObjectPoolByInstanceId))
			{
				dictDisabledObjectPoolByInstanceId = new();
				dictDisabledObjectPoolByParentId.Add(poolObject.ParentId, dictDisabledObjectPoolByInstanceId);
			}

			dictDisabledObjectPoolByInstanceId.Add(poolObject.InstanceId, poolObject);
			poolObject.SetIsInUse(false);

#if UNITY_EDITOR
			poolObject.name = ZString.Concat(poolObject.name, StrDisabledPool);
#endif
		}

		private bool TryGetDisabledObject(Dictionary<int, PoolableObject> dictDisabledObjectPoolByInstanceId, 
			Transform trParent,
			out PoolableObject outPoolObject)
		{
			var isAllHoldOrDestroyed = true;
			foreach (var (_, poolObject) in dictDisabledObjectPoolByInstanceId)
			{
				if (poolObject.Hold)
				{
					continue;
				}

				if (poolObject.IsDestroyed)
				{
					continue;
				}

				isAllHoldOrDestroyed = false;
						
				if (poolObject.transform.parent != trParent)
				{
					continue;
				}

				outPoolObject = poolObject;
				return true;
			}

			if (isAllHoldOrDestroyed)
			{
				outPoolObject = null;
				return false;
			}
					
			foreach (var (_, poolObject) in dictDisabledObjectPoolByInstanceId)
			{
				if (poolObject.Hold)
				{
					continue;
				}

				if (poolObject.IsDestroyed)
				{
					continue;
				}

				if (poolObject.HoldParent && poolObject.transform.parent != trParent && poolObject.transform.parent != PoolableParent)
				{
					continue;
				}

				outPoolObject = poolObject;
				return true;
			}
			
			outPoolObject = null;
			return false;
		}

		private PoolableObject GetDisabledObject(int assetId, int variantId, int parentId, Transform trParent)
		{
			if (false == _dictDisabledObjectPoolByAssetId.TryGetValue(assetId, out var dictDisabledObjectPoolByVariantId))
			{
				return null;
			}

			if (dictDisabledObjectPoolByVariantId.Count <= 0)
			{
				return null;
			}

			if (dictDisabledObjectPoolByVariantId.TryGetValue(variantId,
				    out var dictDisabledObjectPoolByParentId))
			{
				if (0 < dictDisabledObjectPoolByParentId.Count)
				{
					if (dictDisabledObjectPoolByParentId.TryGetValue(parentId, 
						    out var dictDisabledObjectPoolByInstanceId))
					{
						if (0 < dictDisabledObjectPoolByInstanceId.Count)
						{
							if (TryGetDisabledObject(dictDisabledObjectPoolByInstanceId, trParent, out var poolObject))
							{
								return poolObject;
							}
						}
					}
				}

				foreach (var (parentIdTemp, dictDisabledObjectPoolByInstanceId) in dictDisabledObjectPoolByParentId)
				{
					if (parentIdTemp == parentId)
					{
						continue;
					}

					if (dictDisabledObjectPoolByInstanceId.Count <= 0)
					{
						continue;
					}

					if (TryGetDisabledObject(dictDisabledObjectPoolByInstanceId, trParent, out var poolObject))
					{
						return poolObject;
					}
				}
			}

			return null;
		}

		public void MoveToDisabledObjectPool(int assetId, PoolableObject poolObject)
		{
			if (IsClearing)
			{
				return;
			}
			
			if (_dictEnabledObjectPoolByAssetId.TryGetValue(poolObject.AssetId, out var dictEnabledObjectPoolByVariantId)
			    && dictEnabledObjectPoolByVariantId.TryGetValue(poolObject.VariantId, out var dictEnabledObjectPoolByParentId)
			    && dictEnabledObjectPoolByParentId.TryGetValue(poolObject.ParentId, out var dictEnabledObjectPoolByInstanceId))
			{
				dictEnabledObjectPoolByInstanceId.Remove(poolObject.InstanceId);
			}

			AddDisabledObject(assetId, poolObject);
		}

		public void MoveToEnabledObjectPool(int assetId, int prevParentId, PoolableObject poolObject)
		{
			if (_dictDisabledObjectPoolByAssetId.TryGetValue(poolObject.AssetId, out var dictDisabledObjectPoolByVariantId)
			    && dictDisabledObjectPoolByVariantId.TryGetValue(poolObject.VariantId, out var dictDisabledObjectPoolByParentId)
			    && dictDisabledObjectPoolByParentId.TryGetValue(prevParentId, out var dictDisabledObjectPoolByInstanceId))
			{
				dictDisabledObjectPoolByInstanceId.Remove(poolObject.InstanceId);
			}

			AddEnabledObject(assetId, poolObject);
		}

		public void SetMaxPoolSize(string sPrefabPath, int maxPoolSize)
		{
			if (string.IsNullOrEmpty(sPrefabPath))
			{
				return;
			}

			var goAsset = ResourceManager.ResourceManager.Instance.LoadOriginalAsset<GameObject>(sPrefabPath, useCache:true, saveCache:true);
			_ = TryGetAssetId(goAsset, out var assetId);
			SetMaxPoolSize(assetId, maxPoolSize);
		}

		public void SetMaxPoolSize(int assetId, int maxPoolSize)
		{
			if (0 <= maxPoolSize)
			{
				_dictMaxPoolSizeByAssetId[assetId] = maxPoolSize;
			}
		}

		private bool TryGetAssetId(GameObject goAsset, out int assetId, bool make = true)
		{
			if (false == _assetIdByAsset.TryGetValue(goAsset, out assetId))
			{
				if (false == make)
				{
					return false;
				}
				
				assetId = ++_lastAssetId;
				_assetIdByAsset[goAsset] = assetId;
			}

			return true;
		}

		public GameObject GetOrNewObject(string sPrefabPath, Transform trParent = null, PoolableObjectFlag flag = PoolableObjectFlag.None, int variantId = -1, int parentId = -1)
		{
			if (string.IsNullOrEmpty(sPrefabPath))
			{
				return null;
			}

			var goAsset = ResourceManager.ResourceManager.Instance.LoadOriginalAsset<GameObject>(sPrefabPath, useCache:true, saveCache:true);
			return GetOrNewObject(goAsset, trParent, flag, variantId, parentId);
		}

		public GameObject GetOrNewObject(GameObject goAsset, Transform trParent = null, PoolableObjectFlag flag = PoolableObjectFlag.None, int variantId = -1, int parentId = -1)
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			Stopwatch sw = new Stopwatch();
			sw.Start();
#endif
			
			if (goAsset.IsNull())
			{
				return null;
			}

			_ = TryGetAssetId(goAsset, out var assetId);
			
			bool bHoldParent = flag.HasFlagNonAlloc(PoolableObjectFlag.ParentHold);
			bool bManualReturnToPool = flag.HasFlagNonAlloc(PoolableObjectFlag.ManualReturnToPool);
			bool bResetTransformReturnToPool = flag.HasFlagNonAlloc(PoolableObjectFlag.ResetTransformReturnToPool);

			var poolObject = GetDisabledObject(assetId, variantId, parentId, trParent);
			if (poolObject.IsNull(false))
			{
				var newGameObject = ResourceManager.ResourceManager.Instance.Instantiate(goAsset, trParent);
				if (newGameObject.IsNull())
				{
					return null;
				}

				newGameObject.name = newGameObject.name.Replace("(Clone)", "");

				if (false == newGameObject.TryGetComponent(out poolObject))
				{
					Debug.LogWarning(
						"if (false == newGameObject.TryGetComponent(out PoolableObject poolableObject))");
					poolObject = newGameObject.AddComponent<PoolableObject>();
				}
				
				if (0 == poolObject.ShowCount)
				{
					poolObject.OnInstantiate();
					poolObject.SetId(assetId, ++_lastInstanceId, variantId, parentId);
				}
				
				poolObject.GoAsset = goAsset;
				poolObject.HoldParent = bHoldParent;
				poolObject.ManualReturnToPool = bManualReturnToPool;
				poolObject.ResetTransformReturnToPool = bResetTransformReturnToPool;
				
				AddEnabledObject(assetId, poolObject);

				if (_dictMaxPoolSizeByAssetId.ContainsKey(assetId))
				{
					_dictCurPoolSizeByAssetId.TryAdd(assetId, 0);

					_dictCurPoolSizeByAssetId[assetId] += 1;
				}
			}
			else
			{
				var prevParentId = poolObject.ParentId;
				poolObject.SetParentId(parentId);
				
				MoveToEnabledObjectPool(assetId, prevParentId, poolObject);
				
				if (poolObject.transform.parent != trParent)
				{
					poolObject.transform.SetParent(trParent, false);
				}
				
				poolObject.gameObject.SetActive(true);
			}

			poolObject.AddShowCount();
			return poolObject.gameObject;
		}

		public void OnDestroyObject(PoolableObject poolObject)
		{
			if (poolObject.IsNull())
			{
				return;
			}

			if (_dictEnabledObjectPoolByAssetId.TryGetValue(poolObject.AssetId, out var dictEnabledObjectPoolByVariantId)
			    && dictEnabledObjectPoolByVariantId.TryGetValue(poolObject.VariantId, out var dictEnabledObjectPoolByParentId)
			    && dictEnabledObjectPoolByParentId.TryGetValue(poolObject.ParentId, out var dictEnabledObjectPoolByInstanceId))
			{
				dictEnabledObjectPoolByInstanceId.Remove(poolObject.InstanceId);
			}
			
			if (_dictDisabledObjectPoolByAssetId.TryGetValue(poolObject.AssetId, out var dictDisabledObjectPoolByVariantId)
			    && dictDisabledObjectPoolByVariantId.TryGetValue(poolObject.VariantId, out var dictDisabledObjectPoolByParentId)
			    && dictDisabledObjectPoolByParentId.TryGetValue(poolObject.ParentId, out var dictDisabledObjectPoolByInstanceId))
			{
				dictDisabledObjectPoolByInstanceId.Remove(poolObject.InstanceId);
			}

			if (_dictCurPoolSizeByAssetId.ContainsKey(poolObject.AssetId))
			{
				_dictCurPoolSizeByAssetId[poolObject.AssetId] -= 1;	
			}
		}

		public void Clear()
		{
			IsClearing = true;

			void ClearContainer(Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, PoolableObject>>>> poolContainer)
			{
				if (poolContainer.IsNull())
				{
					return;
				}
				
				foreach (var it1 in poolContainer)
				{
					foreach (var it2 in it1.Value)
					{
						foreach (var it3 in it2.Value)
						{
							foreach (var (_, poolObject) in it3.Value)
							{
								if (poolObject.IsNull())
								{
									continue;
								}
								
								if (poolObject.gameObject.IsDestroyed())
								{
									continue;
								}
						
								poolObject.gameObject.DestroySafe();
							}
					
							it3.Value.Clear();
						}

						it2.Value.Clear();
					}
					
					it1.Value.Clear();
				}
				
				poolContainer.Clear();
			}

			_lastAssetId = -1;
			_lastInstanceId = -1;
			_assetIdByAsset.Clear();
			_dictCurPoolSizeByAssetId.Clear();
			_dictMaxPoolSizeByAssetId.Clear();
			
			ClearContainer(_dictEnabledObjectPoolByAssetId);
			ClearContainer(_dictDisabledObjectPoolByAssetId);

			IsClearing = false;
		}

		public int GetPoolableObjectCountByOriginalAsset(GameObject goAsset)
		{
			if (false == TryGetAssetId(goAsset, out var assetId, false))
			{
				return 0;
			}
			
			var count = 0;
			{
				if (_dictDisabledObjectPoolByAssetId.TryGetValue(assetId, out var disableList))
				{
					count += disableList.Sum(it => it.Value.Sum(it2 => it2.Value.Count));
				}
			
				if (_dictEnabledObjectPoolByAssetId.TryGetValue(assetId, out var enableList))
				{
					count += enableList.Sum(it => it.Value.Sum(it2 => it2.Value.Count));
				}
			}

			return count;
		}

		public void AllObjectKeep()
		{
			Dictionary<int, PoolableObject> allObjects = new();
			foreach (var it in _dictEnabledObjectPoolByAssetId)
			{
				foreach (var it2 in it.Value)
				{
					foreach (var it3 in it2.Value)
					{
						foreach (var (instanceId, poolObject) in it3.Value)
						{
							if (poolObject.gameObject.IsDestroyed())
							{
								continue;
							}

							allObjects.Add(instanceId, poolObject);
						}
					}
				}
			}

			foreach (var (_, poolObject) in allObjects)
			{
				poolObject.Hold = false;
				poolObject.ToDisable();
			}
			
			foreach (var it in _dictDisabledObjectPoolByAssetId)
			{
				foreach (var it2 in it.Value)
				{
					foreach (var it3 in it2.Value)
					{
						foreach (var (_, poolObject) in it3.Value)
						{
							if (poolObject.gameObject.IsDestroyed())
							{
								continue;
							}

							if (poolObject.transform.parent == PoolableParent)
							{
								continue;
							}
					
							poolObject.transform.SetParent(PoolableParent, false);
						}
					}
				}
			}
		}
	}
}
