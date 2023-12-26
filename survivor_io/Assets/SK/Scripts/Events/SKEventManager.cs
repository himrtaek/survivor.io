using System;
using System.Collections.Generic;


namespace SK
{
	public class SKEventManager
	{
		#region 후처리 이벤트
	    /// <summary>
	    /// 후처리 이벤트 타입
	    /// </summary>
	    private enum AfterSKEventType : int
	    {
	        Add = 0,
			Remove,
	    };

	    /// <summary>
	    /// 후처리 이벤트 정보
	    /// </summary>
	    class AfterSKEventInfo
	    {
	        // 후처리 타입
	        public AfterSKEventType AfterEventType;
			// N2EventTarget
			public SKEventType EventType;
	        // 콜백 리스트
	        public List<Delegate> CallbackList = new List<Delegate>();

	        public AfterSKEventInfo(AfterSKEventType type, SKEventType eventType, Delegate callback)
	        {
		        AfterEventType = type;
		        EventType = eventType;
	            CallbackList.Add(callback);
			}
	    }
		
	    /// <summary>
	    /// Broadcast 호출 횟수
	    /// StartBroadcast 에서 증가 EndBroadcast 감소 시킨후 
	    /// CallBroadcastCount 가 0일 경우 AfterEventInfoList 에 저장된 이벤트 정보를 이벤트 컨테이너에 등록시킴
	    /// </summary>
	    private int _callBroadcastCount;

	    /// <summary>
	    /// 후처리할 이벤트 정보 리스트
	    /// </summary>
	    private List<AfterSKEventInfo> _afterEventInfoList = new ();
		#endregion
		
		private SKObject _skObject;

		public SKEventManager(SKObject skObject)
		{
			_skObject = skObject;
		}
		
		public enum SKEventType
		{
			Spawn,
			Destroy,
			StateChange,
			Damage,
			Attack,
			DropItemCapture,
			ChangeMoveDirection,
			ReadyForSpawn,
			Kill,
			HPZero,
			SpawnChildObject,
			Heal,
		}

		private Dictionary<SKEventType, List<Delegate>> _eventByEventType = new();

		public void AddListener(SKEventType eventType, Action<SKObject, SKEventParam> callback)
		{
			if (IsBroadcast())
			{
				AddAfterEventInfo(eventType, callback);
				return;
			}
			
			if (false == _eventByEventType.TryGetValue(eventType, out var delegateList))
			{
				delegateList = new();
				_eventByEventType.Add(eventType, delegateList);
			}
        
			delegateList.Add(callback);
		}

		public void RemoveListener(SKEventType eventType, Action<SKObject, SKEventParam> callback)
		{
			if (IsBroadcast())
			{
				RemoveAfterEventInfo(eventType, callback);
				return;
			}

			if (false == _eventByEventType.TryGetValue(eventType, out var delegateList))
			{
				return;
			}
        
			delegateList.Remove(callback);
		}

		public void BroadCast(SKEventType eventType, SKEventParam eventParam = null, bool eventParamReturn = true)
		{
			StartBroadcast();
			
			if (_eventByEventType.TryGetValue(eventType, out var delegateList))
			{
				foreach (var it in delegateList)
				{
					if (it is Action<SKObject, SKEventParam> callback)
						callback(_skObject, eventParam);
				}
			}
			
			if (eventParamReturn && null != eventParam)
			{
				SKEventParam.Return(eventParam);
			}
			
			EndBroadcast();
		}

		private void AddAfterEventInfo(SKEventType eventType, Delegate callback)
		{
			foreach (AfterSKEventInfo info in _afterEventInfoList)
			{
				if (info.AfterEventType == AfterSKEventType.Add && info.EventType == eventType)
				{
					info.CallbackList.Add(callback);
					return;
				}
			}
			
			_afterEventInfoList.Add(new AfterSKEventInfo(AfterSKEventType.Add, eventType, callback));
		}

		private void RemoveAfterEventInfo(SKEventType eventType, Delegate callback)
		{
			foreach (AfterSKEventInfo info in _afterEventInfoList)
			{
				if (info.AfterEventType == AfterSKEventType.Remove && info.EventType == eventType)
				{
					info.CallbackList.Add(callback);
					return;
				}
			}
			
			_afterEventInfoList.Add(new AfterSKEventInfo(AfterSKEventType.Remove, eventType, callback));
		}
		
		private bool IsBroadcast()
		{
			return _callBroadcastCount > 0;
		}

		private void StartBroadcast()
        {
            ++_callBroadcastCount;
        }

		private void EndBroadcast()
        {
            --_callBroadcastCount;

            // Broadcast 가 끝이 났을 경우 이벤트 후처리 실행
            if (IsBroadcast() == false)
            {
                foreach (AfterSKEventInfo info in _afterEventInfoList)
                {
                    // 추가
                    if (info.AfterEventType == AfterSKEventType.Add)
                    {
                        foreach (Delegate del in info.CallbackList)
                        {
                            AddListener(info.EventType, del as Action<SKObject, SKEventParam>);
                        }
                    }
					// N2Event 추가
                    else if (info.AfterEventType == AfterSKEventType.Remove)
                    {
	                    foreach (Delegate del in info.CallbackList)
	                    {
		                    RemoveListener(info.EventType, del as Action<SKObject, SKEventParam>);
	                    }
                    }
                }

                _afterEventInfoList.Clear();
            }
        }
	}	
}
