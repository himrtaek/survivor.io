using System.Collections;
using System.Collections.Generic;
using JHT.Scripts.GameObjectPool;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PoolableObject))]
[DisallowMultipleComponent]
public abstract class SKObjectBase : MonoBehaviour
{
    public enum SKObjectType
    {
        Player,
        PlayerWeapon,
        Monster,
        MonsterWeapon,
        Spawner,
        Deco,
    }

    public abstract SKObjectType ObjectType { get; }

    public enum SKObjectStateType
    {
        None,
        ReadyForSpawn,
        Spawned,
        ReadyForDestroyed,
        Destroyed
    }
    
    public SKObjectStateType ObjectState { get; private set; }

    public void ChangeState(SKObjectStateType objectStateType)
    {
        var beforeType = ObjectState;
        ObjectState = objectStateType;
        
        var eventParam = SKEventParam.GetOrNewParam<SKObjectStateChangeEventParam>();
        eventParam.BeforeType = beforeType;
        eventParam.AfterType = ObjectState;
        Event.BroadCast(SKEvent.SKEventType.StateChange, eventParam);
    }
    
    public long  SpawnId { get; private set; }

    public void SetSpawnId(long spawnId)
    {
        SpawnId = spawnId;
    }

    public SKStatManager StatManager { get; } = new();
    public SKEvent Event { get; } = new();
    public UnityEvent<float> OnGameUpdate { get; } = new();

    public void GameUpdate(float deltaTime)
    {
        OnGameUpdate.Invoke(deltaTime);
    }

    public void DestroyObject()
    {
        SKGameManager.Instance.ObjectManager.DestroyObject(this);
    }
}
