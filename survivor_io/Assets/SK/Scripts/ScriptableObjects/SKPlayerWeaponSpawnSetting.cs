using System;
using JHT.Scripts.Attribute;

namespace SK
{
	[Serializable]
	public class SKPlayerWeaponSpawnInfo : SKSpawnInfoBase
	{
		protected const string FoldoutGroupPlayerWeapon = "플레이어 무기";
		
		[FoldoutGroup(FoldoutGroupCommon, true)]
		[LabelText("스폰 수량")] public uint spawnCount;
		
		[FoldoutGroup(FoldoutGroupCommon, true)]
		[LabelText("스폰 시작 딜레이")] public int spawnStartDelay;
		
		[FoldoutGroup(FoldoutGroupCommon, true)]
		[LabelText("스폰 기간")] public float spawnDuration;
		
		[FoldoutGroup(FoldoutGroupCommon, true)]
		[LabelText("스폰 반복 여부")] public bool spawnRepeat;
		
		[FoldoutGroup(FoldoutGroupCommon, true)]
		[LabelText("스폰 반복 쿨타임")] public float spawnRepeatCoolTime;
		
		[FoldoutGroup(FoldoutGroupPlayerWeapon, true)]
		[LabelText("무기 범위 증가 적용")] public bool affectWeaponRange;
	}
}
