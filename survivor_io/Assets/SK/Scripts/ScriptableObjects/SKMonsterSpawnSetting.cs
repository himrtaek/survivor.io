using System;
using System.Collections.Generic;
using Cysharp.Text;
using JHT.Scripts.Attribute;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SK
{
	public static class SpawnPositionConstraintExtension
	{
		public static Vector3 CalcPosition(this SKMonsterSpawnInfo.SpawnPositionConstraintType spawnPositionConstraint)
		{
			switch (spawnPositionConstraint)
			{
				case SKMonsterSpawnInfo.SpawnPositionConstraintType.Anywhere:
				{
					var rect = SKGameManager.Instance.GetCameraVisibleRect();
					rect.size *= 3.0f;
					return new Vector3(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax), 0);
				}
				case SKMonsterSpawnInfo.SpawnPositionConstraintType.InCamera:
				{
					var rect = SKGameManager.Instance.GetCameraVisibleRect();
					return new Vector3(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax), 0);
				}
				case SKMonsterSpawnInfo.SpawnPositionConstraintType.OutCamera:
				{
					var offsetY = Random.Range(-1, 2);
					var offsetX = Random.Range(-1, 2);
					if (offsetX == 0 && offsetY == 0)
					{
						offsetX = 1;
						offsetY = 0;
					}
					var rect = SKGameManager.Instance.GetCameraVisibleRect();
					rect.position += new Vector2(offsetX * rect.width, offsetY * rect.height);
					return new Vector3(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax), 0);
				}
				case SKMonsterSpawnInfo.SpawnPositionConstraintType.AroundPlayer:
				{
					var width = 3;
					var height = 3;
					var playerPosition = SKGameManager.Instance.ObjectManager.ObjectPlayer.transform.position;
					var rect = new Rect(playerPosition.x - width / 2.0f, playerPosition.y - height / 2.0f, width, height);
					return new Vector3(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax), 0);
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(spawnPositionConstraint), spawnPositionConstraint, null);
			}
		}
	}
	
	[Serializable]
	public class SKMonsterSpawnInfo
	{
		public enum SpawnPositionConstraintType
		{
			[LabelText("카메라 박스 내부 외부 랜덤")]
			Anywhere,
			
			[LabelText("카메라 박스 내부 랜덤")]
			InCamera,
			
			[LabelText("카메라 박스 외부 랜덤")]
			OutCamera,
			
			[LabelText("플레이어 주변")]
			AroundPlayer
		}

		[Serializable]
		public class SKMonsterSpawnStat
		{
			public StatType StatType;
			public StatExprType StatExprType;
			public long StatValue;
		}

		[Serializable]
		public class SKMonsterSpawnOverrideInfo
		{
			public ulong? AttackPower;
			public ulong? Hp;
			public uint? DropItemPoolId;
		}
		
		[LabelText("메모")] public string memo;
		[LabelText("대상 오브젝트")] public GameObject gameObject;
		[LabelText("시작 시간(초)")] public uint spawnStartSecond;
		[LabelText("종료 시간(초)")] public uint spawnEndSecond;
		[LabelText("초당 스폰 수")] public uint spawnCountPerSecond;
		[LabelText("스폰 위치")] public SpawnPositionConstraintType positionConstraint;
		[LabelText("스텟 정보")] public List<SKMonsterSpawnStat> statList;
		[LabelText("데이터 오버라이드 정보")] public SKMonsterSpawnOverrideInfo overrideInfo;
	}
	
	[CreateAssetMenu(fileName = "SKMonsterSpawnSetting", menuName = "ScriptableObject/SKMonsterSpawnSetting")]
	public class SKMonsterSpawnSetting : ScriptableObject
	{
		public static string GetFilePathByStageId(uint stageId)
		{
			return ZString.Concat("SKStageData_", stageId.ToStringCached(), "/SKMonsterSpawnSetting");
		}
		
		public List<SKMonsterSpawnInfo> SpawnInfos = new();
	}
}
