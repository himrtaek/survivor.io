using System.Collections.Generic;
using JHT.Scripts.Common;
using UnityEngine;

namespace SK
{
    public class SKComponentPlayerWeaponSpawner : SKComponentSpawnerCommon
    {
	    [SerializeField] protected bool manualInit;
	    [SerializeField] protected bool destroyOnAllSpawn;
	    [SerializeField] protected bool destroyAllSpawnObjectOnRepeat;
	    [SerializeField] protected bool destroyAllSpawnObjectOnDestroy;
	    [SerializeField] private List<SKPlayerWeaponSpawnInfo> spawnInfoList;

	    private List<SKPlayerWeaponSpawner> _spawnerList = new();
	    private bool _init;

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
		    foreach (var spawner in _spawnerList)
		    {
			    spawner.Clear();
		    }
		    
		    _spawnerList.Clear();

		    _init = false;
		    
		    base.OnSKObjectDestroy();
	    }

	    public override void Init()
	    {
		    if (SkObject is iSKObjectLevel objectLevel)
		    {
			    _init = true;

			    var level = objectLevel.Level;
			    foreach (var spawnInfo in spawnInfoList)
			    {
				    uint spawnCount = spawnInfo.spawnCount;
				    var spawnStartDelay = spawnInfo.spawnStartDelay;
				    var spawnDuration = spawnInfo.spawnDuration;
				    var spawnRepeat = spawnInfo.spawnRepeat;
				    var spawnRepeatCoolTime = spawnInfo.spawnRepeatCoolTime;
				    var affectWeaponRange = spawnInfo.affectWeaponRange;

				    SKPlayerWeaponSpawner spawner = new(level);
				    spawner.InitData(spawnInfo, spawnCount, spawnStartDelay, spawnDuration, spawnRepeat, spawnRepeatCoolTime, SkObject, destroyAllSpawnObjectOnRepeat, destroyAllSpawnObjectOnDestroy, affectWeaponRange);
				    _spawnerList.Add(spawner);
			    }
		    }
	    }

	    public override void GameUpdate(float deltaTime)
	    {
		    base.GameUpdate(deltaTime);
		    
		    SpawnerUpdate(deltaTime);
	    }

	    public override void SpawnerUpdate(float deltaTime)
	    {
		    if (false == _init)
		    {
			    return;
		    }

		    var isAllSpawned = true;
		    foreach (var spawner in _spawnerList)
		    {
			    spawner.GameUpdate(deltaTime);

			    if (false == spawner.IsAllSpawned)
			    {
				    isAllSpawned = false;
			    }
		    }
		    
		    if (destroyOnAllSpawn && isAllSpawned)
		    {
			    SkObject.DestroyObject();
		    }
	    }

	    public SKPlayerWeaponSpawner GetSpawner(uint index)
	    {
		    if (_spawnerList.Count <= index)
		    {
			    return null;
		    }

		    return _spawnerList[(int)index];
	    }
    }
}
