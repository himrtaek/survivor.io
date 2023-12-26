using UnityEngine;

namespace SK
{
    public class SKComponentSpawner : SKComponentSpawnerBase
    {
	    [SerializeField] private SKSpawnSetting spawnSetting;

	    public override void Init()
	    {
		    _spawner.InitData(spawnSetting, SkObject, OnSpawnFromSpawner, OnRepeatFromSpawner);
	    }
    }
}
