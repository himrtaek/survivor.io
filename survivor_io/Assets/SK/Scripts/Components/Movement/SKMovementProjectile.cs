using System.Collections.Generic;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;

namespace SK
{
    public class SKMovementProjectile : SKMovementBase
    {
	    [SerializeField] private SKTargetHelper.SKTargetType targetType;
	    [SerializeField] private SKTargetHelper.SKObjectUpdateType objectUpdateType;
	    [SerializeField] private SKTargetHelper.SKPositionUpdateType positionUpdateType;
	    [SerializeField] private SKTargetHelper.SKConstraintFlag constraintFlag;
	    [SerializeField] private bool noTargetOnlyLeftRight;
	    [SerializeField] private bool rotationToTop;
	    [SKEditableField] [SerializeField] private float moveSpeed;
	    [SerializeField] private float rotationSpeed;
	    [SerializeField] private float rotationAcceleration;
	    [SerializeField] private float minRotationAcceleration;
	    [SerializeField] private float maxRotationAcceleration;
	    
	    private SKTargetHelper _targetHelper = new ();
	    private float _elapsedTime;
	    private Vector3 _prevDirection = Vector3.negativeInfinity;
	    private float _elapsedRotationAcceleration;

	    protected override void ImportFieldFromData()
	    {
		    if (SkObject.DataID <= 0)
		    {
			    return;
		    }
		    
		    /*var data = MOVEMENTPROJECTILEDATA.GetRow(SkObject.DataID, DataSubKey);
		    if (null == data)
		    {
			    return;      
		    }
		    
		    targetType = (SKTargetHelper.SKTargetType)data.TargetType;
		    objectUpdateType = (SKTargetHelper.SKObjectUpdateType)data.ObjectUpdateType;
		    positionUpdateType = (SKTargetHelper.SKPositionUpdateType)data.PositionUpdateType;

		    constraintFlag = SKTargetHelper.SKConstraintFlag.None;
		    if (0 < data.LockAngle45)
		    {
			    constraintFlag = (SKTargetHelper.SKConstraintFlag)BlackBoardCommon.AddFlag((int)constraintFlag, (int)SKTargetHelper.SKConstraintFlag.Angle45);
		    }

		    noTargetOnlyLeftRight = 0 < data.NoTargetOnlyLeftRight;
		    rotationToTop = 0 < data.RotationToTop;
		    moveSpeed = data.MoveSpeed / 100.0f;*/
	    }

	    protected  override List<string> ExportFieldToData()
	    {
		    return new List<string>()
		    {
			    ((int)targetType).ToString(),
			    ((int)objectUpdateType).ToString(),
			    ((int)positionUpdateType).ToString(),
			    constraintFlag.HasFlagNonAlloc(SKTargetHelper.SKConstraintFlag.Angle45) ? "1" : "0",
			    noTargetOnlyLeftRight ? "1" : "0",
			    rotationToTop ? "1" : "0",
			    ((int)(moveSpeed * 100)).ToString(),
		    };
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    SkObject.StatManager.AddStatData(StatType.ProjectileSpeed, StatExprType.Add, StatSourceType.Prefab, GetInstanceID(), moveSpeed);
		    
		    _elapsedTime = 0;
		    _elapsedRotationAcceleration = 0;
		    
		    _targetHelper.SetData(gameObject, targetType, objectUpdateType, positionUpdateType, constraintFlag);
		    UpdateTargetPosition();
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.StatManager.RemoveStatData(StatType.ProjectileSpeed, StatExprType.Add, StatSourceType.Prefab, GetInstanceID());
		    
		    base.OnSKObjectDestroy();
	    }

        public override void GameUpdate(float deltaTime)
        {
	        base.GameUpdate(deltaTime);
	        
	        if (_targetHelper.PositionUpdateType == SKTargetHelper.SKPositionUpdateType.Realtime)
	        {
		        UpdateTargetPosition();
	        }
	        else
	        {
		        _elapsedRotationAcceleration += rotationAcceleration * deltaTime;
		        _elapsedRotationAcceleration = Mathf.Clamp(_elapsedRotationAcceleration, minRotationAcceleration, maxRotationAcceleration);

		        var rotationSpeedResult = rotationSpeed + _elapsedRotationAcceleration;
		        if (0 != rotationSpeedResult)
		        {
			        _prevDirection = Quaternion.Euler(0, 0, rotationSpeedResult * deltaTime) * _prevDirection;
		        }
	        }
	        
	        var direction = _prevDirection;
	        if (direction == Vector3.zero)
	        {
		        return;
	        }

	        if (false == rotationToTop)
	        {
		        SetRotation(direction);
		        direction = Vector3.up;
	        }
	        
	        float moveSpeedStatValue = SkObject.StatManager.GetStatResultValue(StatType.ProjectileSpeed);
	        float speed = deltaTime * moveSpeedStatValue;
	        Translate(direction, speed);
        }

        private void UpdateTargetPosition()
        {
	        if (_targetHelper.TargetObject)
	        {
		        _prevDirection = _targetHelper.GetDirection(transform.position);   
	        }
	        else
	        {
		        if (noTargetOnlyLeftRight)
		        {
			        var isLeftView = SKGameManager.Instance.ObjectManager.ObjectPlayer.ObjectView.IsLeftVIew;
			        _prevDirection = isLeftView ? Vector3.left : Vector3.right;
		        }
		        else
		        {
			        _prevDirection = transform.rotation * Vector3.up;
		        }
	        }
        }
    }
}
