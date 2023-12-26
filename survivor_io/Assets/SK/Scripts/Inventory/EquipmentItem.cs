using System.Collections.Generic;

namespace SK.Inventory
{
	/// <summary>
	///  장착 아이템 정보
	/// </summary>
	public class EquipmentItem : InventoryItemBase
	{
		// 인터페이스용
		public SlotType type { get => itemType; }
		public bool Equipment { get => true; }
		public string Description { get => ""; }
		
		// 내부 변수
		protected SlotType itemType = SlotType.none;
		protected int level;
		protected int grade;
		
		protected List<(StatType, int)> State = new List<(StatType, int)>(); 
		protected List<OptionalItemInformation> Option = new List<OptionalItemInformation>();
	}
}
