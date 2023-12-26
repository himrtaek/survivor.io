using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SK
{
    public class SKMonsterSpawner
    {
	    struct MonsterSpawnInfForSpawnTime
	    {
		    public GameObject GameObject;
		    public uint SpawnTime;
		    public float SpawnCount;
		    public SKMonsterSpawnInfo.SpawnPositionConstraintType PositionConstraint;
		    public List<SKMonsterSpawnInfo.SKMonsterSpawnStat> StatList;
		    public SKMonsterSpawnInfo.SKMonsterSpawnOverrideInfo OverrideInfo;
	    }
	    
	    public bool IsAllSpawned => _isAllSpawned;
	    private bool _isAllSpawned;
	    
	    private bool _init;
	    
	    private float _elapsedTime;
	    private float _maxSpawnTime;
	    private List<SKMonsterSpawnInfo> _monsterSpawnInfo;
	    private List<MonsterSpawnInfForSpawnTime> _monsterSpawnInfoTempList = new();
	    
	    public void InitData(List<SKMonsterSpawnInfo> monsterSpawnInfo)
	    {
		    _init = true;
		    _elapsedTime = 0;
		    _monsterSpawnInfo = monsterSpawnInfo;
		    _monsterSpawnInfoTempList.Clear();
		    _maxSpawnTime = _monsterSpawnInfo.Max(it => it.spawnEndSecond);
	    }

	    public void GameUpdate(float deltaTime)
	    {
		    if (false == _init)
		    {
			    return;
		    }
		    
		    var prevElapsedTime = _elapsedTime; 
		    _elapsedTime += deltaTime;

		    foreach (var spawnInfo in _monsterSpawnInfo)	
		    {
			    if (spawnInfo.spawnStartSecond < prevElapsedTime)
			    {
				    continue;
			    }
			    
			    if (_elapsedTime < spawnInfo.spawnStartSecond)
			    {
				    continue;
			    }

			    for (uint i = spawnInfo.spawnStartSecond; i <= spawnInfo.spawnEndSecond; i++)
			    {
				    _monsterSpawnInfoTempList.Add(new ()
				    {
					    GameObject = spawnInfo.gameObject,
					    SpawnTime = i,
					    SpawnCount = spawnInfo.spawnCountPerSecond,
					    PositionConstraint = spawnInfo.positionConstraint,
					    StatList = spawnInfo.statList
				    });
			    }
		    }
		    
		    foreach (var spawnInfo in _monsterSpawnInfoTempList)	
		    {
			    if (spawnInfo.SpawnTime < prevElapsedTime)
			    {
				    continue;
			    }
			    
			    if (_elapsedTime < spawnInfo.SpawnTime)
			    {
				    continue;
			    }

			    for (int i = 0; i < spawnInfo.SpawnCount; i++)
			    {
				    Spawn(spawnInfo.GameObject, spawnInfo.PositionConstraint, spawnInfo.StatList, spawnInfo.OverrideInfo);
			    }
		    }

		    _isAllSpawned = _maxSpawnTime <= _elapsedTime;
	    }

	    public static SKObject Spawn(GameObject gameObjectAsset, SKMonsterSpawnInfo.SpawnPositionConstraintType positionConstraintType, List<SKMonsterSpawnInfo.SKMonsterSpawnStat> statList = null, SKMonsterSpawnInfo.SKMonsterSpawnOverrideInfo overrideInfo = null)
	    {
		    return SKGameManager.Instance.ObjectManager.SpawnObject(gameObjectAsset, null, false, onSpawnBefore:
			    (skObjectTemp) =>
			    {
				    OnSpawn(skObjectTemp, positionConstraintType, statList, overrideInfo);
			    });
	    }

	    public static void OnSpawn(SKObject skObjectTemp, SKMonsterSpawnInfo.SpawnPositionConstraintType positionConstraintType, List<SKMonsterSpawnInfo.SKMonsterSpawnStat> statList = null, SKMonsterSpawnInfo.SKMonsterSpawnOverrideInfo overrideInfo = null)
	    {
		    if (null != statList)
		    {
			    foreach (var buff in statList)
			    {
				    skObjectTemp.StatManager.AddStatData(buff.StatType, buff.StatExprType,
					    StatSourceType.Spawner, 0, buff.StatValue);
			    }
		    }

		    var position = positionConstraintType.CalcPosition();
		    position = SKGameManager.Instance.BackgroundManager.BackgroundClampPosition(position);
				    
		    skObjectTemp.gameObject.transform.position = position;

		    if (null != overrideInfo?.Hp)
		    {
			    if (skObjectTemp.TryGetSKComponent(out SKComponentAttackee attackee))
			    {
				    attackee.OverrideDefaultHp(overrideInfo.Hp.Value);
			    }
		    }
		    
		    if (null != overrideInfo?.AttackPower)
		    {
			    if (skObjectTemp.TryGetSKComponent(out SKComponentAttacker attacker))
			    {
				    attacker.OverrideDefaultDamage(overrideInfo.AttackPower.Value);
			    }   
		    }
	    }
    }
}
