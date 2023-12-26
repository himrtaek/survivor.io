using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


namespace SK
{
	public enum StatType
	{
		MoveSpeed,
		AttackPower,
		DecreaseAttackDotCoolTime,
		MaxHp,
		GoldEarned,
		ItemAcquisitionRange,
		WeaponLifeTime,
		WeaponRange,
		ExpEarned,
		RandomRebirth,
		RandomRebirthHP,
		DecreaseDamage,
		ProjectileSpeed,
	}

	public enum StatExprType
	{
		Add,
		Percent,
		
		End,
	}

	public enum StatSourceType
	{
		None,
		Test,
		BaseSetting,
		Prefab,
		Spawner,
		EquipItem,
		SupportItem,
		Buff,
	}
	
	[Serializable]
	public class SKStatSerializeData
	{
		public StatType StatType;
		public StatExprType StatExprType;
		public long Value;
	}
	
    public class SKStatInfo
    {
	    private SKObject _skObject;
	    public StatType StatType { get; private set; }
	    public float ResultValue { get; private set; }
	    public float ResultOnlyAdd { get; private set; }
	    public float ResultOnlyPercent { get; private set; }
	    private Dictionary<StatExprType, Dictionary<StatSourceType, Dictionary<long, float>>> _statDataByExprType = new();
	    
	    public UnityEvent<SKObject, StatType, float, float> OnUpdateValue { get; } = new();

	    public SKStatInfo(SKObject skObject, StatType statType)
	    {
		    _skObject = skObject;
		    StatType = statType;
	    }

	    public float Add(StatExprType statExprType, StatSourceType statSourceType, long key, float value)
	    {
		    if (false == _statDataByExprType.TryGetValue(statExprType, out var statDataBySourceType))
		    {
			    statDataBySourceType = new();
			    _statDataByExprType.Add(statExprType, statDataBySourceType);
		    }

		    if (false == statDataBySourceType.TryGetValue(statSourceType, out var statDataByKey))
		    {
			    statDataByKey = new();
			    statDataBySourceType.Add(statSourceType, statDataByKey);
		    }

		    if (statDataByKey.ContainsKey(key))
		    {
			    Debug.LogWarning("스탯 키가 중복되었습니다. 값이 덮어씌워 집니다");
			    statDataByKey[key] = value;
		    }
		    else
		    {
			    statDataByKey.Add(key, value);
		    }
		    
		    return UpdateValue();
	    }

	    public float Remove(StatExprType statExprType, StatSourceType statSourceType, long key)
	    {
		    if (false == _statDataByExprType.TryGetValue(statExprType, out var statDataBySourceType))
		    {
			    return ResultValue;
		    }
		    
		    if (false == statDataBySourceType.TryGetValue(statSourceType, out var statDataByKey))
		    {
			    return ResultValue;
		    }

		    if (false == statDataByKey.Remove(key))
		    {
			    return ResultValue;
		    }
		    
		    return UpdateValue();
	    }

	    public float Remove(StatSourceType statSourceType)
	    {
		    var clear = false;
		    for (int i = 0; i < (int)StatExprType.End; ++i)
		    {
			    var statExprType = (StatExprType)i;
			    if (false == _statDataByExprType.TryGetValue(statExprType, out var statDataBySourceType))
			    {
				    continue;
			    }
		    
			    if (false == statDataBySourceType.TryGetValue(statSourceType, out var statDataByKey))
			    {
				    continue;
			    }

			    clear = true;
			    statDataByKey.Clear();
		    }

		    if (false == clear)
		    {
			    return ResultValue;
		    }
		    
		    return UpdateValue();
	    }

	    public float Clear()
	    {
		    _statDataByExprType.Clear();

		    return UpdateValue();
	    }

	    public void CopyTo(SKStatInfo otherStatInfo)
	    {
		    for (int i = 0; i < (int)StatExprType.End; ++i)
		    {
			    var statExprType = (StatExprType)i;
			    if (false == _statDataByExprType.TryGetValue(statExprType, out var statDataBySourceType))
			    {
				    continue;
			    }

			    foreach (var (statSourceType, statDataByKey) in statDataBySourceType)
			    {
				    foreach (var it in statDataByKey)
				    {
					    if (false == otherStatInfo._statDataByExprType.TryGetValue(statExprType, out var statDataBySourceTypeOther))
					    {
						    statDataBySourceTypeOther = new();
						    otherStatInfo._statDataByExprType.Add(statExprType, statDataBySourceTypeOther);
					    }

					    if (false == statDataBySourceTypeOther.TryGetValue(statSourceType, out var statDataByKeyOther))
					    {
						    statDataByKeyOther = new();
						    statDataBySourceTypeOther.Add(statSourceType, statDataByKeyOther);
					    }
				    
					    statDataByKeyOther.Add(it.Key, it.Value);
				    }
			    }
		    }
		    
		    otherStatInfo.UpdateValue();
	    }

	    public void CopyTo(SKStatInfo otherStatInfo, StatSourceType statSourceType)
	    {
		    var clear = false;
		    for (int i = 0; i < (int)StatExprType.End; ++i)
		    {
			    var statExprType = (StatExprType)i;
			    if (false == _statDataByExprType.TryGetValue(statExprType, out var statDataBySourceType))
			    {
				    continue;
			    }
		    
			    if (false == statDataBySourceType.TryGetValue(statSourceType, out var statDataByKey))
			    {
				    continue;
			    }

			    foreach (var it in statDataByKey)
			    {
				    if (false == otherStatInfo._statDataByExprType.TryGetValue(statExprType, out var statDataBySourceTypeOther))
				    {
					    statDataBySourceTypeOther = new();
					    otherStatInfo._statDataByExprType.Add(statExprType, statDataBySourceTypeOther);
				    }

				    if (false == statDataBySourceTypeOther.TryGetValue(statSourceType, out var statDataByKeyOther))
				    {
					    statDataByKeyOther = new();
					    statDataBySourceTypeOther.Add(statSourceType, statDataByKeyOther);
				    }
				    
				    statDataByKeyOther.Add(it.Key, it.Value);
			    }
		    }

		    otherStatInfo.UpdateValue();
	    }

	    private float UpdateValue()
	    {
		    float resultValue = 0;
		    for (int i = 0; i < (int)StatExprType.End; ++i)
		    {
			    var statExprType = (StatExprType)i;
			    switch (statExprType)
			    {
				    case StatExprType.Add:
				    {
					    float addValue = 0;
					    if (_statDataByExprType.TryGetValue(statExprType, out var statDataBySourceType))
					    {
						    addValue += statDataBySourceType.Sum(it => it.Value.Sum(it2 => it2.Value));
					    }

					    ResultOnlyAdd = addValue;
					    resultValue += addValue;
				    }
					    break;
				    case StatExprType.Percent:
				    {
					    float percentValue = 0;
					    if (_statDataByExprType.TryGetValue(statExprType, out var statDataBySourceType))
					    {
						    percentValue += statDataBySourceType.Sum(it => it.Value.Sum(it2 => it2.Value));
					    }

					    ResultOnlyPercent = percentValue / 100.0f;
					    if (IsOnlyPercentExpr(StatType))
					    {
						    if (resultValue != 0)
						    {
							    Debug.LogError("Logic Error");
						    }
						    
						    resultValue = ResultOnlyPercent;
					    }
					    else
					    {
						    var addValue = resultValue * ResultOnlyPercent;
						    resultValue += addValue;
					    }
				    }
					    break;
				    default:
					    throw new ArgumentOutOfRangeException();
			    }
		    }

		    var prevResultValue = ResultValue;
		    ResultValue = resultValue;
		    
		    OnUpdateValue.Invoke(_skObject, StatType, prevResultValue, ResultValue);
		    
		    return ResultValue;
	    }

	    private static bool IsOnlyPercentExpr(StatType statType)
	    {
		    switch (statType)
		    {
			    case StatType.DecreaseAttackDotCoolTime:
			    case StatType.GoldEarned:
			    case StatType.ItemAcquisitionRange:
			    case StatType.WeaponLifeTime:
			    case StatType.WeaponRange:
			    case StatType.ExpEarned:
			    case StatType.RandomRebirth:
			    case StatType.RandomRebirthHP:
			    case StatType.DecreaseDamage:
				    return true;
			    default:
				    return false;
		    }
	    }
    }
}
