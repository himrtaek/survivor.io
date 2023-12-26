using UnityEngine;

namespace SK
{
    public class SKComponentAffectWeaponRange : SKComponentBase
    {
	    [SerializeField] private bool realTimeUpdate;

	    private Vector3 _scale;

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    if (realTimeUpdate)
		    {
			    SKGameManager.Instance.PlayerStatManager.AddListener(StatType.WeaponRange, OnWeaponRangeChange);   
		    }
		    
		    _scale = transform.localScale;
		    
		    var weaponScale = SKGameManager.Instance.PlayerStatManager.GetStatResultOnlyPercent(StatType.WeaponRange);
		    OnWeaponRangeChange(SkObject, StatType.WeaponRange, 0, weaponScale);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    transform.localScale = _scale;
		    
		    if (realTimeUpdate)
		    {
			    SKGameManager.Instance.PlayerStatManager.RemoveListener(StatType.WeaponRange, OnWeaponRangeChange);   
		    }
		    
		    base.OnSKObjectDestroy();
	    }

	    private void OnWeaponRangeChange(SKObject skObject, StatType statType, float _, float newValue)
	    {
		    transform.localScale = _scale * (1.0f + newValue);
	    }
    }
}
