using System.Collections.Generic;
using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(SKObjectPlayerWeaponSpawnerDroneMissile))]
    public class SKMovementDroneMissile : SKMovementBase
    {
	    [SerializeField] private float phase1MoveTime;
	    [SerializeField] private float phase2MoveTime;
	    
	    public Vector3 TargetPosition { get; set; }
	    
	    private float _elapsedTime;
	    private Vector3 _prevPosition;
	    private Vector3 _moveStartPosition;
	    private Vector3 _moveCurvePosition;
	    private Vector3 _moveEndPosition;
	    private uint _phase;
	    
	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    _elapsedTime = 0;

		    _phase = 1;
		    
		    var position = transform.position;
		    _moveStartPosition = position;
		    
		    position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
		    _moveEndPosition = position;
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    base.OnSKObjectDestroy();
	    }

	    protected override void ImportFieldFromData()
	    {
		    
	    }

	    protected override List<string> ExportFieldToData()
	    {
		    return new();
	    }

	    public override void GameUpdate(float deltaTime)
	    {
		    base.GameUpdate(deltaTime);
	        
		    _elapsedTime += deltaTime;

		    switch (_phase)
		    {
			    case 1:
				    Phase1Process();
				    break;
			    case 2:
				    Phase2Process();
				    break;
		    }
	    }

	    private void Phase1Process()
	    {
		    var newPosition = Vector3.Lerp(_moveStartPosition, _moveEndPosition, _elapsedTime / phase1MoveTime);
	        
		    _prevPosition = transform.position;
		    SetPosition(newPosition);
		    SetRotation((newPosition - _prevPosition).normalized);

		    if (phase1MoveTime <= _elapsedTime)
		    {
			    _phase = 2;
			    _elapsedTime = 0;
			    _moveStartPosition = transform.position;
		    
			    var angleVector = Quaternion.AngleAxis(Random.Range(0, 2) == 0 ? 90 : -90, Vector3.back) * (TargetPosition - _moveStartPosition).normalized;
		    
			    _moveCurvePosition = _moveStartPosition + angleVector;
			    _moveEndPosition = TargetPosition;
		    }
	    }

	    private void Phase2Process()
	    {
		    Vector3 p1 = Vector3.Lerp(_moveStartPosition, _moveCurvePosition, _elapsedTime / phase2MoveTime);
		    Vector3 p2 = Vector3.Lerp(_moveCurvePosition, _moveEndPosition, _elapsedTime / phase2MoveTime);
		    var newPosition = Vector3.Lerp(p1, p2, _elapsedTime / phase2MoveTime);

		    _prevPosition = transform.position;
		    SetPosition(newPosition);
		    SetRotation((newPosition - _prevPosition).normalized);

		    if (phase2MoveTime <= _elapsedTime)
		    {
			    SkObject.DestroyObject();
		    }
	    }
    }
}
