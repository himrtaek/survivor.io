using UnityEngine;

namespace SK
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SKComponentPlayerWeaponSpawner))]
    public class SKObjectPlayerWeaponSpawner : SKObject, iSKObjectLevel
    {
	    #region Cache

	    [SerializeField] private SKComponentPlayerWeaponSpawner playerWeaponSpawner;

	    public SKComponentPlayerWeaponSpawner PlayerWeaponSpawner
	    {
		    get
		    {
			    if (false == playerWeaponSpawner)
			    {
				    TryGetComponent(out playerWeaponSpawner);
			    }
			    
			    return playerWeaponSpawner;
		    }
	    }

	    #endregion
	    
	    [SerializeField] private uint level;

	    public uint Level
	    {
		    get => level;
		    set => level = value;
	    }
	    
	    public override SKObjectType ObjectType { get; } = SKObjectType.PlayerWeaponSpawner;
    }
}
