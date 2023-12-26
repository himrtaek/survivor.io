using Unity.VisualScripting;

namespace SK
{
    public class SKItemSpawnManager
    {
	    private float _elapsedTime = 0;
	    private float _spawnTime = 5;
	    
	    public void Init()
	    {
		    
	    }

	    public void GameUpdate(float deltaTime)
	    {
		    _elapsedTime += deltaTime;

		    if (_spawnTime <= _elapsedTime)
		    {
			    _elapsedTime -= _spawnTime;
			    SpawnItemBox();
		    }
	    }

	    public void SpawnItemBox()
	    {
		    /*var itemType = Random.Range((int)SKIngameItemType.Magnet, (int)SKIngameItemType.Heal + 1);*/
		    var itemType = SKIngameItemType.RandomBox;
		    uint itemKey = 1;
		    var position = SKMonsterSpawnInfo.SpawnPositionConstraintType.AroundPlayer.CalcPosition();
		    position = SKGameManager.Instance.BackgroundManager.BackgroundClampPosition(position);
		    
		    SKObjectRandomItemBox.SpawnDropItem(itemType, itemKey, position);
	    }
    }
}
