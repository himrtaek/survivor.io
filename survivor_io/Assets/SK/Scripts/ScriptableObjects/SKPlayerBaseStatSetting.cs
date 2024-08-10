using System.Collections.Generic;
using UnityEngine;

namespace SK
{
	
	[CreateAssetMenu(fileName = "PlayerBaseStatSetting", menuName = "ScriptableObject/PlayerBaseStatSetting")]
	public class SKPlayerBaseStatSetting : ScriptableObject
	{
		public const string FilePath = "Player/PlayerBaseStatSetting";
		public List<SKStatSerializeData> statDataList = new();
	}
}
