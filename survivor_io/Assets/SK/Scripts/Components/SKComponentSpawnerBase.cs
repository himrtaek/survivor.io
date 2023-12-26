using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public abstract class SKComponentSpawnerBase : SKComponentSpawnerCommon
    {
	    [SerializeField] protected bool manualInit;
	    [SerializeField] protected bool destroyOnAllSpawn;
	    [SerializeField] protected bool destroyAllSpawnObjectOnRepeat;
	    
	    protected SKSpawner _spawner = new();
	    private List<SKObject> _spawnedSkObjectList = new();

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    if (false == manualInit)
		    {
			    Init();   
		    }
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    OnRepeatFromSpawner();
		    _spawner.Clear();
		    
		    base.OnSKObjectDestroy();
	    }

	    private void OnSpawnedObjectSKEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    if (eventParam.EventType == SKEventManager.SKEventType.Destroy)
		    {
			    _spawnedSkObjectList.Remove(skObject);
			    skObject.EventManager.RemoveListener(SKEventManager.SKEventType.Destroy, OnSpawnedObjectSKEvent);
		    }
	    }

	    protected void OnSpawnFromSpawner(SKObject skObject)
	    {
		    if (destroyAllSpawnObjectOnRepeat)
		    {
			    skObject.EventManager.AddListener(SKEventManager.SKEventType.Destroy, OnSpawnedObjectSKEvent);
			    _spawnedSkObjectList.Add(skObject);   
		    }
	    }
	    
	    protected virtual void OnRepeatFromSpawner()
	    {
		    if (destroyAllSpawnObjectOnRepeat)
		    {
			    foreach (var spawnedSKObject in _spawnedSkObjectList)
			    {
				    spawnedSKObject.EventManager.RemoveListener(SKEventManager.SKEventType.Destroy, OnSpawnedObjectSKEvent);
				    spawnedSKObject.DestroyObject();
			    }
			    
			    _spawnedSkObjectList.Clear();
		    }
	    }

	    public override void GameUpdate(float deltaTime)
	    {
		    base.GameUpdate(deltaTime);
		    
		    SpawnerUpdate(deltaTime);
	    }

	    public override void SpawnerUpdate(float deltaTime)
	    {
		    _spawner.GameUpdate(deltaTime);

		    if (destroyOnAllSpawn && _spawner.IsAllSpawned)
		    {
			    SkObject.DestroyObject();
		    }
	    }
    }
}
