using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(SKComponentLifeTime))]
	[RequireComponent(typeof(SKComponentAttacker))]
    public abstract class SKObjectWeapon : SKObject
    {
	    #region Cache

	    [SerializeField] private SKComponentAttacker componentAttacker;
	    public SKComponentAttacker ComponentAttacker
	    {
		    get
		    {
			    if (false == componentAttacker)
			    {
				    TryGetComponent(out componentAttacker);
			    }
			    
			    return componentAttacker;
		    }
	    }

	    [SerializeField] private SKComponentLifeTime componentLifeTime;
	    public SKComponentLifeTime ComponentLifeTime
	    {
		    get
		    {
			    if (false == componentLifeTime)
			    {
				    TryGetComponent(out componentLifeTime);
			    }
			    
			    return componentLifeTime;
		    }
	    }
	    
	    protected override void Reset()
	    {
		    base.Reset();
		    
		    if(false == componentAttacker)
		    {
			    TryGetComponent(out componentAttacker);
		    }
		    
		    if(false == componentLifeTime)
		    {
			    TryGetComponent(out componentLifeTime);
		    }
	    }

	    #endregion
    }
}
