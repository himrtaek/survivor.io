using System;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SKMovementFollow : SKCreatureMovementBase
    {
	    [SerializeField] private SKTargetHelper.SKTargetType targetType;
	    [SerializeField] private SKTargetHelper.SKObjectUpdateType objectUpdateType;
	    [SerializeField] private SKTargetHelper.SKPositionUpdateType positionUpdateType;
	    [SKEditableField] [SerializeField] private float moveStartDelay;
	    [SKEditableField] [SerializeField] private float moveSpeed;
	    [SKEditableField] [SerializeField] private float freezeTimeOnDamage = 0.5f;

	    private SKTargetHelper _targetHelper = new();
	    private float _remainFreezeTime = 0.0f;
	    private float _elapsedTime;
	    
	    protected override void ImportFieldFromData()
	    {
		    if (SkObject.DataID <= 0)
		    {
			    return;
		    }
	    }

	    protected override List<string> ExportFieldToData()
	    {
		    return new List<string>()
		    {
			    ((int)targetType).ToString(),
			    ((int)objectUpdateType).ToString(),
			    ((int)positionUpdateType).ToString(),
			    ((int)(moveSpeed * 100)).ToString(),
			    ((int)(freezeTimeOnDamage * 100)).ToString(),
		    };
	    }

	    public override void OnSKObjectSpawn()
        {
	        base.OnSKObjectSpawn();
	        
	        SKGameManager.Instance.OnGameFixedUpdate += (OnGameFixedUpdate);

	        _elapsedTime = 0;
	        _remainFreezeTime = 0;
		    
	        SkObject.StatManager.AddStatData(StatType.MoveSpeed, StatExprType.Add, StatSourceType.Prefab, GetInstanceID(), moveSpeed);
	        
	        SkObject.EventManager.AddListener(SKEventManager.SKEventType.Damage, OnReceiveEvent);
	        
	        _targetHelper.SetData(gameObject, targetType, objectUpdateType, positionUpdateType);
        }

	    public override void OnSKObjectReadyForDestroy()
	    {
		    SKGameManager.Instance.OnGameFixedUpdate -= (OnGameFixedUpdate);
		    
		    base.OnSKObjectReadyForDestroy();
	    }
	    
	    public override void OnSKObjectDestroy()
        {
	        SkObject.EventManager.RemoveListener(SKEventManager.SKEventType.Damage, OnReceiveEvent);
		    
	        SkObject.StatManager.RemoveStatData(StatType.MoveSpeed, StatExprType.Add, StatSourceType.Prefab, GetInstanceID());
	        
	        base.OnSKObjectDestroy();
        }

        public void OnReceiveEvent(SKObject skObject, SKEventParam eventParam)
        {
	        switch (eventParam)
	        {
		        case SKDamageEventParam damageEventParam:
			        OnDamage(damageEventParam);
			        break;
	        }
        }

        public void OnDamage(SKDamageEventParam damageEventParam)
        {
	        _remainFreezeTime = freezeTimeOnDamage;
        }

        public void OnGameFixedUpdate(float deltaTime)
        {
	        _elapsedTime += deltaTime;
	        if (_elapsedTime < moveStartDelay)
	        {
		        return;
	        }
	        
	        if (0 < _remainFreezeTime)
	        {
		        _remainFreezeTime -= Math.Min(deltaTime, _remainFreezeTime);
		        return;
	        }
	        
	        Vector3 direction = _targetHelper.GetDirectionWithNearCheck(transform.position);
	        if (direction == Vector3.zero)
	        {
		        return;
	        }

	        float moveSpeedStatValue = SkObject.StatManager.GetStatResultValue(StatType.MoveSpeed);
	        float speed = deltaTime * moveSpeedStatValue;
	        
	        Translate(SkObjectCreature.Rigidbody2D, direction, speed);
	        SkObjectCreature.ObjectView.LookAtLeftByDirectionValue(direction.x);
        }
    }
}
