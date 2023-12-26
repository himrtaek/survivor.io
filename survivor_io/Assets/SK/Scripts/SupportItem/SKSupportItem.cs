namespace SK
{
	public enum SKSupportItemType
	{
		ItemAcquisitionRange,
		GoldEarned,
		WeaponLifeTime,
		HealPer5Second,
		WeaponRange,
		MoveSpeed,
	}
	
	public class SKSupportItem
	{
		public SKObject SKObject;
		public SKSupportItemType SupportItemType;
		public SKSkill Skill;

		public static SKSupportItem CreateSupportItem(SKObject skObject, uint supportItemId, uint level)
		{
			var supportItemType = (SKSupportItemType)supportItemId;
			
			SKSupportItem supportItem = new()
			{
				SKObject = skObject,
				SupportItemType = supportItemType,
				Skill = new SKSkill((SKSkillType)supportItemId, level * 10),
			};

			return supportItem;
		}

		public void DoAction()
		{
			Skill.DoAction(SKObject, StatSourceType.SupportItem, GetHashCode());
		}

		public void UnDoAction()
		{
			Skill.UnDoAction(SKObject, StatSourceType.SupportItem, GetHashCode());
		}
	}
}
