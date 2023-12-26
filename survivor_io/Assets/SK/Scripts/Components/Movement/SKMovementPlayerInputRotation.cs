using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SKMovementPlayerInputRotation : SKMovementBase
    {
	    protected override void ImportFieldFromData()
	    {
		    
	    }

	    protected override List<string> ExportFieldToData()
	    {
		    return null;
	    }
	    
	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    SKGameManager.Instance.PlayerInput.OnInputDirection.AddListener(OnInputDirection);
	    }
	    public override void OnSKObjectDestroy()
	    {
		    SKGameManager.Instance.PlayerInput.OnInputDirection.RemoveListener(OnInputDirection);
		    
		    base.OnSKObjectDestroy();
	    }

        void OnInputDirection(Vector3 direction)
        {
	        SetRotation(direction);
        }
    }
}
