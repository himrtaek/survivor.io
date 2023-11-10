using System;
using JHT.Scripts.Common;
using UnityEngine;

namespace JHT.Scripts.GameObjectPool
{
	public static class PoolableObjectExtensions
	{
		public static int GetPoolObjectShowCount(this GameObject gameObject)
		{
			if (gameObject.TryGetComponent(out PoolableObject poolableObject))
			{
				return poolableObject.ShowCount;
			}

			return -1;
		}
		
		public static void SetActiveFalseWithPool(this PoolableObject poolableObject, bool force = false)
		{
			if (poolableObject.gameObject.IsNull())
			{
				return;
			}

			if (force || false == poolableObject.gameObject.activeInHierarchy)
			{
				poolableObject.ToDisable();
			}

			poolableObject.gameObject.SetActive(false);
		}
		
		public static void SetActiveFalseWithPool(this GameObject go, bool force = false)
		{
			if (go.IsNull())
			{
				return;
			}

			if (force || false == go.activeInHierarchy)
			{
				if (go.TryGetComponent(out PoolableObject poolableObject))
				{
					poolableObject.ToDisable();
				}
			}

			go.SetActive(false);
		}
	}

	public class PoolableObject : MonoBehaviour, IDestroyable, IInstantiatable
	{
		[NonSerialized] public string PrefabPath = "";
		[NonSerialized] public GameObject GoAsset;

		public int AssetId { get; private set; } = -1;
		
		public int VariantId { get; private set; } = -1;
		
		public int ParentId { get; private set; } = -1;
		
		public int InstanceId { get; private set; }

		public int ShowCount { get; private set; }

		public bool IsInUse { get; private set; }

		[NonSerialized] public Vector3 OriginalLocalPosition;
		[NonSerialized] public Quaternion OriginalLocalRotation;
		[NonSerialized] public Vector3 OriginalLocalScale;

		[NonSerialized] public bool HoldParent = false;
		[NonSerialized] public bool Hold;
		[NonSerialized] public bool ManualReturnToPool;
		[NonSerialized] public bool ResetTransformReturnToPool;
		
		[NonSerialized] public bool IsInitialized;
		[NonSerialized] public bool IsDestroyed;

		public void OnInstantiate()
		{
			var cachedTransform = transform;
			OriginalLocalPosition = cachedTransform.localPosition;
			OriginalLocalRotation = cachedTransform.localRotation;
			OriginalLocalScale = cachedTransform.localScale;
		}
		
		public void SetId(int assetId, int instanceId, int variantId, int parentId)
		{
			AssetId = assetId;
			InstanceId = instanceId;
			VariantId = variantId;
			ParentId = parentId;
			
			IsInitialized = true;
		}
		
		public void SetParentId(int parentId)
		{
			ParentId = parentId;
		}

		public void AddShowCount()
		{
			ShowCount++;
		}

		public void SetIsInUse(bool isInUse)
		{
			IsInUse = isInUse;
		}

		void OnEnable()
		{	
			ToEnable();
		}

		void OnDisable()
		{
			ToDisable(false);
		}

		void ToEnable()
		{
			if (false == IsInitialized)
			{
				return;
			}
			
			if (IsInUse)
			{
				return;
			}
			
			GameObjectPoolManager.Instance.MoveToEnabledObjectPool(AssetId, ParentId, this);
		}

		public void ToDisable(bool manualReturn = true)
		{
			if (false == IsInitialized)
			{
				return;
			}
			
			if (false == IsInUse)
			{
				return;
			}
			
			if (false == manualReturn && ManualReturnToPool)
			{
				return;
			}
			
			if ((false == Hold).IsFalse())
			{
				return;
			}

			if (ResetTransformReturnToPool)
			{
				var tr = transform;
				tr.localPosition = OriginalLocalPosition;
				tr.localRotation = OriginalLocalRotation;
				tr.localScale = OriginalLocalScale;	
			}

			GameObjectPoolManager.Instance.MoveToDisabledObjectPool(AssetId, this);
		}

		void OnDestroy()
		{
			if (false == IsInitialized)
			{
				return;
			}

			GameObjectPoolManager.Instance.OnDestroyObject(this);
		}

		public void OnDestroyNow()
		{
			IsDestroyed = true;
		}
	}
}
