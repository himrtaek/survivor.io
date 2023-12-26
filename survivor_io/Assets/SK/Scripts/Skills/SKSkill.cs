using System;
using System.Collections.Generic;

namespace SK
{
	public enum SKSkillType
	{
		ItemAcquisitionRange,
		GoldEarned,
		WeaponLifeTime,
		HealPer5Second,
		WeaponRange,
		MoveSpeedPercent,
		ExpEarned,
		MaxHp,
		RandomRebirth,
		DecreaseDamage,
		DecreaseDamageOnDamage,
		MoveSpeedOnKill,
	}
	
	public class SKSkill
	{
		public SKSkill(SKSkillType skillType, long skillValue1 = 0, long skillValue2 = 0, long skillValue3 = 0, long skillValue4 = 0)
		{
			SkillType = skillType;
			SkillValue1 = skillValue1;
			SkillValue2 = skillValue2;
			SkillValue3 = skillValue3;
			SkillValue4 = skillValue4;
		}

		public SKSkillType SkillType { get; private set; }
		public long SkillValue1 { get; private set; }
		public long SkillValue2 { get; private set; }
		public long SkillValue3 { get; private set; }
		public long SkillValue4 { get; private set; }
		
		private List<ulong> _triggerKeyList;

		private bool _keepTriggerOnUnDoAction;
		
		public void DoAction(SKObject skObject, StatSourceType statSourceType, long key)
		{
			switch (SkillType)
			{
				case SKSkillType.GoldEarned:
				{
					skObject.StatManager.AddStatData(StatType.GoldEarned, StatExprType.Percent, statSourceType,
						key, SkillValue1);
				}
					break;
				case SKSkillType.ItemAcquisitionRange:
				{
					skObject.StatManager.AddStatData(StatType.ItemAcquisitionRange, StatExprType.Percent, statSourceType,
						key, SkillValue1);
				}
					break;
				case SKSkillType.WeaponLifeTime:
				{
					skObject.StatManager.AddStatData(StatType.WeaponLifeTime, StatExprType.Percent, statSourceType,
						key, SkillValue1);
				}
					break;
				case SKSkillType.HealPer5Second:
				{
					var triggerBuilder = new SKTrigger.SKTriggerBuilder();
					triggerBuilder.SetCondition(new SKTriggerTimerCondition.Builder().SetTime(5));
					triggerBuilder.AddAction(new SKTriggerHealAction.Builder().SetHealPercent((uint)SkillValue1));
					triggerBuilder.SetIsRepeat(true);
					
					_triggerKeyList = new() { skObject.TriggerManager.AddTrigger(triggerBuilder) };
				}
					break;
				case SKSkillType.WeaponRange:
				{
					skObject.StatManager.AddStatData(StatType.WeaponRange, StatExprType.Percent, statSourceType,
						key, SkillValue1);
				}
					break;
				case SKSkillType.MoveSpeedPercent:
				{
					skObject.StatManager.AddStatData(StatType.MoveSpeed, StatExprType.Percent, statSourceType,
						key, SkillValue1);
				}
					break;
				case SKSkillType.ExpEarned:
				{
					skObject.StatManager.AddStatData(StatType.ExpEarned, StatExprType.Percent, statSourceType,
						key, SkillValue1);
				}
					break;
				case SKSkillType.MaxHp:
				{
					skObject.StatManager.AddStatData(StatType.MaxHp, StatExprType.Percent, statSourceType,
						key, SkillValue1);
				}
					break;
				case SKSkillType.RandomRebirth:
				{
					skObject.StatManager.AddStatData(StatType.RandomRebirth, StatExprType.Percent, statSourceType,
						key, SkillValue1);
					skObject.StatManager.AddStatData(StatType.RandomRebirthHP, StatExprType.Percent, statSourceType,
						key, SkillValue2);
				}
					break;
				case SKSkillType.DecreaseDamage:
				{
					skObject.StatManager.AddStatData(StatType.DecreaseDamage, StatExprType.Percent, statSourceType,
						key, SkillValue1);
				}
					break;
				case SKSkillType.DecreaseDamageOnDamage:
				{
					var triggerBuilder = new SKTrigger.SKTriggerBuilder();
					triggerBuilder.SetEvent(new SKTriggerSKEventReceiveEvent.Builder().
						SetSKEvent(SKEventManager.SKEventType.Damage));
					triggerBuilder.AddAction(new SKTriggerAddStatAction.Builder()
						.SetStatType(StatType.DecreaseDamage)
						.SetExprType(StatExprType.Percent)
						.SetSourceType(statSourceType)
						.SetStatKey(key)
						.SetStatValue(SkillValue2));
					triggerBuilder.AddAction(new SKTriggerRemoveStatAction.Builder()
						.SetStatType(StatType.DecreaseDamage)
						.SetExprType(StatExprType.Percent)
						.SetSourceType(statSourceType)
						.SetStatKey(key)
						.SetDelay(SkillValue1)
						.SetForceFireBeforeClear(true)
						.SetForceFireBeforeRepeatFire(true));
					triggerBuilder.SetIsRepeat(true);
					
					_triggerKeyList = new() { skObject.TriggerManager.AddTrigger(triggerBuilder) };
				}
					break;
				case SKSkillType.MoveSpeedOnKill:
				{
					var triggerBuilder = new SKTrigger.SKTriggerBuilder();
					triggerBuilder.SetEvent(new SKTriggerSKEventReceiveEvent.Builder().
						SetSKEvent(SKEventManager.SKEventType.Kill)
						.SetCount(SkillValue1));
					triggerBuilder.AddAction(new SKTriggerAddStatAction.Builder()
						.SetStatType(StatType.MoveSpeed)
						.SetExprType(StatExprType.Percent)
						.SetSourceType(statSourceType)
						.SetStatKey(key)
						.SetStatValue(SkillValue3));
					triggerBuilder.AddAction(new SKTriggerRemoveStatAction.Builder()
						.SetStatType(StatType.MoveSpeed)
						.SetExprType(StatExprType.Percent)
						.SetSourceType(statSourceType)
						.SetStatKey(key)
						.SetDelay(SkillValue2)
						.SetForceFireBeforeClear(true)
						.SetForceFireBeforeRepeatFire(true));
					triggerBuilder.SetIsRepeat(true);
					
					_triggerKeyList = new() { skObject.TriggerManager.AddTrigger(triggerBuilder) };
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		public void UnDoAction(SKObject skObject, StatSourceType statSourceType, long key)
		{
			if (null != _triggerKeyList && false == _keepTriggerOnUnDoAction)
			{
				foreach (var triggerKey in _triggerKeyList)
				{
					skObject.TriggerManager.RemoveTrigger(triggerKey);	
				}
			}
			
			switch (SkillType)
			{
				case SKSkillType.GoldEarned:
				{
					skObject.StatManager.RemoveStatData(StatType.GoldEarned, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.ItemAcquisitionRange:
				{
					skObject.StatManager.RemoveStatData(StatType.ItemAcquisitionRange, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.WeaponLifeTime:
				{
					skObject.StatManager.RemoveStatData(StatType.WeaponLifeTime, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.HealPer5Second:
					break;
				case SKSkillType.WeaponRange:
				{
					skObject.StatManager.RemoveStatData(StatType.WeaponRange, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.MoveSpeedPercent:
				{
					skObject.StatManager.RemoveStatData(StatType.MoveSpeed, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.ExpEarned:
				{
					skObject.StatManager.RemoveStatData(StatType.ExpEarned, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.MaxHp:
				{
					skObject.StatManager.RemoveStatData(StatType.MaxHp, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.RandomRebirth:
				{
					skObject.StatManager.RemoveStatData(StatType.RandomRebirth, StatExprType.Percent, statSourceType,
						key);
					skObject.StatManager.RemoveStatData(StatType.RandomRebirthHP, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.DecreaseDamage:
				{
					skObject.StatManager.RemoveStatData(StatType.DecreaseDamage, StatExprType.Percent, statSourceType,
						key);
				}
					break;
				case SKSkillType.DecreaseDamageOnDamage:
					break;
				case SKSkillType.MoveSpeedOnKill:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
