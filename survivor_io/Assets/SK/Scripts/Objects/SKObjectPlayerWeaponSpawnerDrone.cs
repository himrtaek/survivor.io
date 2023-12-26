using UnityEngine;

namespace SK
{
    public class SKObjectPlayerWeaponSpawnerDrone : SKObjectPlayerWeaponSpawner
    {
	    [SerializeField] private float attackRange;
	    [SerializeField] private float randomRange;
	    [SerializeField] private float rotationSpeed;

	    private Transform _playerTransform;
	    private float _rotation;
	    
	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();

		    _playerTransform = SKGameManager.Instance.ObjectManager.ObjectPlayer.transform;
		    
		    SkObject.EventManager.AddListener(SKEventManager.SKEventType.SpawnChildObject, OnReceiveEvent);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.EventManager.RemoveListener(SKEventManager.SKEventType.SpawnChildObject, OnReceiveEvent);

		    _playerTransform = null;
		    
		    base.OnSKObjectDestroy();
	    }

	    private void OnReceiveEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    switch (eventParam)
		    {
			    case SKSpawnChildObjectEventParam spawnOtherObjectEventParam:
				    if (spawnOtherObjectEventParam.SpawnObject.TryGetComponent(
					        out SKObjectPlayerWeaponSpawnerDroneMissile droneMissile))
			    {
				    droneMissile.SetTargetPosition(GetTargetPosition());
			    }
				    break;
		    }
	    }

	    public override void GameUpdate(float deltaTime)
	    {
		    base.GameUpdate(deltaTime);
		    
		    _rotation += rotationSpeed * deltaTime;
		    
		    if (360 <= _rotation)
		    {
			    _rotation %= 360;
		    }
	    }

	    private Vector3 GetTargetPosition()
	    {
		    var position = _playerTransform.position;
		    var angleVector = Quaternion.AngleAxis(_rotation, Vector3.back) * Vector3.up;
		    position += angleVector * attackRange;
		    position += new Vector3(Random.Range(-randomRange, randomRange), Random.Range(-randomRange, randomRange), 0);
		    return position;
	    }
    }
}
