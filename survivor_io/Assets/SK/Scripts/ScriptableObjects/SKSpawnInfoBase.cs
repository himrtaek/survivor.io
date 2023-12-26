using System;
using JHT.Scripts.Attribute;
using UnityEngine;
using UnityEngine.Serialization;

namespace SK
{
	[Serializable]
	public class SKSpawnInfoBase
	{
		protected const string FoldoutGroupCommon = "공통";
		protected const string FoldoutGroupPosition = "포지션";
		protected const string FoldoutGroupRotation = "로테이션";
		protected const string FoldoutGroupScale = "스케일";
		
		[FoldoutGroup(FoldoutGroupCommon, true)]
		[LabelText("대상 오브젝트")] public GameObject gameObject;
		
		[FoldoutGroup(FoldoutGroupCommon, true)]
		[LabelText("스포너의 자식")] public bool isChild;
		
		[FoldoutGroup(FoldoutGroupPosition, true)]
		[LabelText("로컬 포지션 사용")] public bool isLocalPosition;
		
		[FoldoutGroup(FoldoutGroupPosition, true)]
		[LabelText("포지션")] public Vector2 position;
		
		[FoldoutGroup(FoldoutGroupPosition, true)]
		[LabelText("스포너 포지션 합산")] public bool addParentPosition;
		
		[FoldoutGroup(FoldoutGroupRotation, true)]
		[LabelText("로컬 로테이션 사용")] public bool isLocalRotation;
		
		[FoldoutGroup(FoldoutGroupRotation, true)]
		[LabelText("로테이션 Z")] public float rotationZ;
		
		[FoldoutGroup(FoldoutGroupRotation, true)]
		[LabelText("스포너 로테이션 합산")] public bool addParentRotation;
		
		[FoldoutGroup(FoldoutGroupScale, true)]
		[LabelText("스케일")] public Vector3 scale = Vector3.one;
	}
}
