using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SKMovementBoomerang : SKMovementBase
    {
	    [SerializeField] private float rotationSpeed;
	    [SerializeField] private float forwardDistance;
	    [SerializeField] private float forwardMoveTime;
	    [SerializeField] private float backwardDistance;
	    [SerializeField] private float backwardMoveTime;
	    
	    private SKTargetHelper _targetHelper = new();
	    private Vector2 _startPosition;
	    private Vector2 _forwardPosition;
	    private Vector2 _backwardPosition;
	    private bool _backwardProgress;
	    private float _elapsedTime;
	    private bool _moveDone;
	    
	    protected override void ImportFieldFromData()
	    {
		    
	    }

	    protected override List<string> ExportFieldToData()
	    {
		    return null;
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    Vector3 direction;
		    _targetHelper.SetData(gameObject, SKTargetHelper.SKTargetType.NearestMonster, SKTargetHelper.SKObjectUpdateType.Once, SKTargetHelper.SKPositionUpdateType.Once);
		    if (_targetHelper.TargetObject)
		    {
			    direction = _targetHelper.GetDirection(transform.position);   
		    }
		    else
		    {
			    direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0).normalized;
		    }

		    var position = transform.position;
		    _startPosition = position;
		    _forwardPosition = position + direction * forwardDistance;
		    _backwardPosition =  position + (-1 * direction * backwardDistance);
		    _backwardProgress = false;
		    _elapsedTime = 0;
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    base.OnSKObjectDestroy();
	    }

        public override void GameUpdate(float deltaTime)
        {
	        base.GameUpdate(deltaTime);
	        
	        _elapsedTime += deltaTime;
	        
	        float speed = deltaTime * rotationSpeed;
	        Rotate(speed);

	        var startPosition = _backwardProgress ? _forwardPosition : _startPosition;
	        var endPosition = _backwardProgress ? _backwardPosition : _forwardPosition;
	        var moveTime = _backwardProgress ? backwardMoveTime : forwardMoveTime;

	        SetPosition(Vector3.Lerp(startPosition, endPosition, _elapsedTime / moveTime));
	        
	        if (moveTime <= _elapsedTime)
	        {
		        if (false == _backwardProgress)
		        {
			        _backwardProgress = true;
			        _elapsedTime = 0;
		        }
	        }
        }
    }
}
