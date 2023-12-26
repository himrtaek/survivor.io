using System.Collections.Generic;
using UnityEngine.Events;

namespace SK
{
    public class SKStatManager
    {
	    private readonly SKObject _skObject;
	    private Dictionary<StatType, SKStatInfo> _statInfoByType = new();
	    private Dictionary<StatType, List<SKStatManager>> _syncStatManagerByType = new();

	    public SKStatManager(SKObject skObject)
	    {
		    _skObject = skObject;
	    }
	    
	    public void AddListener(StatType statType, UnityAction<SKObject, StatType, float, float> call)
	    {
		    if (false == _statInfoByType.TryGetValue(statType, out var statInfo))
		    {
			    statInfo = new(_skObject, statType);
			    _statInfoByType.Add(statType, statInfo);
		    }
		    
		    statInfo.OnUpdateValue.AddListener(call);
	    }
	    
	    public void RemoveListener(StatType statType, UnityAction<SKObject, StatType, float, float> call)
	    {
		    if (false == _statInfoByType.TryGetValue(statType, out var statInfo))
		    {
			    return;
		    }

		    statInfo.OnUpdateValue.RemoveListener(call);
	    }
	    
	    public float AddStatData(StatType statType, StatExprType statExprType, StatSourceType statSourceType, long key, float value)
	    {
		    if (_syncStatManagerByType.TryGetValue(statType, out var syncStatManagerList))
		    {
			    foreach (var statManager in syncStatManagerList)
			    {
				    statManager.AddStatData(statType, statExprType, statSourceType, key, value);
			    }
		    }
		    
		    if (false == _statInfoByType.TryGetValue(statType, out var statInfo))
		    {
			    statInfo = new(_skObject, statType);
			    _statInfoByType.Add(statType, statInfo);
		    }
		    
		    return statInfo.Add(statExprType, statSourceType, key, value);
	    }
	    
	    public float RemoveStatData(StatType statType, StatExprType statExprType, StatSourceType statSourceType, long key)
	    {
		    if (_syncStatManagerByType.TryGetValue(statType, out var syncStatManagerList))
		    {
			    foreach (var statManager in syncStatManagerList)
			    {
				    statManager.RemoveStatData(statType, statExprType, statSourceType, key);
			    }
		    }
		    
		    if (false == _statInfoByType.TryGetValue(statType, out var statInfo))
		    {
			   return 0;
		    }
		    
		    return statInfo.Remove(statExprType, statSourceType, key);
	    }
	    
	    public void SyncStartTo(SKStatManager otherStatManager, StatType statType)
	    {
		    CopyTo(otherStatManager, statType);

		    if (false == _syncStatManagerByType.TryGetValue(statType, out var syncStatManagerList))
		    {
			    syncStatManagerList = new();
			    _syncStatManagerByType.Add(statType, syncStatManagerList);
		    }
		    
		    syncStatManagerList.Add(otherStatManager);
	    }
	    
	    public void SyncEndTo(SKStatManager otherStatManager, StatType statType)
	    {
		    if (_syncStatManagerByType.TryGetValue(statType, out var syncStatManagerList))
		    {
			    syncStatManagerList.Remove(otherStatManager);
		    }
	    }
	    
	    public void CopyTo(SKStatManager otherStatManager, StatType statType)
	    {
		    if (true == _statInfoByType.TryGetValue(statType, out var statInfo))
		    {
			    if (false == otherStatManager._statInfoByType.TryGetValue(statType, out var otherStatInfo))
			    {
				    otherStatInfo = new(_skObject, statType);
				    otherStatManager._statInfoByType.Add(statType, otherStatInfo);
			    }

			    otherStatInfo.Clear();
			    statInfo.CopyTo(otherStatInfo);
		    }
	    }

	    public float GetStatResultValue(StatType statType)
	    {
		    if (false == _statInfoByType.TryGetValue(statType, out var statData))
		    {
			    return 0;
		    }

		    return statData.ResultValue;
	    }

	    public float GetStatResultOnlyAdd(StatType statType)
	    {
		    if (false == _statInfoByType.TryGetValue(statType, out var statData))
		    {
			    return 0;
		    }

		    return statData.ResultOnlyAdd;
	    }

	    public float GetStatResultOnlyPercent(StatType statType)
	    {
		    if (false == _statInfoByType.TryGetValue(statType, out var statData))
		    {
			    return .0f;
		    }

		    return statData.ResultOnlyPercent;
	    }

	    public void Clear()
	    {
		    foreach (var (statType, statInfo) in _statInfoByType)
		    {
			    statInfo.Clear();   
		    }
		    
		    _syncStatManagerByType.Clear();
	    }
    }
}
