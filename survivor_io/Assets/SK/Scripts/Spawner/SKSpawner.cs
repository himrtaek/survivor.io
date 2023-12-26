using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SK
{
    public class SKSpawner
    {
	    public bool IsAllSpawned => _isAllSpawned;
	    private bool _isAllSpawned;
	    
	    private bool _init;
	    private bool _repeat;
	    private float _repeatCoolTime;
	    private float _maxSpawnTime;
	    
	    private SKObject _spawnerSkObject;
	    
	    private float _elapsedTime;
	    private float _elapsedRepeatCoolTime;
	    
	    private UnityAction<SKObject> _onSpawn;
	    private UnityAction _onRepeat;
	    
	    private List<SKSpawnInfo> _spawnInfo;
	    
	    public void InitData(List<SKSpawnInfo> spawnSetting, bool repeat, float repeatTime, SKObject spawnerSkObject, UnityAction<SKObject> onSpawn = null, UnityAction onRepeat = null)
	    {
		    _init = true;
		    _spawnerSkObject = spawnerSkObject;
		    _elapsedTime = 0;
		    _isAllSpawned = false;
		    _spawnInfo = spawnSetting;
		    _repeat = repeat;
		    _repeatCoolTime = repeatTime;
		    _onSpawn = onSpawn;
		    _onRepeat = onRepeat;
		    _maxSpawnTime = spawnSetting.Max(it => it.spawnTime);
	    }
	    
	    public void InitData(SKSpawnSetting spawnSetting, SKObject spawnerSkObject, UnityAction<SKObject> onSpawn = null, UnityAction onRepeat = null)
	    {
		    InitData(spawnSetting.SpawnInfos, spawnSetting.Repeat, spawnSetting.RepeatTime, spawnerSkObject, onSpawn, onRepeat);
	    }

	    public void Clear()
	    {
		    _init = false;
		    _spawnerSkObject = null;
		    _elapsedTime = 0;
		    _isAllSpawned = false;
		    _spawnInfo = null;
		    _repeat = false;
		    _repeatCoolTime = 0;
		    _onSpawn = null;
		    _onRepeat = null;
	    }

	    public void GameUpdate(float deltaTime)
	    {
		    if (false == _init)
		    {
			    return;
		    }
		    
		    if (_isAllSpawned)
		    {
			    if (false == _repeat)
			    {
				    return;
			    }
			    
			    _elapsedRepeatCoolTime += deltaTime;
			    if (_repeatCoolTime <= _elapsedRepeatCoolTime)
			    {
				    _elapsedRepeatCoolTime = 0;
				    _elapsedTime = 0;
				    _onRepeat?.Invoke();
			    }
			    else
			    {
				    return;
			    }
		    }
		    
		    var prevElapsedTime = _elapsedTime; 
		    _elapsedTime += deltaTime;

		    foreach (var spawnInfo in _spawnInfo)	
		    {
			    if (spawnInfo.spawnTime <= prevElapsedTime && prevElapsedTime != 0)
			    {
				    continue;
			    }
			    
			    if (_elapsedTime < spawnInfo.spawnTime)
			    {
				    continue;
			    }
			    
			    var skObject = Spawn(spawnInfo, _spawnerSkObject);
			    _onSpawn?.Invoke(skObject);
		    }

		    _isAllSpawned = _maxSpawnTime <= _elapsedTime;
	    }

	    public static SKObject Spawn(SKSpawnInfoBase spawnInfo, SKObject spawnerSkObject, Action<SKObject> onSpawn = null)
	    {
		    return SKGameManager.Instance.ObjectManager.SpawnObject(spawnInfo.gameObject, spawnerSkObject, spawnInfo.isChild, onSpawnBefore:
			    (skObjectTemp) =>
			    {
				    var transformTemp = skObjectTemp.gameObject.transform;
				    transformTemp.localPosition = Vector3.zero;
				    transformTemp.localRotation = Quaternion.identity;
				    transformTemp.localScale = Vector3.one;
				    
				    if (spawnInfo.isLocalPosition)
				    {
					    transformTemp.localPosition = spawnInfo.position;
				    }
				    else
				    {
					    transformTemp.position = spawnInfo.position;   
				    }

				    if (spawnInfo.addParentPosition && spawnerSkObject)
				    {
					    transformTemp.position += spawnerSkObject.transform.position;
				    }
				    
				    if (spawnInfo.isLocalRotation)
				    {
					    transformTemp.localRotation = Quaternion.Euler(transformTemp.localRotation.x, transformTemp.localRotation.y, spawnInfo.rotationZ);
				    }
				    else
				    {
					    transformTemp.rotation = Quaternion.Euler(transformTemp.rotation.x, transformTemp.rotation.y, spawnInfo.rotationZ);   
				    }

				    if (spawnInfo.addParentRotation && spawnerSkObject)
				    {
					    var rotationZ = transformTemp.rotation.eulerAngles.z;
					    rotationZ += spawnerSkObject.transform.rotation.eulerAngles.z;
					    transformTemp.rotation = Quaternion.Euler(transformTemp.rotation.x, transformTemp.rotation.y, rotationZ);
				    }

				    transformTemp.localScale = spawnInfo.scale;
				    
				    onSpawn?.Invoke(skObjectTemp);
			    });
	    }
    }
}
