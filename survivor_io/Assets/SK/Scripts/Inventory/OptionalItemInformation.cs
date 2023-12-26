using System.Collections.Generic;

namespace SK.Inventory
{
	public abstract class OptionalItemInformation
	{
		public abstract string Name();
		public abstract string Description();
		public abstract int OpenGrade { get; }
	};
	
	public class SkillItem : OptionalItemInformation
	{
		protected int Grade;	// 등급
		protected SKSkillType _skillType;
		protected List<int> _skillValue;

		public override int  OpenGrade { get => Grade; }
		public override string Name()
		{
			return "스킬명";
		}

		public override string Description()
		{
			return "설명";
		}
	}	
}
