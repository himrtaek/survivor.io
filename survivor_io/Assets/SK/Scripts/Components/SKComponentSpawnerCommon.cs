using Unity.VisualScripting;

namespace SK
{
    public abstract class SKComponentSpawnerCommon : SKComponentBase
    {
	    public abstract void Init();
	    public abstract void SpawnerUpdate(float deltaTime);   
    }
}
