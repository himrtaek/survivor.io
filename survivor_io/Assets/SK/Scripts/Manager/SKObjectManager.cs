using System.Collections;
using System.Collections.Generic;
using JHT.Scripts.Common;
using JHT.Scripts.GameObjectPool;
using JHT.Scripts.ResourceManager;
using UnityEngine;

public class SKObjectManager
{
    public SKObjectPlayer ObjectPlayer { get; private set; }
    
    private Dictionary<SKObjectBase.SKObjectType, Dictionary<long, SKObjectBase>> _skObjectByObjectType = new();
    private List<SKObjectBase> _skObjectListTemp = new();
    private List<SKObjectBase> _readyForDestroyObjectList = new();
    private long _lastSpawnId;

    public void Init()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        var skObject = SpawnObject("Prefab/Player");
        if (skObject.IsNull())
        {
            return;
        }
        
        if (skObject is SKObjectPlayer objectPlayer)
        {
            ObjectPlayer = objectPlayer;
            objectPlayer.AddSupportItemInfo(SKSupportItem.SKSupportItemType.FitnessManual, 1);
        }
    }

    public SKObjectBase SpawnObject(string prefabPath)
    {
        var originalAsset = ResourceManager.Instance.LoadOriginalAsset<GameObject>(prefabPath);
        if (originalAsset.IsNull())
        {
            return null;
        }
        
        return SpawnObject(originalAsset);
    }

    public SKObjectBase SpawnObject(GameObject originalAsset)
    {
        var spawnObject = GameObjectPoolManager.Instance.GetOrNewObject(originalAsset);
        if (spawnObject.IsNull())
        {
            return null;
        }

        if (spawnObject.TryGetComponent(out SKObjectBase skObject).IsFalse())
        {
            return null;
        }
        
        skObject.SetSpawnId(++_lastSpawnId);
        
        if (false ==_skObjectByObjectType.TryGetValue(skObject.ObjectType, out var skObjectBySpawnId))
        {
            skObjectBySpawnId = new();
            _skObjectByObjectType.Add(skObject.ObjectType, skObjectBySpawnId);
        }
        
        skObjectBySpawnId.Add(skObject.SpawnId, skObject);
        
        skObject.ChangeState(SKObjectBase.SKObjectStateType.Spawned);
        
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
            skObject.ChangeState(SKObjectBase.SKObjectStateType.Destroyed);
            skObject.Event.BroadCast(SKEvent.SKEventType.Destroy);
            skObject.gameObject.SetActive(false);
        }
        
        _skObjectListTemp.Clear();
    }

    public void DestroyObject(SKObjectBase skObject)
    {
        skObject.ChangeState(SKObjectBase.SKObjectStateType.ReadyForDestroyed);
        _readyForDestroyObjectList.Add(skObject);
    }
}
