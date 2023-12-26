using System;
using System.Collections.Generic;
using Cysharp.Text;
using JHT.Scripts.Common;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;

namespace JHT.Scripts
{
	public enum SceneType
	{
		Begin,
		
		SKGameScene,

		End,
	}

	public class SceneManager : Common.Singleton<SceneManager>
	{
		private BaseScene _curScene;
		public BaseScene CurScene
		{
			get
			{
#if UNITY_EDITOR
				if (AppMain.InitStep < AppMain.InitStepType.BeforeSceneLoad)
				{
					Debug.LogError("=== [호출 순서 오류, 초기화 후 진행하세요] ===");
				}
#endif

				if (true == _curScene.IsNull(false))
				{
					foreach (var it in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
					{
						if (true == it.TryGetComponent(out BaseScene scene))
						{
							_curScene = scene;
							break;
						}
					}
				}

				return _curScene;
			}
			set
			{
				_curScene = value;
			}
		}

		public SceneType PrevSceneType;
		public SceneType CurSceneType;

		public SceneType _changeSceneType = SceneType.Begin;
		public SceneType ChangeSceneType => _changeSceneType;

		protected string GetScenePath(SceneType sceneType)
		{
			switch (sceneType)
			{
				case SceneType.SKGameScene:
					return ZString.Format("Scene_SKGame/{0}.unity", sceneType.ToStringCached());

			}

			return ZString.Format("{0}.unity", sceneType.ToStringCached());
		}

		public enum OnAwakeEventType
		{
			AwakeBegin,
			Awake,
			AwakeEnd,
		}

		public enum OnStartEventType
		{
			StartBegin,
			Start,
			StartEnd,
		}

		public struct stOnAwakeWork
		{
			public SceneType SceneType;
			public OnAwakeEventType EventType;
			public Action Action;
		}

		public struct stOnStartWork
		{
			public SceneType SceneType;
			public OnStartEventType EventType;
			public Action Action;
		}

		List<stOnAwakeWork> _listOnAwakeWork = new List<stOnAwakeWork>();
		List<stOnStartWork> _listOnStartWork = new List<stOnStartWork>();

		public void Clear()
		{
			_listOnAwakeWork.Clear();
			_listOnStartWork.Clear();
		}

		public void AddOnAwakeWorkOnce(SceneType sceneType, OnAwakeEventType eventType, Action action)
		{
			_listOnAwakeWork.Add(new stOnAwakeWork()
			{
				SceneType = sceneType,
				EventType = eventType,
				Action = action
			});
		}

		public void AddOnStartWork(SceneType sceneType, OnStartEventType eventType, Action action)
		{
			_listOnStartWork.Add(new stOnStartWork()
			{
				SceneType = sceneType,
				EventType = eventType,
				Action = action
			});
		}

		public void OnAwakeWork(SceneType sceneType, OnAwakeEventType eEventType)
		{
			int idx = 0;
			while (idx < _listOnAwakeWork.Count)
			{
				if (_listOnAwakeWork[idx].SceneType == sceneType && _listOnAwakeWork[idx].EventType == eEventType)
				{
					var action = _listOnAwakeWork[idx].Action;
					_listOnAwakeWork.RemoveAt(idx);

					action?.Invoke();
				}
				else
				{
					++idx;
				}
			}
		}

		public void OnStartWork(SceneType sceneType, OnStartEventType eEventType)
		{
			int idx = 0;
			while (idx < _listOnStartWork.Count)
			{
				if (_listOnStartWork[idx].SceneType == sceneType && _listOnStartWork[idx].EventType == eEventType)
				{
					var action = _listOnStartWork[idx].Action;
					_listOnStartWork.RemoveAt(idx);

					action?.Invoke();
				}
				else
				{
					++idx;
				}
			}
		}

		public string GetCurSceneName()
		{
			return CurScene.SceneName;
		}

		public SceneType GetCurSceneType()
		{
			return CurSceneType;
		}

		public SceneType GetPrevSceneType()
		{
			return PrevSceneType;
		}

		public T GetCurScene<T>() where T : BaseScene
		{
			return CurScene as T;
		}

		public void ReloadScene()
		{
			ChangeScene(CurSceneType, true, true, isReload:true);
		}

		public void ChangeScene(SceneType eSceneType, bool IsLoadingScene = true, bool isForceChange = false, bool isReload = false)
		{
			ChangeSceneInternal(eSceneType, IsLoadingScene, isForceChange, isReload);
		}

		private void ChangeSceneInternal(SceneType eSceneType, bool IsLoadingScene = true, bool isForceChange = false, bool isReload = false)
		{
			Debug.Log($"{CurSceneType.ToStringCached()} => SceneManager.ChangeScene(eSceneType:{eSceneType.ToStringCached()}, IsLoadingScene:{IsLoadingScene.ToStringCached()}, isForceChange:{isForceChange.ToStringCached()})");

			_changeSceneType = eSceneType;

			// 끝 처리
			{
				PrevSceneType = CurSceneType;
			}

			// 캐싱 클리어
			StringPerformanceExtension.ClearNumericCache();
			EnumPerformanceExtension.ClearValuesCache();
			YieldCache.ClearCache();

			try
			{
				var curScene = GetCurScene<BaseScene>();
				curScene.OnEndScene(ChangeSceneType);
				curScene.CheckInitState(BaseScene.InitStateType.EndDone);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
