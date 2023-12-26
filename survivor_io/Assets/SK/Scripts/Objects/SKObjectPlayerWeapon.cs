using Unity.VisualScripting;

namespace SK
{
	public interface iSKObjectLevel
	{
		public uint Level { get; set; }
	}
	
	public enum SKPlayerWeaponType
	{
		Start,
		
		Kunai,
		Revolver,
		RotationWheel,
		Bat,
		Boomerang,
		FireBottle,
		Shield,
		RocketLauncher,
		DroneA,
		DrillShot,
		ForceOfDestroy,
		
		End,
	}
	
    public class SKObjectPlayerWeapon : SKObjectWeapon, iSKObjectLevel
    {
	    public uint Level { get; set; }
	    
	    public override SKObjectType ObjectType { get; } = SKObjectType.PlayerWeapon;
    }
}
