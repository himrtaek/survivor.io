using System;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Collider2D))]
    public class SKComponentAttacker : SKComponentFromDataBase
    {
	    #region Cache

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
		    
		    if(false == _collider2D)
		    {
			    TryGetComponent(out _collider2D);
		    }
	    }

	    #endregion
	    
	    [SKEditableField] [SerializeField] private bool isPercentDamage;
	    [SKEditableField] [SerializeField] private ulong defaultDamage;
	    [SKEditableField] [SerializeField] private float dotDamageCoolTime = -1;
	    [SKEditableField] [SerializeField] private bool destroyOnAttack;
	    [SKEditableField] [SerializeField] private float knockBackPower;
	    [SerializeField] private List<SKObject.SKObjectType> ignoreTypeList;

	    private List<ulong> _collisionStayObjectSpawnIdList = new();
	    private List<SKComponentAttackee> _collisionStayObjectAttackeeListTemp = new();
	    private Dictionary<ulong, float> _remainDotDamageCoolTimeBySpawnId = new();
	    private List<ulong> _spawnIdTemp = new();
	    private List<ulong> _removeSpawnIdTemp = new();
	    private bool isPlayerWeapon;
	    
	    public void OverrideDefaultDamage(ulong newDefaultDamage)
	    {
		    defaultDamage = newDefaultDamage;
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
			    defaultDamage.ToString(),
			    ((int)(dotDamageCoolTime * 100)).ToString(),
			    destroyOnAttack ? "1" : "0",
			    knockBackPower.ToString(),
		    };
	    }

	    protected override void Awake()
	    {
		    base.Awake();
		    
		    if (SkObject is SKObjectPlayerWeapon playerWeapon)
		    {
			    isPlayerWeapon = true;
		    }
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    _collisionStayObjectSpawnIdList.Clear();
		    _spawnIdTemp.Clear();
		    _removeSpawnIdTemp.Clear();
		    _remainDotDamageCoolTimeBySpawnId.Clear();
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    base.OnSKObjectDestroy();
	    }

	    public override void GameUpdate(float deltaTime)
	    {
		    base.GameUpdate(deltaTime);

		    ProcessCollision();
		    ProcessDotDamageCoolTime(deltaTime);
	    }

	    private void ProcessDotDamageCoolTime(float deltaTime)
	    {
		    foreach (var it in _remainDotDamageCoolTimeBySpawnId)
		    {
			    _spawnIdTemp.Add(it.Key);
		    }
		    
		    foreach (var spawnId in _spawnIdTemp)
		    {
			    var remainDamageCoolTime = _remainDotDamageCoolTimeBySpawnId[spawnId];
			    var newRemainDamageCoolTime = remainDamageCoolTime - deltaTime;
			    if (newRemainDamageCoolTime <= 0)
			    {
				    _removeSpawnIdTemp.Add(spawnId);   
			    }
			    else
			    {
				    _remainDotDamageCoolTimeBySpawnId[spawnId] = newRemainDamageCoolTime;   
			    }
		    }

		    foreach (var spawnId in _removeSpawnIdTemp)
		    {
			    _remainDotDamageCoolTimeBySpawnId.Remove(spawnId);
		    }
		    
		    _removeSpawnIdTemp.Clear();
		    _spawnIdTemp.Clear();
	    }

	    public void OnCollisionStart(Collision2D other)
	    {
		    if (other.gameObject.layer == gameObject.layer)
		    {
			    return;
		    }
		    
		    if (false == other.gameObject.TryGetComponent(out SKComponentAttackee attackee))
		    {
			    return;
		    }
		    
		    OnCollisionStart(attackee);
	    }

	    public void OnCollisionEnd(Collision2D other)
	    {
		    if (other.gameObject.layer == gameObject.layer)
		    {
			    return;
		    }
		    
		    if (false == other.gameObject.TryGetComponent(out SKComponentAttackee attackee))
		    {
			    return;
		    }
		    
		    OnCollisionEnd(attackee);
	    }

	    public void OnCollisionStart(Collider2D other)
	    {
		    if (other.gameObject.layer == gameObject.layer)
		    {
			    return;
		    }
		    
		    if (false == other.gameObject.TryGetComponent(out SKComponentAttackee attackee))
		    {
			    return;
		    }
		    
		    OnCollisionStart(attackee);
	    }

	    public void OnCollisionEnd(Collider2D other)
	    {
		    if (other.gameObject.layer == gameObject.layer)
		    {
			    return;
		    }
		    
		    if (false == other.gameObject.TryGetComponent(out SKComponentAttackee attackee))
		    {
			    return;
		    }
		    
		    OnCollisionEnd(attackee);
	    }

	    public void OnCollisionStart(SKComponentAttackee attackee)
	    {
		    if (SkObject.ObjectType == attackee.SkObject.ObjectType)
		    {
			    return;
		    }

		    var spawnId = attackee.SkObject.SpawnId;
		    if (_collisionStayObjectSpawnIdList.Contains(spawnId))
		    {
			    return;
		    }

		    ProcessCollision(attackee);
		    
		    _collisionStayObjectSpawnIdList.Add(spawnId);
	    }

	    public void OnCollisionEnd(SKComponentAttackee attackee)
	    {
		    if (SkObject.ObjectType == attackee.SkObject.ObjectType)
		    {
			    return;
		    }

		    _remainDotDamageCoolTimeBySpawnId.Remove(attackee.SkObject.SpawnId);
		    _collisionStayObjectSpawnIdList.Remove(attackee.SkObject.SpawnId);
	    }

	    private void OnCollisionEnter2D(Collision2D other)
	    {
		    OnCollisionStart(other);
	    }

	    private void OnCollisionExit2D(Collision2D other)
	    {
		    OnCollisionEnd(other);
	    }
	    
	    /*private void OnCollisionStay2D(Collision2D other)
	    {
		    OnCollision(other);
	    }*/

	    private void OnTriggerEnter2D(Collider2D other)
	    {
		    OnCollisionStart(other);
	    }
	    
	    private void OnTriggerExit2D(Collider2D other)
	    {
		    OnCollisionEnd(other);
	    }
	    
	    /*private void OnTriggerStay2D(Collider2D other)
	    {
		    OnCollision(other);
	    }*/

	    private void ProcessCollision()
	    {
		    foreach (var spawnId in _collisionStayObjectSpawnIdList)
		    {
			    var skObject = SKGameManager.Instance.ObjectManager.GetObjectBySpawnId(spawnId);
			    if (false == skObject)
			    {
				    continue;
			    }

			    if (false == skObject.TryGetSKComponent(out SKComponentAttackee attackee))
			    {
				    continue;
			    }
			    
			    _collisionStayObjectAttackeeListTemp.Add(attackee);
		    }

		    foreach (var attackee in _collisionStayObjectAttackeeListTemp)
		    {
			    if (false == ProcessCollision(attackee)) continue;

			    if (SKObject.SKObjectStateType.ReadyForDestroy <= SkObject.ObjectState)
			    {
				    break;
			    }
		    }
		    
		    _collisionStayObjectAttackeeListTemp.Clear();
	    }

	    private bool ProcessCollision(SKComponentAttackee attackee)
	    {
		    if (false == CanAttack(attackee))
		    {
			    return false;
		    }
		    
		    if (false == attackee.CanDamage(this))
		    {
			    return false;
		    }
			    
		    var damage = CalcDamage(attackee);
		    
		    OnAttack(attackee, damage);
		    attackee.OnDamage(this, damage);

		    if (isPlayerWeapon)
		    {
			    SKGameManager.Instance.AddDamageText(attackee.transform.position, damage);
		    }

		    return true;
	    }

	    private ulong CalcDamage(SKComponentAttackee attackee)
	    {
		    var damage = isPercentDamage 
			    ? SkObject.StatManager.GetStatResultValue(StatType.AttackPower) * defaultDamage / 100.0f
			    : defaultDamage;
		    
		    var decreaseDamagePercent = attackee.SkObject.StatManager.GetStatResultOnlyPercent(StatType.DecreaseDamage);
		    if (1.0f <= decreaseDamagePercent)
		    {
			    damage = 0f;
		    }
		    else
		    {
			    damage *= (1.0f - decreaseDamagePercent);
		    }
		    
		    var damageAsULong = checked((ulong)damage);
		    damageAsULong = Math.Max(damageAsULong, SKConstants.MinimumDamage);
		    return damageAsULong;
	    }

	    public bool CanAttack(SKComponentAttackee attackee)
	    {
		    if (SKObject.SKObjectStateType.Spawned != SkObject.ObjectState)
		    {
			    return false;
		    }

		    if (null != ignoreTypeList && ignoreTypeList.Contains(attackee.SkObject.ObjectType))
		    {
			    return false;
		    }
		    
		    if (_remainDotDamageCoolTimeBySpawnId.TryGetValue(attackee.SkObject.SpawnId, out var remainDamageCoolTime))
		    {
			    if (0 < remainDamageCoolTime)
			    {
				    return false;
			    }
		    }

		    return true;
	    }

	    public void OnAttack(SKComponentAttackee attackee, ulong damage)
	    {
		    if (0 < knockBackPower)
		    {
			    if (attackee.Rigidbody2D)
			    {
				    var moveVector = (attackee.transform.position - transform.position).normalized;
				    moveVector *= knockBackPower;
				    attackee.Rigidbody2D.AddForce(moveVector);
			    }
		    }

		    var dotDamageCoolTimeResult = dotDamageCoolTime < 0 ? float.MaxValue : dotDamageCoolTime;
		    var damageCoolTime = dotDamageCoolTimeResult * (1.0f - SkObject.StatManager.GetStatResultOnlyPercent(StatType.DecreaseAttackDotCoolTime));
		    if (0 < damageCoolTime)
		    {
			    _remainDotDamageCoolTimeBySpawnId.Add(attackee.SkObject.SpawnId, damageCoolTime);
		    }
		    
		    var eventParam = SKEventParam.GetOrNewParam<SKAttackEventParam>();
		    eventParam.Attackee = attackee;
		    eventParam.Damage = damage;
		    SkObject.EventManager.BroadCast(SKEventManager.SKEventType.Attack, eventParam);
		    
		    if (destroyOnAttack)
		    {
			    SkObject.DestroyObject();
		    }
	    }
    }
}
