using Unity.VisualScripting;
using UnityEngine;

namespace SK
{
    public abstract class SKInstantMovementBase : ISKObjectLifeCycle
    {
	    public SKObject SkObject { get; }
	    public GameObject MyGameObject { get; }
	    
	    public virtual int Priority { get; }
	    public virtual bool IgnoreOtherMovement { get; }
	    public double CreateTime { get; }

	    public float LifeTime;

	    public float ElapsedTime { get; private set; }

	    protected SKInstantMovementBase(SKObject skObject, GameObject myGameObject)
	    {
		    SkObject = skObject;
		    MyGameObject = myGameObject;
		    CreateTime = Time.timeSinceLevelLoad;
	    }
	    
	    public virtual void OnSKObjectSpawn()
	    {
		    
	    }

	    public virtual void OnSKObjectReadyForDestroy()
	    {
		    
	    }

	    public virtual void OnSKObjectDestroy()
	    {
		    
	    }

	    public void AddElapsedTime(float deltaTime)
	    {
		    ElapsedTime += deltaTime;   
	    }

	    public virtual void GameUpdate(float deltaTime)
	    {
	    }

	    public virtual void OnStart()
	    {
		    
	    }
	    
	    public virtual void OnEnd()
	    {
		    
	    }
    }
}
