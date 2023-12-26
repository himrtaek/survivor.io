namespace SK.Inventory
{
	// 장착 가능한 슬롯
	public enum SlotType
	{
		none = 0,
		Weapon, // 무기
		Chest, // 가빠
		Arms, // 팔뚝
		Legs, // 바지
		Hands, // 장갑
		Feet, // 신발
		Neck, // 목걸이
		Belt, // 허리띠
		Jewel, // 보석
		LeftRing, // 왼쪽 반지
		RightRing, // 오른쪽 반지
		Recipe = 100, // 강화 재료 또는 기타
		Material	  // 다른 재료 
	};
}
