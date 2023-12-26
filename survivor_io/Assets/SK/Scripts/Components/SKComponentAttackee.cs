using System;
using System.Collections.Generic;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SK
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(Collider2D))]
    public class SKComponentAttackee : SKComponentFromDataBase
    {
	    #region Cache
	    
	    [SerializeField] private Rigidbody2D _rigidbody2D;
	    public Rigidbody2D Rigidbody2D
	    {
		    get
		    {
			    if (false == _rigidbody2D)
			    {
				    TryGetComponent(out _rigidbody2D);
			    }
			    
			    return _rigidbody2D;
		    }
	    }

	    [SerializeField] private Collider2D _collider2D;
	    public Collider2D Collider2D
	    {
		    get
		    {
			    if (false == _collider2D)
			    {
				    TryGetComponent(out _collider2D);
			    }
			    
			    return _collider2D;
		    }
	    }
	    
	    protected override void Reset()
	    {
		    base.Reset();
		    
		    if(false == _rigidbody2D)
		    {
			    TryGetComponent(out _rigidbody2D);
		    }
		    
		    if(false == _collider2D)
		    {
			    TryGetComponent(out _collider2D);
		    }
	    }

	    #endregion
	    
	    [SerializeField] private bool invincibility;
	    [SKEditableField] [SerializeField] private ulong defaultHp;
	    [SerializeField] private List<SKObject.SKObjectType> ignoreTypeList;
	    [SerializeField] private uint dropItemPoolId;

	    private uint _shieldOfCount;
	    private ulong _shield;
	    private ulong _currentHp;

	    public ulong CurrentHp => _currentHp;
	    
	    public float DestroyDelay { get; set; }

	    public void OverrideDefaultHp(ulong newDefaultHp)
	    {
		    defaultHp = newDefaultHp;
	    }
	    
	    public void OverrideDropItemPoolId(uint newDropItemPoolId)
	    {
		    dropItemPoolId = newDropItemPoolId;
	    }
	    
	    protected override void ImportFieldFromData()
	    {
		    if (SkObject.DataID <= 0)
		    {
			    return;
		    }
	    }

	    protected  override List<string> ExportFieldToData()
	    {
		    return new List<string>()
		    {
			    invincibility ? "1" : "0",
			    defaultHp.ToString(),
		    };
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    SkObject.StatManager.AddListener(StatType.MaxHp, OnChangeMaxHp);
		    SkObject.StatManager.AddStatData(StatType.MaxHp, StatExprType.Add, StatSourceType.Prefab, GetInstanceID(), (long)defaultHp);
		    
		    Collider2D.enabled = true;
		    DestroyDelay = 0;
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.StatManager.RemoveListener(StatType.MaxHp, OnChangeMaxHp);
		    SkObject.StatManager.RemoveStatData(StatType.MaxHp, StatExprType.Add, StatSourceType.Prefab, GetInstanceID());
		    
		    base.OnSKObjectDestroy();
	    }

	    private void OnChangeMaxHp(SKObject skObject, StatType statType, float prevValue, float newValue)
	    {
		    if (prevValue < newValue)
		    {
			    var damagedValue = prevValue - _currentHp;
			    _currentHp = (ulong)(newValue - damagedValue);
		    }
		    else
		    {
			    if (newValue < _currentHp)
			    {
				    _currentHp = (ulong)newValue;
			    }
		    }
	    }

	    public override void GameUpdate(float deltaTime)
	    {
		    base.GameUpdate(deltaTime);
	    }

	    public bool CanDamage(SKComponentAttacker attacker)
	    {
		    if (SKObject.SKObjectStateType.Spawned != SkObject.ObjectState)
		    {
			    return false;
		    }

		    if (null != ignoreTypeList && ignoreTypeList.Contains(attacker.SkObject.ObjectType))
		    {
			    return false;
		    }

		    return true;
	    }

	    public void OnDamage(SKComponentAttacker attacker, ulong damage)
	    {
		    // HP를 넘어서서 피해를 입힐 수 없는 계산법
		    var damageOfHp = Math.Min(damage, _shield + _currentHp);
		    var damageOfShield = Math.Min(damageOfHp, _shield);
		    uint damageOfShieldCount = 0;
		    
		    if (0 < _shieldOfCount)
		    {
			    _shieldOfCount--;
			    damageOfShieldCount = 1;
		    }
		    else
		    {
			    if (0 < _shield)
			    {
				    _shield -= damageOfShield;
				    damageOfHp -= damageOfShield;
			    }

			    if (0 < damageOfHp)
			    {
				    if (_currentHp <= damageOfHp)
				    {
					    _currentHp = 0;
				    }
				    else
				    {
					    _currentHp -= damageOfHp;
				    }
			    }
		    }

		    var skObjectType = SkObject.ObjectType;
		    var invincibilityPlayer = false;
		    if (skObjectType == SKObject.SKObjectType.Player)
		    {
			    if (SKGameManager.Instance.CheatFlagType.HasFlagNonAlloc(SKGameManager.SKCheatFlagType.PlayerInvincibility))
			    {
				    invincibilityPlayer = true;
			    }
		    }

		    var invincibilityMonster = false;
		    if (skObjectType == SKObject.SKObjectType.Monster)
		    {
			    if (SKGameManager.Instance.CheatFlagType.HasFlagNonAlloc(SKGameManager.SKCheatFlagType.MonsterInvincibility))
			    {
				    invincibilityMonster = true;
			    }
		    }

		    if (invincibility || invincibilityPlayer || invincibilityMonster)
		    {
			    _currentHp = (ulong)SkObject.StatManager.GetStatResultValue(StatType.MaxHp);
		    }
		    
		    var eventParam = SKEventParam.GetOrNewParam<SKDamageEventParam>();
		    eventParam.Attacker = attacker;
		    eventParam.DamageOfHp = damageOfHp;
		    eventParam.DamageOfShield = damageOfShield;
		    eventParam.DamageOfShieldCount = damageOfShieldCount;
		    SkObject.EventManager.BroadCast(SKEventManager.SKEventType.Damage, eventParam);
		    
		    /*N2Logger<LogCombat>.Log($"{attacker.name}이 {SKObject.name}에게 {damage.ToStringCached()} 피해");*/

		    if (_currentHp <= 0)
		    {
			    var randomRebirth = SkObject.StatManager.GetStatResultOnlyPercent(StatType.RandomRebirth);
			    var randomRebirthRange = Random.value;
			    if (randomRebirthRange <= randomRebirth && 0 < randomRebirth)
			    {
				    _currentHp = (ulong)(SkObject.StatManager.GetStatResultValue(StatType.MaxHp) * SkObject.StatManager.GetStatResultOnlyPercent(StatType.RandomRebirthHP));
			    }
			    else
			    {
				    if (0 < dropItemPoolId)
				    {
					    SKObjectDropItem.SpawnDropItem(dropItemPoolId, transform.position);
				    }
			    
				    var killEventParam = SKEventParam.GetOrNewParam<SKKillEventParam>();
				    killEventParam.Attackee = this;
				    killEventParam.DamageOfHp = damageOfHp;
				    killEventParam.DamageOfShield = damageOfShield;
				    killEventParam.DamageOfShieldCount = damageOfShieldCount;
				    attacker.SkObject.EventManager.BroadCast(SKEventManager.SKEventType.Kill, killEventParam, false);

				    if (skObjectType == SKObject.SKObjectType.Monster 
				        && attacker.SkObject.ObjectType == SKObject.SKObjectType.PlayerWeapon)
				    {
					    SKGameManager.Instance.ObjectManager.ObjectPlayer.EventManager.BroadCast(SKEventManager.SKEventType.Kill, killEventParam);
					    SKGameManager.Instance.AddKill(1);
				    }

				    OnDeath();
			    }
		    }
	    }

	    public void Heal(ulong healValue)
	    {
		    var maxHp = SkObject.StatManager.GetStatResultValue(StatType.MaxHp);
		    healValue = Math.Min(healValue, (ulong)(maxHp - CurrentHp));
		    
		    _currentHp += healValue;
				    
		    var healEventParam = SKEventParam.GetOrNewParam<SKHealEventParam>();
		    healEventParam.HealValue = healValue;
		    SkObject.EventManager.BroadCast(SKEventManager.SKEventType.Heal, healEventParam);
	    }

	    public void OnDeath()
	    {
		    Collider2D.enabled = false;
				    
		    var hpZeroParam = SKEventParam.GetOrNewParam<SKHPZeroEventParam>();
		    hpZeroParam.Attackee = this;
		    SkObject.EventManager.BroadCast(SKEventManager.SKEventType.HPZero, hpZeroParam);
				    
		    SkObject.DestroyObject(DestroyDelay);
	    }
    }
}
