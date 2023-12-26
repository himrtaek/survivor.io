using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SK
{
    public class SKComponentSpawnerRandom : SKComponentSpawnerBase
    {
	    [Serializable]
	    public class SKRandomSpawnSetting
	    {
		    public SKSpawnSetting SpawnSetting;
		    public float Weight;
	    }
	    
	    [SerializeField] private List<SKRandomSpawnSetting> spawnSettingList;
	    [SerializeField] private bool randomPerRepeat;

	    public override void Init()
	    {
		    var index = ChooseRandom();
		    _spawner.InitData(spawnSettingList[index].SpawnSetting, SkObject, OnSpawnFromSpawner, OnRepeatFromSpawner);
	    }
	    
	    private int ChooseRandom ()
	    {
		    float total = 0;
		    foreach (var elem in spawnSettingList)
		    {
			    total += elem.Weight;
		    }

		    float randomPoint = Random.value * total;

		    for (int i= 0; i < spawnSettingList.Count; i++)
		    {
			    if (randomPoint < spawnSettingList[i].Weight)
			    {
				    return i;
			    }

			    randomPoint -= spawnSettingList[i].Weight;
		    }

		    return spawnSettingList.Count - 1;
	    }
	    
	    protected override void OnRepeatFromSpawner()
	    {
		    base.OnRepeatFromSpawner();

		    if (randomPerRepeat)
		    {
			    Init();
		    }
	    }
    }
}
