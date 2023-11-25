using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SKStatManager
{
    private Dictionary<SKStatInfo.SKStatType, SKStatInfo> _statInfoByType = new();

    public void Add(SKStatInfo.SKStatType statType, SKStatInfo.SKStatExprType statExprType, SKStatInfo.SKStatSourceType statSourceType, long key, long value)
    {
        if (false == _statInfoByType.TryGetValue(statType, out var statInfo))
        {
            statInfo = new();
            _statInfoByType.Add(statType, statInfo);
        }
        
        statInfo.Add(statExprType, statSourceType, key, value);
    }

    public void Remove(SKStatInfo.SKStatType statType, SKStatInfo.SKStatExprType statExprType, SKStatInfo.SKStatSourceType statSourceType, long key)
    {
        if (false == _statInfoByType.TryGetValue(statType, out var statInfo))
        {
            return;
        }
        
        statInfo.Remove(statExprType, statSourceType, key);
    }

    public long GetValue(SKStatInfo.SKStatType statType, long defaultValue = 0)
    {
        if (false == _statInfoByType.TryGetValue(statType, out var statInfo))
        {
            return defaultValue;
        }

        return statInfo.ResultValue;
    }
}
