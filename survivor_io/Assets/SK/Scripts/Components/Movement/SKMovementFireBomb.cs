using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SKMovementFireBomb : SKMovementBase
    {
	    [SerializeField] private float moveTime;
	    [SerializeField] private float distanceX = 1;
	    [SerializeField] private float distanceY = 1;
	    
	    private float _elapsedTime;
	    private Vector3 _moveStartPosition;
	    private Vector3 _moveCurvePosition;
	    private Vector3 _moveEndPosition;
	    
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
		    
		    _elapsedTime = 0;
		    _moveStartPosition = transform.position;
		    _moveCurvePosition = _moveStartPosition + transform.up * distanceY;
		    _moveEndPosition = _moveStartPosition + transform.right * distanceX;
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    base.OnSKObjectDestroy();
	    }

        public override void GameUpdate(float deltaTime)
        {
	        base.GameUpdate(deltaTime);
	        
	        _elapsedTime += deltaTime;
	        
	        Vector3 p1 = Vector3.Lerp(_moveStartPosition, _moveCurvePosition, _elapsedTime / moveTime);
	        Vector3 p2 = Vector3.Lerp(_moveCurvePosition, _moveEndPosition, _elapsedTime / moveTime);
	        var newPosition = Vector3.Lerp(p1, p2, _elapsedTime / moveTime);
	        
	        SetPosition(newPosition);
        }
    }
}
