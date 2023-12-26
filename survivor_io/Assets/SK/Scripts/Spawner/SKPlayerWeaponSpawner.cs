using System.Collections.Generic;
using System.Linq;

namespace SK
{
    public class SKPlayerWeaponSpawner
    {
	    struct SKPlayerWeaponSpawnInfoForSpawnTime
	    {
		    public float SpawnTime;
		    public SKSpawnInfoBase SpawnInfo;
	    }

	    public SKPlayerWeaponSpawner(uint level)
	    {
		    _level = level;
	    }
	    
	    private uint _level;

	    public bool IsAllSpawned => _init && _playerWeaponSpawnInfoList.Count <= 0;

	    private bool _repeatCheck;
	    
	    private bool _init;
	    
	    private bool _repeat;
	    public float RepeatCoolTime { get; private set; }

	    private float _maxSpawnTime;
	    private float _spawnStartDelay;
	    
	    private SKObject _spawnerSkObject;
	    
	    private float _elapsedTime;
	    public float ElapsedRepeatCoolTime { get; private set; }

	    public uint SpawnCount { get; private set; }

	    public uint RemainSpawnCount => (uint)_playerWeaponSpawnInfoList.Count;

	    public uint ElapsedSpawnCount { get; private set; }
	    
	    private float _spawnInterval;
	    
	    private bool _destroyAllSpawnObjectOnRepeat;
	    private bool _destroyAllSpawnObjectOnDestroy;
	    private bool _affectWeaponRange;
	    
	    private SKSpawnInfoBase _spawnInfo;
	    private List<SKPlayerWeaponSpawnInfoForSpawnTime> _playerWeaponSpawnInfoList = new();
	    private List<int> _removeSpawnInfoList = new();
	    private List<SKObject> _spawnedSkObjectList = new();
	    
	    public void InitData(SKSpawnInfoBase spawnInfo, uint spawnCount, float spawnStartDelay, float spawnDuration, bool spawnRepeat, float spawnRepeatCoolTime, SKObject spawnerSkObject, bool destroyAllSpawnObjectOnRepeat, bool destroyAllSpawnObjectOnDestroy, bool affectWeaponRange)
	    {
		    _init = true;
		    _repeatCheck = false;
		    _repeat = spawnRepeat;
		    RepeatCoolTime = spawnRepeatCoolTime;
		    _elapsedTime = 0;
		    ElapsedRepeatCoolTime = 0;
		    SpawnCount = spawnCount;
		    _spawnerSkObject = spawnerSkObject;
		    _spawnInfo = spawnInfo;
		    _playerWeaponSpawnInfoList.Clear();
		    _removeSpawnInfoList.Clear();
		    _maxSpawnTime = spawnStartDelay + spawnDuration;
		    _spawnStartDelay = spawnStartDelay;
		    _destroyAllSpawnObjectOnRepeat = destroyAllSpawnObjectOnRepeat;
		    _destroyAllSpawnObjectOnDestroy = destroyAllSpawnObjectOnDestroy;
		    _affectWeaponRange = affectWeaponRange;
		    _spawnInterval = SpawnCount == 0 ? 0 : _maxSpawnTime / SpawnCount;

		    ParsingSpawnInfo();
	    }

	    private void OnSpawnedObjectSKEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    if (eventParam.EventType == SKEventManager.SKEventType.Destroy)
		    {
			    _spawnedSkObjectList.Remove(skObject);
			    skObject.EventManager.RemoveListener(SKEventManager.SKEventType.Destroy, OnSpawnedObjectSKEvent);
		    }
	    }

	    private void OnSpawnFromSpawner(SKObject skObject)
	    {
		    if (_destroyAllSpawnObjectOnRepeat || _destroyAllSpawnObjectOnDestroy)
		    {
			    skObject.EventManager.AddListener(SKEventManager.SKEventType.Destroy, OnSpawnedObjectSKEvent);
			    _spawnedSkObjectList.Add(skObject);   
		    }
	    }
	    
	    private void OnRepeatFromSpawner()
	    {
		    if (_destroyAllSpawnObjectOnRepeat)
		    {
			    foreach (var spawnedSKObject in _spawnedSkObjectList)
			    {
				    spawnedSKObject.EventManager.RemoveListener(SKEventManager.SKEventType.Destroy, OnSpawnedObjectSKEvent);
				    spawnedSKObject.DestroyObject();
			    }
			    
			    _spawnedSkObjectList.Clear();
		    }
	    }
	    
	    private void OnDestroyFromSpawner()
	    {
		    if (_destroyAllSpawnObjectOnDestroy)
		    {
			    foreach (var spawnedSKObject in _spawnedSkObjectList)
			    {
				    spawnedSKObject.EventManager.RemoveListener(SKEventManager.SKEventType.Destroy, OnSpawnedObjectSKEvent);
				    spawnedSKObject.DestroyObject();
			    }
			    
			    _spawnedSkObjectList.Clear();
		    }
	    }

	    public void Clear()
	    {
		    OnDestroyFromSpawner();
		    
		    _init = false;
		    _repeatCheck = false;
		    _repeat = false;
		    RepeatCoolTime = 0;
		    _elapsedTime = 0;
		    ElapsedRepeatCoolTime = 0;
		    _spawnerSkObject = null;
		    _spawnInfo = null;
		    _playerWeaponSpawnInfoList.Clear();
		    _removeSpawnInfoList.Clear();
		    _maxSpawnTime = 0;
		    _destroyAllSpawnObjectOnRepeat = false;
		    _affectWeaponRange = false;
		    ElapsedSpawnCount = 0;
	    }

	    private void ParsingSpawnInfo()
	    {
		    _playerWeaponSpawnInfoList.Clear();
		    _removeSpawnInfoList.Clear();
		    
		    var spawnIntervalElapsed = _spawnStartDelay;
		    for (int i = 0; i < SpawnCount; i++)
		    {
			    _playerWeaponSpawnInfoList.Add(new()
			    {
				    SpawnTime = spawnIntervalElapsed,
				    SpawnInfo = _spawnInfo
			    });

			    spawnIntervalElapsed += _spawnInterval;
		    }
	    }

	    public void DuplicateOnceSpawnInfo()
	    {
		    var maxSpawnTime = _playerWeaponSpawnInfoList.Max(it => it.SpawnTime);
		    
		    _playerWeaponSpawnInfoList.Add(new()
		    {
			    SpawnTime = maxSpawnTime + _spawnInterval,
			    SpawnInfo = _spawnInfo
		    });
	    }

	    public void GameUpdate(float deltaTime)
	    {
		    if (false == _init)
		    {
			    return;
		    }
		    
		    ProcessRepeat(deltaTime);
		    ProcessSpawn(deltaTime);
	    }

	    private void ProcessSpawn(float deltaTime)
	    {
		    var prevElapsedTime = _elapsedTime; 
		    _elapsedTime += deltaTime;

		    _removeSpawnInfoList.Clear();
		    for (var i = 0; i < _playerWeaponSpawnInfoList.Count; i++)
		    {
			    var spawnInfo = _playerWeaponSpawnInfoList[i];
			    if (_elapsedTime < spawnInfo.SpawnTime)
			    {
				    continue;
			    }

			    var skObject = Spawn(spawnInfo.SpawnInfo, _level, _affectWeaponRange, _spawnerSkObject);
			    OnSpawnFromSpawner(skObject);
			    _removeSpawnInfoList.Add(i);
			    ElapsedSpawnCount++;
			    if (SpawnCount <= ElapsedSpawnCount && _repeat)
			    {
				    _repeatCheck = true;
			    }
		    }

		    for (int i = _removeSpawnInfoList.Count - 1; i >= 0; i--)
		    {
			    _playerWeaponSpawnInfoList.RemoveAt(i);
		    }
		    
		    _removeSpawnInfoList.Clear();
	    }

	    private void ProcessRepeat(float deltaTime)
	    {
		    if (_repeatCheck)
		    {
			    ElapsedRepeatCoolTime += deltaTime;
			    if (RepeatCoolTime <= ElapsedRepeatCoolTime)
			    {
				    _repeatCheck = false;
				    ElapsedSpawnCount = 0;
				    _elapsedTime = 0;
				    ElapsedRepeatCoolTime = 0;
				    OnRepeatFromSpawner();
			    
				    ParsingSpawnInfo();
			    }
		    }
	    }

	    public static SKObject Spawn(SKSpawnInfoBase spawnInfo, uint level, bool affectWeaponRange, SKObject spawnerSkObject)
	    {
		    return SKSpawner.Spawn(spawnInfo, spawnerSkObject, (skObjectTemp) =>
		    {
			    OnSpawn(skObjectTemp, level, affectWeaponRange);
		    });
	    }

	    public static void OnSpawn(SKObject skObjectTemp, uint level, bool affectWeaponRange)
	    {
		    // 데이터 바인딩을 위한 레벨 전달
		    if (skObjectTemp.SkObject is iSKObjectLevel objectLevel)
		    {
			    objectLevel.Level = level;
		    }
			    
		    // 무기 범위 적용
		    if (affectWeaponRange)
		    {
			    var weaponScale = SKGameManager.Instance.PlayerStatManager.GetStatResultOnlyPercent(StatType.WeaponRange);
			    skObjectTemp.transform.localScale *= 1.0f + weaponScale;
		    }
	    }
    }
}
