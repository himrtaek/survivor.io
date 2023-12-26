using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(CircleCollider2D))]
    public class SKComponentDropItemCapture : SKComponentBase
    {
	    #region Cache

	    [SerializeField] private CircleCollider2D _collider2D;
	    public CircleCollider2D CircleCollider2D
	    {
		    get
		    {
			    if (false == _collider2D)
			    {
				    TryGetComponent(out _collider2D);
			    }
			    
			    return _collider2D;
		    }
	    }
	    
	    protected override void Reset()
	    {
		    base.Reset();
		    
		    if(false == _collider2D)
		    {
			    TryGetComponent(out _collider2D);
		    }
	    }

	    #endregion

	    private float _colliderRadius;
	    
	    protected override void Awake()
	    {
		    base.Awake();
		    
		    _colliderRadius = CircleCollider2D.radius;
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    OnChangeItemAcquisitionRange(SkObject, StatType.ItemAcquisitionRange, 0, SkObject.StatManager.GetStatResultOnlyPercent(StatType.ItemAcquisitionRange));
		    SkObject.StatManager.AddListener(StatType.ItemAcquisitionRange, OnChangeItemAcquisitionRange);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.StatManager.RemoveListener(StatType.ItemAcquisitionRange, OnChangeItemAcquisitionRange);
		    
		    base.OnSKObjectDestroy();
	    }

	    public void OnChangeItemAcquisitionRange(SKObject skObject, StatType statType, float prevValue, float newValue)
	    {
		    CircleCollider2D.radius = _colliderRadius + _colliderRadius * newValue;
	    }
	    
	    public void OnCollision(Collider2D other)
	    {
		    if (false == other.gameObject.TryGetComponent(out SKObjectDropItem itemObject))
		    {
			    return;
		    }
		    
		    OnCollision(itemObject);
	    }

	    public void OnCollision(SKObjectDropItem objectDropItem, bool isMagnet = false)
	    {
		    if (false == CanCapture(objectDropItem))
		    {
			    return;
		    }
		    
		    if (false == objectDropItem.CanCapture(this, isMagnet))
		    {
			    return;
		    }
		    
		    objectDropItem.OnCaptured(this);
	    }

        private void OnTriggerEnter2D(Collider2D other)
        {
	        OnCollision(other);
        }
	    
        private void OnTriggerStay2D(Collider2D other)
        {
	        OnCollision(other);
        }

        public bool CanCapture(SKObjectDropItem objectDropItem)
        {
	        if (SKObject.SKObjectStateType.ReadyForDestroy <= SkObject.ObjectState)
	        {
		        return false;
	        }
	        
	        return true;
        }
    }
}
