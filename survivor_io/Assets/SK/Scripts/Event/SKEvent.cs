using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SKEvent
{
    public enum SKEventType
    {
        Spawn,
        Destroy,
        StateChange,
    }

    private Dictionary<SKEventType, UnityEvent<SKEventParam>> _eventByEventType = new();

    public void AddListener(SKEventType eventType, UnityAction<SKEventParam> unityAction)
    {
        if (false == _eventByEventType.TryGetValue(eventType, out var unityEvent))
        {
            unityEvent = new();
            _eventByEventType.Add(eventType, unityEvent);
        }
        
        unityEvent.AddListener(unityAction);
    }

    public void RemoveListener(SKEventType eventType, UnityAction<SKEventParam> unityAction)
    {
        if (false == _eventByEventType.TryGetValue(eventType, out var unityEvent))
        {
            return;
        }
        
        unityEvent.RemoveListener(unityAction);
    }

    public void BroadCast(SKEventType eventType, SKEventParam eventParam = null, bool eventParamReturn = true)
    {
        if (false == _eventByEventType.TryGetValue(eventType, out var unityEvent))
        {
            return;
        }
        
        unityEvent.Invoke(eventParam);
        
        if (eventParamReturn && null != eventParam)
        {
            SKEventParam.Return(eventParam);
        }
    }
}
