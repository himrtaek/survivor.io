using System;
using System.Collections.Generic;
using Cysharp.Text;
using JHT.Scripts.Attribute;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;

namespace SK
{
	[Serializable]
	public class SKSpawnInfo : SKSpawnInfoBase
	{
		[FoldoutGroup(FoldoutGroupCommon, true)]
		[LabelText("시간")] public float spawnTime;
	}
	
	[CreateAssetMenu(fileName = "SKSpawnSetting", menuName = "ScriptableObject/SKSpawnSetting")]
	public class SKSpawnSetting : ScriptableObject
	{
		public static string GetFilePathByItemId(SKIngameItemType itemType, uint itemKey)
		{
			/*if (itemType == SKIngameItemType.RandomBox)*/
			{
				return ZString.Concat("SKDropItemData/Item", itemType.ToStringCached(), itemKey.ToStringCached(), ".asset");
			}
			/*else
			{
				return ZString.Concat("SKDropItemData/DropItemBase.asset");	
			}*/
		}
		
		public List<SKSpawnInfo> SpawnInfos = new();
		
		public bool Repeat;
		public float RepeatTime;
	}
}
