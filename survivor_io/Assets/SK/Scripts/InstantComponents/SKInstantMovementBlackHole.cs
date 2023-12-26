using Unity.VisualScripting;
using UnityEngine;

namespace SK
{
    public class SKInstantMovementBlackHole : SKInstantMovementBase
    {
	    public override int Priority { get; } = 2;
	    public override bool IgnoreOtherMovement { get; } = true;

	    private Vector3 _targetPosition;
	    private float _distance;
	    private float _minDistance;
	    private Vector3 _dirVector;
	    private float _moveSpeed;
	    private float _rotationSpeed;
	    
	    public SKInstantMovementBlackHole(SKObject skObject, GameObject myGameObject,
		    Vector3 targetPosition,
		    float moveSpeed,
		    float rotationSpeed) : base(skObject, myGameObject)
	    {
		    var startPosition = myGameObject.transform.position;
		    _targetPosition = targetPosition;
		    _distance = Vector3.Distance(startPosition, targetPosition);
		    _minDistance = Random.Range(0.01f, 0.5f);
		    _dirVector = (startPosition - targetPosition).normalized;
		    _moveSpeed = moveSpeed;
		    _rotationSpeed = rotationSpeed;
	    }

	    public void OnGameFixedUpdate(float deltaTime)
	    {
		    _dirVector = Quaternion.AngleAxis(_rotationSpeed * deltaTime, Vector3.back) * _dirVector;
		    _distance -= _moveSpeed * deltaTime;
		    if (_distance < _minDistance)
		    {
			    _distance = _minDistance;
		    }

		    var offset = _dirVector * _distance;

		    /*if (SkObject is SKObjectCreature objectCreature)
		    {
			    var newPosition = offset + _targetPosition;
			    objectCreature.Rigidbody2D.MovePosition(newPosition);
		    }
		    else*/
		    {
			    MyGameObject.transform.position = offset + _targetPosition;   
		    }
	    }

	    public override void OnStart()
	    {
		    SKGameManager.Instance.OnGameFixedUpdate += OnGameFixedUpdate;
	    }
	    
	    public override void OnEnd()
	    {
		    SKGameManager.Instance.OnGameFixedUpdate -= OnGameFixedUpdate;
	    }
    }
}
