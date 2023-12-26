using UnityEngine;

namespace SK
{
    public class SKObjectPlayerWeaponSpawnerDroneMissile : SKObjectPlayerWeaponSpawner
    {
	    [SerializeField] private GameObject targetObject;
	    [SerializeField] private GameObject explosionObject;
	    
	    private Vector3 _targetPosition;
	    public Vector3 TargetPosition => _targetPosition;
	    
	    private SKObject _spawnedTargetObject;

	    public void SetTargetPosition(Vector3 targetPosition)
	    {
		    _targetPosition = targetPosition;

		    if (TryGetSKComponent(out SKMovementDroneMissile movementDroneMissile))
		    {
			    movementDroneMissile.TargetPosition = _targetPosition;
		    }
		    
		    _spawnedTargetObject = SKGameManager.Instance.ObjectManager.SpawnObject(targetObject, this, false, (targetObjectTemp) =>
		    {
			    targetObjectTemp.transform.position = _targetPosition;
		    });
	    }
	    
	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    _spawnedTargetObject.DestroyObject();
		    _spawnedTargetObject = null;
		    
		    SKGameManager.Instance.ObjectManager.SpawnObject(explosionObject, this, false, (skObjectTemp) =>
		    {
			    skObjectTemp.transform.position = _targetPosition;
			    SKPlayerWeaponSpawner.OnSpawn(skObjectTemp, Level, false);
		    });
		    
		    _targetPosition = Vector3.zero;
		    
		    base.OnSKObjectDestroy();
	    }
    }
}
