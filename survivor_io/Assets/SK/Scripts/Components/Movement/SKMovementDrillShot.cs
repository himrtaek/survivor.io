using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SKMovementDrillShot : SKMovementBase
    {
	    [SKEditableField] [SerializeField] private float moveSpeed;
	    private int _ignoreLieIndex;
	    private Rect _bgRect;
	    private Vector3 _rotationDirection;

	    protected override void ImportFieldFromData()
	    {
		    if (SkObject.DataID <= 0)
		    {
			    return;
		    }
	    }

	    protected  override List<string> ExportFieldToData()
	    {
		    return new List<string>();
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    SkObject.StatManager.AddStatData(StatType.ProjectileSpeed, StatExprType.Add, StatSourceType.Prefab, GetInstanceID(), moveSpeed);

		    _bgRect = SKGameManager.Instance.BackgroundManager.BGRect;
		    _ignoreLieIndex = -1;
		    _rotationDirection = (new Vector2(Random.Range(0.01f, 1.0f), Random.Range(0.01f, 1.0f))).normalized;
		    SetRotation(_rotationDirection);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.StatManager.RemoveStatData(StatType.ProjectileSpeed, StatExprType.Add, StatSourceType.Prefab, GetInstanceID());
		    
		    base.OnSKObjectDestroy();
	    }

        public override void GameUpdate(float deltaTime)
        {
	        base.GameUpdate(deltaTime);

	        CheckCollision();
	        
	        var direction = Vector3.up;
	        var moveSpeedStatValue = SkObject.StatManager.GetStatResultValue(StatType.ProjectileSpeed);
	        var speed = deltaTime * moveSpeedStatValue;
	        
	        Translate(direction, speed);
        }

        private void CheckCollision()
        {
	        bool CalcCollisionNormal(Vector3 position, out Vector2 normal, out Vector3 collisionPosition)
	        {
		        for (int i = 0; i < 4; i++)
		        {
			        if (i == _ignoreLieIndex)
			        {
				        continue;
			        }
			        
			        var lineStartScreenPosition = Vector2.zero;
			        normal = Vector2.zero;
			        var width = Screen.width;
			        var height = Screen.height;
			        var checkX = false;
			        var checkLess = false;
			        switch (i)
			        {
				        case 0:
					        lineStartScreenPosition = new Vector2(0, 0);
					        normal = Vector2.up;
					        checkX = false;
					        checkLess = true;
					        break;
				        case 1:
					        lineStartScreenPosition = new Vector2(width, 0);
					        normal = Vector2.left;
					        checkX = true;
					        checkLess = false;
					        break;
				        case 2:
					        lineStartScreenPosition = new Vector2(width, height);
					        normal = Vector2.down;
					        checkX = false;
					        checkLess = false;
					        break;
				        case 3:
					        lineStartScreenPosition = new Vector2(0, height);
					        normal = Vector2.right;
					        checkX = true;
					        checkLess = true;
					        break;
			        }
			        
			        var lineStartWorldPosition = Camera.main.ScreenToWorldPoint(lineStartScreenPosition);
			        var targetPosition = position;
			        
			        switch (i)
			        {
				        case 0:
					        targetPosition.y = Mathf.Max(lineStartWorldPosition.y, _bgRect.yMin);
					        break;
				        case 1:
					        targetPosition.x= Mathf.Min(lineStartWorldPosition.x, _bgRect.xMax);
					        break;
				        case 2:
					        targetPosition.y = Mathf.Min(lineStartWorldPosition.y, _bgRect.yMax);
					        break;
				        case 3:
					        targetPosition.x = Mathf.Max(lineStartWorldPosition.x, _bgRect.xMin);
					        break;
			        }
			        
			        var targetPositionValue = checkX ? targetPosition.x : targetPosition.y;
			        var currentPositionValue = checkX ? position.x : position.y;
			        if ((checkLess && currentPositionValue <= targetPositionValue)
			            || (!checkLess && targetPositionValue <= currentPositionValue))
			        {
				        collisionPosition = targetPosition;
				        _ignoreLieIndex = i;
				        return true;
			        }
		        }
		        
		        normal = Vector2.zero;
		        collisionPosition = Vector3.zero;
		        return false;
	        }
	        
	        bool CalcCollisionReflect(Vector3 position, Vector3 inputDirection, out Vector3 outputDirection, out Vector3 reflectPosition)
	        {
		        if (CalcCollisionNormal(position, out var collisionNormal, out var collisionPosition))
		        {
			        outputDirection = Vector2.Reflect(inputDirection, collisionNormal);
			        reflectPosition = collisionPosition;
			        return true;   
		        }

		        outputDirection = Vector3.zero;
		        reflectPosition = Vector3.zero;
		        return false;
	        }

	        if (CalcCollisionReflect(transform.position, _rotationDirection, out var rotationDirection, out var position))
	        {
		        _rotationDirection = rotationDirection;
		        SetRotation(_rotationDirection);
		        SetPosition(position);
	        }
        }
    }
}
