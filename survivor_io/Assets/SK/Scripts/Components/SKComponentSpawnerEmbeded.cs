using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SKComponentSpawnerEmbeded : SKComponentSpawnerBase
    {
	    [SerializeField] private List<SKSpawnInfo> spawnSetting;
	    [SerializeField] private bool repeat;
	    [SerializeField] private float repeatTime;

	    public override void Init()
	    {
		    _spawner.InitData(spawnSetting, repeat, repeatTime, SkObject, OnSpawnFromSpawner, OnRepeatFromSpawner);
	    }
    }
}
