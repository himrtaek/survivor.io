using System.Collections.Generic;
using UnityEngine;

namespace SK
{
	[DisallowMultipleComponent]
    public class SKComponentLifeTime : SKComponentFromDataBase
    {
	    [SKEditableField] [SerializeField] private float lifeTime;
	    [SKEditableField] [SerializeField] private bool affectWeaponLifeTime;
	    private float _elapsedTime;
	    private float _lifeTimeResult;

	    protected override void ImportFieldFromData()
	    {
		    if (SkObject.DataID <= 0)
		    {
			    return;
		    }
	    }

	    protected  override List<string> ExportFieldToData()
	    {
		    return new List<string>()
		    {
			    ((int)(lifeTime * 100)).ToString(),
			    affectWeaponLifeTime ? "1" : "0",
		    };
	    }
	    
	    protected override void Awake()
	    {
		    base.Awake();
		    
		    _lifeTimeResult = lifeTime;
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    if (affectWeaponLifeTime)
		    {
			    var weaponLifeTime =
				    SKGameManager.Instance.PlayerStatManager.GetStatResultOnlyPercent(StatType.WeaponLifeTime);
			    _lifeTimeResult = lifeTime + lifeTime * weaponLifeTime;
		    }
		    
		    _elapsedTime = 0;
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    base.OnSKObjectDestroy();
	    }

        public override void GameUpdate(float deltaTime)
        {
	        base.GameUpdate(deltaTime);
	        
	        _elapsedTime += deltaTime;
	        
	        if (0 < _lifeTimeResult)
	        {
		        if (_lifeTimeResult <= _elapsedTime)
		        {
			        SkObject.DestroyObject();
		        }
	        }
        }
    }
}
