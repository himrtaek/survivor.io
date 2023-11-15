using System;
using System.Collections.Generic;

public abstract class SKEventParam
{
    private static Dictionary<Type, Queue<SKEventParam>> _eventParamPoolByType = new();
    
    public static T GetOrNewParam<T>() where T : SKEventParam
    {
        var paramType = typeof(T);
        if (false == _eventParamPoolByType.TryGetValue(paramType, out var eventParamPool))
        {
            eventParamPool = new();
            _eventParamPoolByType.Add(paramType, eventParamPool);
        }
        
        if (eventParamPool.TryDequeue(out var result))
        {
            return result as T;
        }

        return Activator.CreateInstance(paramType) as T;
    }

    public static void Return(SKEventParam eventParam)
    {
        var paramType = eventParam.GetType();
        if (false == _eventParamPoolByType.TryGetValue(paramType, out var eventParamPool))
        {
            eventParamPool = new();
            _eventParamPoolByType.Add(paramType, eventParamPool);
        }
        
        eventParamPool.Enqueue(eventParam);
    }

    public abstract void Reset();
}

public class SKSpawnEventParam : SKEventParam
{
    public long SpawnId;
    
    public override void Reset()
    {
        SpawnId = 0;
    }
}

public class SKObjectStateChangeEventParam : SKEventParam
{
    public SKObjectBase.SKObjectStateType BeforeType;
    public SKObjectBase.SKObjectStateType AfterType;
    
    public override void Reset()
    {
        BeforeType = SKObjectBase.SKObjectStateType.None;
        AfterType = SKObjectBase.SKObjectStateType.None;
    }
}
