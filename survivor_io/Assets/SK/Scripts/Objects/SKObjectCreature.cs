using System;
using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(SKComponentAttackee))]
    public abstract class SKObjectCreature : SKObject
    {
	    #region Cache
	    
	    [SerializeField] private Rigidbody2D _rigidbody2D;
	    public Rigidbody2D Rigidbody2D
	    {
		    get
		    {
			    if (false == _rigidbody2D)
			    {
				    TryGetComponent(out _rigidbody2D);
			    }
			    
			    return _rigidbody2D;
		    }
	    }

	    [SerializeField] private SKComponentAttackee componentAttackee;
	    public SKComponentAttackee ComponentAttackee
	    {
		    get
		    {
			    if (false == componentAttackee)
			    {
				    TryGetComponent(out componentAttackee);
			    }
			    
			    return componentAttackee;
		    }
	    }
	    
	    [SerializeField] private SKComponentObjectView objectView;

	    public SKComponentObjectView ObjectView
	    {
		    get
		    {
			    if (false == objectView)
			    {
				    TryGetSKComponent(out objectView);
			    }
			    
			    return objectView;
		    }
	    }
	    
	    protected override void Reset()
	    {
		    base.Reset();
		    
		    if(false == _rigidbody2D)
		    {
			    TryGetComponent(out _rigidbody2D);
		    }
		    
		    if(false == componentAttackee)
		    {
			    TryGetComponent(out componentAttackee);
		    }
	    }

	    #endregion

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    SkObject.EventManager.AddListener(SKEventManager.SKEventType.HPZero, OnReceiveEvent);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.EventManager.RemoveListener(SKEventManager.SKEventType.HPZero, OnReceiveEvent);
		    
		    base.OnSKObjectDestroy();
	    }

	    private void OnReceiveEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    switch (eventParam)
		    {
			    case SKHPZeroEventParam hpZeroEventParam:
				    var deathTime = ObjectView.PlayAnim(SKViewAnimNameType.Death);
				    hpZeroEventParam.Attackee.DestroyDelay = Math.Max(hpZeroEventParam.Attackee.DestroyDelay, deathTime); 
				    break;
		    }
	    }
    }
}
