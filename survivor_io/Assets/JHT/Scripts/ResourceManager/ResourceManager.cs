using System.Collections.Generic;
using Cysharp.Text;
using JHT.Scripts.Common;
using UnityEngine;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

namespace JHT.Scripts.ResourceManager
{
	public class ResourceManager : Singleton<ResourceManager>
	{
		public void Clear()
		{
			ClearOriginalAssetList();
			ClearPreLoadObject();
		}

		#region ## OnPostLoad ##
		public delegate void OnPostLoad(Object objAsset, Object objInstance);
		private OnPostLoad _postLoad;

		public void BindOnPostLoad(OnPostLoad onPostLoad)
		{
			_postLoad -= onPostLoad;
			_postLoad += onPostLoad;
		}
		#endregion

		#region ## OriginalAsset ##
		private readonly Dictionary<string, Object> _cachedOriginalAssets = new();

		private T GetOriginalAsset<T>(string assetPath) where T : Object
		{
			if (string.IsNullOrEmpty(assetPath))
			{
				return null;
			}

			if (false == _cachedOriginalAssets.TryGetValue(assetPath, out var goAsset))
			{
				// 프리로드를 했더라도 경로에 따른 Object 정보가 없으므로 최초 1회 로드하는게 의도
				return null;
			}

			return goAsset as T;
		}

		private T NewOriginalAsset<T>(string assetPath, bool saveCache = false, bool showLog = true) where T : Object
		{
#if UNITY_EDITOR
			var loadStartTime = Time.realtimeSinceStartupAsDouble; 
#endif
			
			var asset = Resources.Load<T>(assetPath);
		
#if UNITY_EDITOR
			var logTime = 100;
			var warningTime = 500;
			
			var loadEndTime = Time.realtimeSinceStartupAsDouble;
			var loadTime = (int)((loadEndTime - loadStartTime) * 1000);
			if (logTime <= loadTime)
			{
				var logMessage =
					$"Resources.Load too slow [Path:{assetPath}] [ElapsedTime:{loadTime:##,##0} ms]";
				
				if (warningTime < loadTime)
				{
					Debug.LogWarning(logMessage);
				}
				else
				{
					Debug.Log(logMessage);
				}
			}
#endif
			
			if (asset.IsNull(false))
			{
				if (showLog)
				{
					Debug.LogError($"NewOriginalAsset : Not found resource path => \"{assetPath}\"");
				}
				return null;
			}

			if (saveCache)
			{
				_cachedOriginalAssets[assetPath] = asset;	
			}

			return asset;
		}

		private T GetOrNewOriginalAsset<T>(string assetPath, bool saveCache = false, bool showLog = true) where T : Object
		{
			var objOriginalAsset = GetOriginalAsset<T>(assetPath);
			if (objOriginalAsset)
			{
				return objOriginalAsset;
			}
			
			objOriginalAsset = NewOriginalAsset<T>(assetPath, saveCache, showLog);
			if (!objOriginalAsset)
			{
				return null;
			}

			return objOriginalAsset;
		}

		public T LoadOriginalAsset<T>(string assetPath, bool useCache = true, bool saveCache = true, bool showLog = true) where T : Object
		{
			if (string.IsNullOrEmpty(assetPath))
			{
				return null;
			}

			return useCache ? GetOrNewOriginalAsset<T>(assetPath, saveCache, showLog) : NewOriginalAsset<T>(assetPath, saveCache, showLog);
		}

		private void ClearOriginalAssetList()
		{
			_cachedOriginalAssets.Clear();
		}

		public void UnloadUnusedAssets()
		{
			Resources.UnloadUnusedAssets();
		}
		#endregion

		#region ## ObjectInstance ##

		public T LoadInstance<T>(string sPath, Transform trParent = null, bool bUsePreLoad = true, bool saveCache = true, bool bShowLog = true) where T : Object
		{
			if (string.IsNullOrEmpty(sPath))
			{
				return null;
			}

			var objOriginalAsset = LoadOriginalAsset<T>(sPath, true, saveCache, false);
			if (objOriginalAsset.IsNull(false))
			{
				if (bShowLog)
				{
					Debug.LogError($"ResourceManager.LoadInstance<{typeof(T).Name}>(\"{sPath}\") : loading failed (not found resource)");
				}
					
				return null;
			}

			return Instantiate(objOriginalAsset, trParent, bUsePreLoad);
		}

		public T InstantiateNoPreload<T>(T objSrc, Transform parent=null, bool showLog=false) where T : Object
		{
			return Instantiate(objSrc, parent, bUsePreLoad:false, showLog);
		}

		public T Instantiate<T>(T objSrc, bool bUsePreLoad, bool showLog=true) where T : Object
		{
			return Instantiate(objSrc, null, bUsePreLoad, showLog);
		}

		public T Instantiate<T>(T objSrc, Transform trParent = null, bool bUsePreLoad = true, bool showLog=true) where T : Object
		{
			if (objSrc.IsNull(false))
			{
				if (showLog)
				{
					Debug.LogError("Instantiate : objSrc is null");
				}
				return null;
			}

			var objInstance = bUsePreLoad ? GetPreLoadObject(objSrc, trParent) : null;
			if (objInstance.IsNull(false))
			{
				objInstance = Object.Instantiate(objSrc, trParent);
				if (objInstance.IsNull(false))
				{
					if (showLog)
					{
						Debug.LogError(ZString.Format(
							"Instantiate : Instance is null (src=\"{0}\")", objSrc ? objSrc.name : ""));
					}
					return null;
				}

				// (Clone) 붙는 이름 대신 원본 이름을 그대로 사용
				objInstance.name = objSrc.name;

				_postLoad?.Invoke(objSrc, objInstance);
			}

			return objInstance;
		}
		#endregion

		#region ## PreLoad (pool) ##
		private Transform _preLoadParent;
		private Transform PreLoadParent
		{
			get
			{
				if (false == _preLoadParent.IsNull(false))
				{
					return _preLoadParent;
				}

				var go = new GameObject();
				go.name = "PreLoadObject";
				go.transform.localPosition = Vector3.zero;
				go.transform.localScale = Vector3.one;
				go.SetActive(false);

				Object.DontDestroyOnLoad(go);

				_preLoadParent = go.transform;

				return _preLoadParent;
			}
		}

		private readonly Dictionary<Object, List<Object>> _mDictPreLoadObject = new ();

		public bool AddPreLoadObject<T>(string sPath, int iCount = 1) where T : Object
		{
			if (string.IsNullOrEmpty(sPath))
			{
				return false;
			}

			var objAsset = LoadOriginalAsset<T>(sPath, saveCache:true);
			if (false == AddPreLoadObject(objAsset, iCount))
			{
				return false;
			}

			return true;
		}

		public bool AddPreLoadObject(Object objAsset, int iCount = 1)
		{
			if (objAsset.IsNull())
			{
				return false;
			}

			for (int i = 0; i < iCount; ++i)
			{
				var objInstance = Instantiate(objAsset, PreLoadParent, false);
				if (objInstance.IsNull(false))
				{
					Debug.LogError($"ResourceManager.AddPreLoadObject(objAsset.name = \"{objAsset.name}\") : instantiate error");
					return false;
				}

				if (objInstance is GameObject go)
				{
					go.SetActive(false);
				}

				if (false == _mDictPreLoadObject.TryGetValue(objAsset, out var list))
				{
					list = new List<Object>();
					_mDictPreLoadObject[objAsset] = list;
				}

				list.Add(objInstance);
			}

			return true;
		}

		public bool IsPreLoadObject<T>(T objAsset) where T : Object
		{
			return _mDictPreLoadObject.TryGetValue(objAsset, out var _);
		}

		public int GetPreLoadObjectCount<T>(T objAsset) where T : Object
		{
			if (objAsset.IsNull())
			{
				return 0;
			}

			if (false == _mDictPreLoadObject.TryGetValue(objAsset, out var list) || null == list)
			{
				return 0;
			}

			return list.Count;
		}

		private T GetPreLoadObject<T>(T objAsset, Transform trParent = null) where T : Object
		{
			if (objAsset.IsNull())
			{
				return null;
			}

			if (false == _mDictPreLoadObject.TryGetValue(objAsset, out var list) || null == list || 0 == list.Count)
			{
				return null;
			}

			var objInstance = list[0];
			list.RemoveAt(0);

			if (objInstance is GameObject go)
			{
				go.transform.SetParent(trParent, false);
				go.SetActive(true);
			}

			return objInstance as T;
		}

		private void ClearPreLoadObject()
		{
			foreach (var it in _mDictPreLoadObject)
			{
				foreach (var it2 in it.Value)
				{
					if (it2.IsNull(false))
					{
						continue;
					}

					if (it2 is GameObject go)
					{
						if (go.IsDestroyed(false))
						{
							continue;
						}

						go.DestroySafe();
					}
					else
					{
						Object.Destroy(it2);
					}
				}

				it.Value.Clear();
			}

			_mDictPreLoadObject.Clear();
		}
		#endregion
	}
}
