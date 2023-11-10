using JHT.Scripts.Common;
using JHT.Scripts.ResourceManager;
using UnityEditor;
using UnityEngine;

namespace JHT.Scripts
{
	public class AppMain : Singleton<AppMain>
	{
		public enum InitStepType
		{
			None,
			SubsystemRegistration,
			AfterAssembliesLoaded,
			BeforeSplashScreen,
			BeforeSceneLoad,
			AfterSceneLoad
		}

		private int _mainThreadId = int.MinValue;

		public static string AppStartSceneName { get; private set; } = "";
		public static InitStepType InitStep = InitStepType.None;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		public static void OnSubsystemRegistration()
		{
			Debug.Log("=== [OnSubsystemRegistration Start] ===");
		
			Debug.Log("=== [OnSubsystemRegistration Finish] ===");
		
			InitStep = InitStepType.SubsystemRegistration;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		public static void OnAfterAssembliesLoaded()
		{
			Debug.Log("=== [OnAfterAssembliesLoaded Start] ===");
		
			Debug.Log("=== [OnAfterAssembliesLoaded Finish] ===");
		
			InitStep = InitStepType.AfterAssembliesLoaded;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		public static void OnBeforeSplashScreen()
		{
			Debug.Log("=== [OnBeforeSplashScreen Start] ===");
		
			Debug.Log("=== [OnBeforeSplashScreen Finish] ===");
		
			InitStep = InitStepType.BeforeSplashScreen;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void BeforeSceneLoad()
		{
			Debug.Log("=== [BeforeSceneLoad Start] ===");
		
			AppStartSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

			// AppMain 초기화
			Debug.Log("=== [AppMain.Init Start] ===");
			AppMain.Instance.Init();
			Debug.Log("=== [AppMain.Init End] ===");

			Debug.Log("=== [BeforeSceneLoad Finish] ===");
		
			InitStep = InitStepType.BeforeSceneLoad;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void OnAfterSceneLoad()
		{	
			Debug.Log("=== [OnAfterSceneLoad Start] ===");

			Debug.Log("=== [OnAfterSceneLoad Finish] ===");
		
			InitStep = InitStepType.AfterSceneLoad;
		}

		private void Init()
		{
			_mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
		
			// 슬립 모드 방지
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			// 이모지 로드
			GText.LoadEmojiInfo();
		
			// 리소스 로딩 정보를 남김
#if UNITY_EDITOR
			ResourceManager.ResourceManager.Instance.BindOnPostLoad(OnPostResourceLoad);
#endif
		}

		/// <summary>
		/// 현재 실행되고 있는 쓰래드가 매인 쓰래드인지 반환
		/// </summary>
		/// <returns></returns>
		public bool IsMainThread()
		{
			return _mainThreadId == int.MinValue || _mainThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId;
		}

		private void OnPostResourceLoad(Object objAsset, Object objInstance)
		{
#if UNITY_EDITOR
			var goAsset = objAsset as GameObject;
			var goInstance = objInstance as GameObject;
			if (false == goAsset.IsNull(false) && false == goInstance.IsNull(false))
			{
				goInstance.DoRecursiveAction(goChild =>
				{
					if (false == goChild.TryGetComponent(out ResourceLoadInfo comp))
					{
						comp = goChild.AddComponent<ResourceLoadInfo>();
					}

					if (comp.rootObject == null)
					{
						comp.prefabObject = objAsset;
						comp.prefabPath = objAsset.IsNull(false) ? "" : AssetDatabase.GetAssetPath(objAsset);
						comp.rootObject = objInstance;
					}
				});
			}
#endif
		}
	}
}
