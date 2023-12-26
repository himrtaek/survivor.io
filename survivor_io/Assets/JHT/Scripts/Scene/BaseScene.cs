using System;
using JHT.Scripts.Common;
using JHT.Scripts.Common.PerformanceExtension;
using JHT.Scripts.GameObjectPool;
using UnityEngine;

namespace JHT.Scripts
{
	[DefaultExecutionOrder(-5000)]
	public class BaseScene : MonoBehaviour
	{
		public enum InitStateType
		{
			None,

			AwakeBeginDone,
			AwakeDone,
			AwakeEndDone,
			StartBeginDone,
			StartDone,
			StartEndDone,
			EndBegin,
			EndDone,
		}

		public InitStateType InitState { get; private set; }

		[SerializeField] private GameObject uiRoot;
		public GameObject UIRoot => uiRoot; 

		private string m_sSceneName_;

		public string SceneName
		{
			get
			{
				if (true == string.IsNullOrEmpty(m_sSceneName_))
				{
					m_sSceneName_ = GetType().Name;
				}

				return m_sSceneName_;
			}
		}

		private SceneType m_eSceneType_ = SceneType.End;
		public SceneType SceneType
		{
			get
			{
				if (SceneType.End == m_eSceneType_)
				{
					if (true == Enum.TryParse(SceneName, out SceneType sceneType))
					{
						m_eSceneType_ = sceneType;
					}
				}

				return m_eSceneType_;
			}
		}

		[SerializeField] private Camera mainCamera;
		public Camera MainCamera
		{
			get
			{
				if (true == mainCamera.IsNull(false))
				{
					mainCamera = Camera.main;
				}
				
				return mainCamera;
			}
		}

		[SerializeField] private Camera uiCamera;
		public Camera UICamera => uiCamera;
		
		public bool StopBgmOnDestroy { get; set; } = true;
		
		protected virtual void Awake()
		{
			try
			{
				SceneManager.Instance.CurSceneType = SceneType;
				SceneManager.Instance.CurScene = this;

				OnAwakeBegin();
				CheckInitState(InitStateType.AwakeBeginDone);

				OnAwake();
				CheckInitState(InitStateType.AwakeDone);

				OnAwakeEnd();
				CheckInitState(InitStateType.AwakeEndDone);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		protected virtual void OnAwakeBegin()
		{
			try
			{
				SceneManager.Instance.OnAwakeWork(SceneType, SceneManager.OnAwakeEventType.AwakeBegin);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				InitState = InitStateType.AwakeBeginDone;
			}
		}

		protected virtual void OnAwake()
		{
			try
			{
				SceneManager.Instance.OnAwakeWork(SceneType, SceneManager.OnAwakeEventType.Awake);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				InitState = InitStateType.AwakeDone;
			}
		}

		protected virtual void OnAwakeEnd()
		{
			try
			{
				SceneManager.Instance.OnAwakeWork(SceneType, SceneManager.OnAwakeEventType.AwakeEnd);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				InitState = InitStateType.AwakeEndDone;
			}
		}

		protected virtual void OnApplicationPause(bool pause)
		{
			Debug.Log($"OnApplicationPause(pause:{pause.ToStringCached()})");
		}

		protected virtual void OnApplicationFocus(bool focus)
		{
			Debug.Log($"OnApplicationFocus(focus:{focus.ToStringCached()})");
		}

		protected virtual void OnApplicationQuit()
		{
			Debug.Log("BaseScene.OnApplicationQuit");
			
			if (AppMain.IsQuit)
			{
				Debug.LogWarning($"BaseScene => 이미 OnApplicationQuit이 호출되었습니다");
			}

			AppMain.OnApplicationQuit();
		}

		public void CheckInitState(InitStateType initState)
		{
			if (InitState != initState)
			{
				Debug.LogError("InitStateType 오류! 부모 함수 호출이 안불린게 의심됩니다");
				InitState = initState;
			}
		}

		protected virtual void Start()
		{
			try
			{
				OnStartBegin();
				CheckInitState(InitStateType.StartBeginDone);
				
				OnStart();
				CheckInitState(InitStateType.StartDone);
				
				OnStartEnd();
				CheckInitState(InitStateType.StartEndDone);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		protected virtual void OnStartBegin()
		{
			try
			{
				SceneManager.Instance.OnStartWork(SceneType, SceneManager.OnStartEventType.StartBegin);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				InitState = InitStateType.StartBeginDone;
			}
		}

		protected virtual void OnStart()
		{
			try
			{
				SceneManager.Instance.OnStartWork(SceneType, SceneManager.OnStartEventType.Start);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				InitState = InitStateType.StartDone;
			}
		}

		protected virtual void OnStartEnd()
		{
			try
			{
				SceneManager.Instance.OnStartWork(SceneType, SceneManager.OnStartEventType.StartEnd);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				InitState = InitStateType.StartEndDone;
			}
		}

		protected virtual void Update()
		{
			
		}

		// 다른씬으로 변경 될 떄
		public virtual void OnEndScene(SceneType nextScene)
		{
			InitState = InitStateType.EndBegin;

			try
			{
				GameObjectPoolManager.Instance.Clear();
				ResourceManager.ResourceManager.Instance.Clear();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				InitState = InitStateType.EndDone;
			}
		}
	}
}
