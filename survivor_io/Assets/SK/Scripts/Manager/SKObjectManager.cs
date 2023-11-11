using System.Collections;
using System.Collections.Generic;
using JHT.Scripts.Common;
using JHT.Scripts.GameObjectPool;
using UnityEngine;

public class SKObjectManager
{
    private Dictionary<SKObject.SKObjectType, Dictionary<long, SKObject>> _skObjectByObjectType = new();
    private List<SKObject> _skObjectListTemp = new();
    private List<SKObject> _readyForDestroyObjectList = new();
    private long _lastSpawnId;

    public void Init()
    {
        
    }
    
    public SKObject SpawnObject(GameObject originalAsset)
    {
        var spawnObject = GameObjectPoolManager.Instance.GetOrNewObject(originalAsset);
        if (spawnObject.IsNull())
        {
            return null;
        }

        if (spawnObject.TryGetComponent(out SKObject skObject).IsFalse())
        {
            return null;
        }
        
        skObject.SetSpawnId(++_lastSpawnId);
        
        skObject.ChangeState(SKObject.SKObjectStateType.Spawned);
        
        var spawnEventParam = SKEventParam.GetOrNewParam<SKSpawnEventParam>();
        spawnEventParam.SpawnId = _lastSpawnId;
        skObject.Event.BroadCast(SKEvent.SKEventType.Spawn, spawnEventParam);

        return skObject;
    }

    public void GameUpdate(float deltaTime)
    {
        foreach (var it in _skObjectByObjectType)
        {
            foreach (var it2 in it.Value)
            {
                _skObjectListTemp.Add(it2.Value);
            }
        }
        
        foreach (var skObject in _skObjectListTemp)
        {
            skObject.GameUpdate(deltaTime);
        }
        
        _skObjectListTemp.Clear();

        foreach (var skObject in _readyForDestroyObjectList)
        {
            _skObjectListTemp.Add(skObject);
        }
        
        foreach (var skObject in _skObjectListTemp)
        {
            skObject.ChangeState(SKObject.SKObjectStateType.Destroyed);
            skObject.Event.BroadCast(SKEvent.SKEventType.Destroy);
            skObject.gameObject.SetActive(false);
        }
        
        _skObjectListTemp.Clear();
    }

    public void DestroyObject(SKObject skObject)
    {
        skObject.ChangeState(SKObject.SKObjectStateType.ReadyForDestroyed);
        _readyForDestroyObjectList.Add(skObject);
    }
}
