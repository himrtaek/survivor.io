namespace SK
{
    public interface ISKObjectLifeCycle
    {
	    public void OnSKObjectSpawn();

	    public void OnSKObjectReadyForDestroy();

	    public void OnSKObjectDestroy();

	    public void GameUpdate(float deltaTime);
    }
}
