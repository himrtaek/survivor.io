using System.Collections.Generic;
using UnityEngine;

namespace SK
{
	
	[CreateAssetMenu(fileName = "SKPlayerBaseStatSetting", menuName = "ScriptableObject/SKPlayerBaseStatSetting")]
	public class SKPlayerBaseStatSetting : ScriptableObject
	{
		public const string FilePath = "SKPlayerData/SKPlayerBaseStatSetting";
		public List<SKStatSerializeData> statDataList = new();
	}
}
