using System;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SKMovementDrone : SKMovementBase
    {
	    [SerializeField] private SKComponentObjectView objectView;

	    public SKComponentObjectView ObjectView
	    {
		    get
		    {
			    if (false == objectView)
			    {
				    SkObject.TryGetSKComponent(out objectView);
			    }
			    
			    return objectView;
		    }
	    }
	    
	    enum Phase
	    {
		    Follow,
		    UpDown,
	    }
	    
	    [SerializeField] private Vector2 offset;
	    [SerializeField] private float moveSpeed;
	    [SerializeField] private float maxDistance;
	    [SerializeField] private float upDownLength;
	    [SerializeField] private float upDownSpeed;

	    private Transform _playerTransform;
	    private Vector3 _prevTargetPosition;
	    private float _upDownElapsedTime;
	    private Phase _phase;
	    
	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();

		    _playerTransform = SKGameManager.Instance.ObjectManager.ObjectPlayer.transform;
		    
		    SKGameManager.Instance.OnGameFixedUpdate += OnFixGameUpdate;
	    }
	    
	    public override void OnSKObjectReadyForDestroy()
	    {
		    SKGameManager.Instance.OnGameFixedUpdate -= OnFixGameUpdate;

		    _playerTransform = null;
		    
		    base.OnSKObjectReadyForDestroy();
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    _playerTransform = null;
		    
		    base.OnSKObjectDestroy();
	    }

	    protected override void ImportFieldFromData()
	    {
		    
	    }

	    protected override List<string> ExportFieldToData()
	    {
		    return new();
	    }

	    public void OnFixGameUpdate(float deltaTime)
	    {
		    switch (_phase)
		    {
			    case Phase.Follow:
				    ProcessFollow(deltaTime);
				    break;
			    case Phase.UpDown:
				    ProcessUpDown(deltaTime);
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    private void ProcessFollow(float deltaTime)
	    {
		    var curPosition = transform.position;
		    var targetPosition = _playerTransform.position + (Vector3)offset;
		    var targetDistance = Vector3.Distance(curPosition, targetPosition);
		    var dirVector = (targetPosition - curPosition).normalized;
		    var moveOffset = dirVector * (moveSpeed * deltaTime);
		    var newPosition = curPosition + moveOffset;
		    if (Vector3.Distance(_prevTargetPosition, targetPosition) < moveSpeed * deltaTime 
		        && targetDistance < moveSpeed * deltaTime)
		    {
			    newPosition = targetPosition;
			    _phase = Phase.UpDown;
			    _upDownElapsedTime = 0;
		    }
		    else
		    {
			    if (maxDistance < targetDistance)
			    {
				    newPosition = targetPosition + -dirVector * (maxDistance * 0.99f);
			    }
		    }
		    
		    var delta = targetPosition - transform.position;
		    ObjectView.LookAtLeftByDirectionValue(delta.x);
		    SetPosition(newPosition);
		    
		    _prevTargetPosition = targetPosition;
	    }

	    private void ProcessUpDown(float deltaTime)
	    {
		    var targetPosition = _playerTransform.position + (Vector3)offset;
		    if (moveSpeed * deltaTime < Vector3.Distance(_prevTargetPosition, targetPosition))
		    {
			    _phase = Phase.Follow;
			    return;
		    }

		    _prevTargetPosition = targetPosition;
		    _upDownElapsedTime += deltaTime * upDownSpeed;
		    
		    var yPos = Mathf.Sin(_upDownElapsedTime) * upDownLength;
		    targetPosition.y += yPos;

		    var delta = targetPosition - transform.position;
		    ObjectView.LookAtLeftByDirectionValue(delta.x);
		    SetPosition(targetPosition);
	    }
    }
}
