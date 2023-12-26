namespace SK.Inventory
{
	public interface InventoryItemBase
	{
		public SlotType type { get; }
		public bool Equipment { get; }
		public string Description { get; }
	}
}
