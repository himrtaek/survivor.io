using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class SKObject : MonoBehaviour
{
    public enum SKObjectType
    {
        Player,
        PlayerWeapon,
        Monster,
        MonsterWeapon,
        Deco,
    }

    [SerializeField] private SKObjectType _skObjectType;
    public SKObjectType ObjectType => _skObjectType;
    
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
