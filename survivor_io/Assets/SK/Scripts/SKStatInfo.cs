using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SKStatInfo
{
    public enum SKStatType
    {
        Attack,
        Hp,
        MoveSpeed
    }
    
    public enum SKStatExprType
    {
        Add,
        Multiply
    }
    
    public enum SKStatSourceType
    {
        None,
        Test,
        Base,
        Table,
        Buff,
    }
    
    public long ResultValue { get; private set; }

    private Dictionary<SKStatExprType, Dictionary<SKStatSourceType, Dictionary<long, long>>> _statValueByExprType = new();

    public long Add(SKStatExprType statExprType, SKStatSourceType statSourceType, long key, long value)
    {
        if (false == _statValueByExprType.TryGetValue(statExprType, out var statValueBySourceType))
        {
            statValueBySourceType = new();
            _statValueByExprType.Add(statExprType, statValueBySourceType);
        }

        if (false == statValueBySourceType.TryGetValue(statSourceType, out var statValueByKey))
        {
            statValueByKey = new();
            statValueBySourceType.Add(statSourceType, statValueByKey);
        }
        
        statValueByKey.Add(key, value);

        return UpdateResultValue();
    }

    public long Remove(SKStatExprType statExprType, SKStatSourceType statSourceType, long key)
    {
        if (false == _statValueByExprType.TryGetValue(statExprType, out var statValueBySourceType))
        {
            return ResultValue;
        }

        if (false == statValueBySourceType.TryGetValue(statSourceType, out var statValueByKey))
        {
            return ResultValue;
        }

        if (false == statValueByKey.Remove(key))
        {
            return ResultValue;
        }

        return UpdateResultValue();
    }

    public long UpdateResultValue()
    {
        long resultValue = 0;
        {
            if (_statValueByExprType.TryGetValue(SKStatExprType.Add, out var statValueBySourceType))
            {
                resultValue = statValueBySourceType.Sum(it => it.Value.Sum(it2 => it2.Value));
            }
        }
        
        {
            if (_statValueByExprType.TryGetValue(SKStatExprType.Multiply, out var statValueBySourceType))
            {
                var resultValueMultiply = statValueBySourceType.Sum(it => it.Value.Sum(it2 => it2.Value));
                resultValue += resultValue * (long)(resultValueMultiply / 100.0f);
            }
        }

        ResultValue = resultValue;
        
        return ResultValue;
    }
}
