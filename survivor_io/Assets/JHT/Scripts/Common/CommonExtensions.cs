using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Text;
using JHT.Scripts.Common.PerformanceExtension;
using JHT.Scripts.GameObjectPool;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace JHT.Scripts.Common
{
	public static class BoolExtensions
	{
		[Conditional("UNITY_EDITOR")]
		private static void TrueLog(string typeName, StackTrace stackTrace, int callDepth = 1)
		{
			var frame = stackTrace.GetFrame(callDepth);
			var log = ZString.Format("{3} Is True in \"{2}\" (File=\"{0}\", Line={1})", 
				System.IO.Path.GetFileName(frame?.GetFileName() ?? ""), 
				frame?.GetFileLineNumber()??0, 
				frame?.GetMethod()?.Name??"", 
				typeName
			);
			Debug.LogError(log);
		}
		
		[Conditional("UNITY_EDITOR")]
		private static void FalseLog(string typeName, StackTrace stackTrace, int callDepth = 1)
		{
			var frame = stackTrace.GetFrame(callDepth);
			var log = ZString.Format("{3} Is False in \"{2}\" (File=\"{0}\", Line={1})", 
				System.IO.Path.GetFileName(frame?.GetFileName() ?? ""), 
				frame?.GetFileLineNumber()??0, 
				frame?.GetMethod()?.Name??"", 
				typeName
			);
			Debug.LogError(log);
		}
		
		public static bool IsFalse(this bool obj, bool showLog = true)
		{
			if (false == obj)
			{
				if (showLog)
				{
#if DEBUG
					FalseLog("System.Boolean", new StackTrace(true));
#else
					Debug.LogError("System.Boolean Is False");
#endif
				}

				return true;
			}

			return false;
		}
	}
	
	public static class UnityObjectExtensions
	{
		public static void DoRecursiveAction(this GameObject goParent, Action<GameObject> action)
		{
			if (goParent.IsNull())
			{
				return;
			}

			action(goParent);

			for (int i = 0; i < goParent.transform.childCount; ++i)
			{
				var child = goParent.transform.GetChild(i).gameObject;
				if (child.IsDestroyed())
				{
					continue;
				}

				DoRecursiveAction(child, action);
			}
		}
		
		public static bool IsPrefab(this GameObject gameObject)
		{
			if (gameObject.IsNull())
			{
				throw new ArgumentNullException(nameof(gameObject));
			}

			return
				!gameObject.scene.IsValid() &&
				!gameObject.scene.isLoaded &&
				gameObject.GetInstanceID() >= 0 &&
				// I noticed that ones with IDs under 0 were objects I didn't recognize
				!gameObject.hideFlags.HasFlagNonAlloc(HideFlags.HideInHierarchy);
			// I don't care about GameObjects *inside* prefabs, just the overall prefab.
		}

		public static bool IsRootPrefab(this GameObject gameObject)
		{
			if (gameObject.IsNull() || gameObject.transform.IsNull())
			{
				throw new ArgumentNullException(nameof(gameObject));
			}

			// 부모가 있으면 루트 프리팹 아님
			if (false == gameObject.transform.parent.IsNull(false))
			{
				return false;
			}
	        
			// 일반 프리팹 체크
			if (false == IsPrefab(gameObject))
			{
				return false;
			}
	        
			// 부모가 없지만 씬 루트에 배치됐으면 프리팹 아님
			foreach (var goRoot in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
			{
				if (goRoot == gameObject)
				{
					return false;
				}
			}

			return true;
		}
		
		[Conditional("UNITY_EDITOR")]
		private static void NullLog(string typeName, StackTrace stackTrace, int callDepth = 1)
		{
			var frame = stackTrace.GetFrame(callDepth);
			var log = ZString.Format("{3} Is Null in \"{2}\" (File=\"{0}\", Line={1})", 
				System.IO.Path.GetFileName(frame?.GetFileName() ?? ""), 
				frame?.GetFileLineNumber()??0, 
				frame?.GetMethod()?.Name??"", 
				typeName
			);
			Debug.LogError(log);
		}
		
		public static bool IsNull(this GameObject go, bool showLog = true)
		{
			// FakeNull 이슈
			if (false == go)
			{
				if (showLog)
				{
#if DEBUG
					NullLog("GameObject", new StackTrace(true));
#else
					Debug.LogError("GameObject Is Null");
#endif
				}

				return true;
			}

			return false;
		}
		
		public static bool IsNull(this Object obj, bool showLog = true)
		{
			// FakeNull 이슈
			if (false == obj)
			{
				if (showLog)
				{
#if DEBUG
					NullLog("Object", new StackTrace(true)); ;
#else
					Debug.LogError("Object Is Null");
#endif
				}

				return true;
			}

			return false;
		}
		
		public static bool IsNull(this object obj, bool showLog = true)
		{
			if (null == obj)
			{
				if (showLog)
				{
#if DEBUG
					NullLog("System.Object", new StackTrace(true));
#else
					Debug.LogError("System.Object Is Null");
#endif
				}

				return true;
			}

			return false;
		}
	}
	
	public static class GameObjectExtensions
	{
		private const string TagDestroyed = "Destroyed";
		
		public static void DestroySafe(this GameObject go)
		{
			if (go.IsNull(false))
			{
				return;
			}

			go.tag = TagDestroyed;

			var components = go.GetComponentsInChildren(typeof(IDestroyable));
			foreach (var comp in components)
			{
				if (!(comp is IDestroyable destroyable))
				{
					continue;
				}

				destroyable.OnDestroyNow();
			}

			Object.Destroy(go);
		}

		public static bool IsDestroyed(this GameObject go, bool bShowErrorLog = true)
		{
			if (go.IsNull(bShowErrorLog))
			{
				return true;
			}

			return go.CompareTag(TagDestroyed);
		}
	}
	
	public static class TypeExtensions
	{
		/// <summary>
		/// 구조체 여부 확인
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static bool IsStruct(this Type t) => t.IsValueType && !t.IsPrimitive && !t.IsEnum && t != typeof(Decimal);
	}
	
	public static class CancellationTokenSourceExtensions
	{
		public static void SafeCancel(this CancellationTokenSource cancellationTokenSource)
		{
			if (cancellationTokenSource is { IsCancellationRequested: false })
			{
				cancellationTokenSource.Cancel();
				cancellationTokenSource.Dispose();
			}
			
			cancellationTokenSource = null;
		}
	}
}
