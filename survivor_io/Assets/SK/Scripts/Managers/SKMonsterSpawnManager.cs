using System.Collections.Generic;
using JHT.Scripts.ResourceManager;
using UnityEngine;

namespace SK
{
    public class SKMonsterSpawnManager
    {
	    public bool IsAllSpawned => _isAllSpawned;
	    private bool _isAllSpawned;
	    
	    private SKMonsterSpawner _spawner = new();
	    public void Init(uint stageId)
	    {
		    InitStage(stageId);
	    }

	    private void InitStage(uint stageId)
	    {
		    var spawnSetting =
			    ResourceManager.Instance.LoadOriginalAsset<SKMonsterSpawnSetting>(
				    SKMonsterSpawnSetting.GetFilePathByStageId(stageId),
				    useCache: false, saveCache: false);

		    _spawner.InitData(spawnSetting.SpawnInfos);
	    }

	    public void GameUpdate(float deltaTime)
	    {
		    _spawner.GameUpdate(deltaTime);

		    if (_spawner.IsAllSpawned)
		    {
			    _isAllSpawned = true;
		    }
	    }
    }
}
