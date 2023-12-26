using JHT.Scripts.Common;
using UnityEngine;

namespace SK
{
    public abstract class SKComponentBase : MonoBehaviour, ISKObjectLifeCycle
    {
	    [SerializeField] private SKObject skObject;
	    public SKObject SkObject
	    {
		    get
		    {
			    if (false == skObject)
			    {
				    if (TryGetComponent(out skObject).IsFalse())
				    {
					    return null;
				    }
			    }
			    
			    return skObject;
		    }
	    }
	    
	    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	    protected virtual void Reset()
	    {
		    if(false == skObject)
		    {
			    TryGetComponent(out skObject);
		    }
	    }

	    protected virtual void Awake()
	    {
		    SkObject.AddSKComponent(this);
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

	    public virtual void GameUpdate(float deltaTime)
	    {
		    
	    }
    }
}
