using JHT.Scripts.Common;
using UnityEngine;
using UnityEngine.UI;

namespace JHT.Scripts
{
	public class WorldSpaceUI : MonoBehaviour
	{
		private bool _initialized;
		public Camera m_cam;
		public RectTransform rect;
		public RectTransform m_rectCanvas;

		public bool bUsePosition;
		public GameObject m_goTarget;
		public Vector3 Offset = Vector3.zero;

		public bool bUseScale;
		private float _osmMinZoomValue;
		private float _osmMaxZoomValue;
		public float m_fOriSize;
		public Vector3 m_vecOriScale = Vector3.one;

		public bool AutoCheckDistancePosZ = false;
		public bool ForceCheckDistancePosZ = false;
		private float _distanceScaleRate = 1;

		// Start is called before the first frame update
		void Start()
		{
			Init();
		}

		void Init()
		{
			if (true == _initialized)
			{
				return;
			}
			
			if (false == TryGetComponent(out RectTransform rect_))
			{
				rect_ = gameObject.AddComponent<RectTransform>();
				Debug.LogWarning("WorldSpaceUI : RectTransform이 없어서 Add, 리소스 수정 필요");
			}

			rect = rect_;
			m_vecOriScale = rect.localScale;

			_initialized = true;
		}

		public void Clear()
		{
			m_cam = null;
			m_goTarget = null;
			m_rectCanvas = null;
		}

		public bool SetTarget(Camera cam, RectTransform rectCanvas, GameObject goTarget)
		{
			return SetTarget(cam, rectCanvas, goTarget, Vector3.zero);
		}

		public bool SetTarget(Camera cam, RectTransform rectCanvas, GameObject goTarget, Vector3 offset)
		{
			if (null == cam)
			{
				return false;
			}

			if (null == goTarget)
			{
				return false;
			}

			if (null == rectCanvas)
			{
				return false;
			}

			m_cam = cam;
			m_goTarget = goTarget;
			Offset = offset;
			m_rectCanvas = rectCanvas;
			bUsePosition = true;

			LateUpdate();

			return true;
		}

		public bool SetEnableScale(Camera cam, float fSize)
		{
			if (null == cam)
			{
				return false;
			}

			m_cam = cam;
			m_fOriSize = fSize;
			bUseScale = true;

			return true;
		}

		public bool SetEnableOSMScale(Camera cam, float fSize, int osmMinZoomLevel = -1, int osmMaxZoomLevel = -1, float osmMinScale = -1, float osmMaxScale = -1)
		{
			if (null == cam)
			{
				return false;
			}

			m_cam = cam;
			m_fOriSize = fSize;
			bUseScale = true;
			
			return true;
		}

		// Update is called once per frame
		void LateUpdate()
		{
			if (false == _initialized)
			{
				Init();
			}
			
			if (true == bUsePosition)
			{
				UpdatePosition();
			}

			if (true == bUseScale)
			{
				UpdateScale();
			}
		}

		void UpdatePosition()
		{
			if (null == m_cam)
			{
				return;
			}

			if (null == rect)
			{
				return;
			}

			if (null == m_goTarget)
			{
				return;
			}

			SetWorldToCanvas(
				m_goTarget.transform.position + Offset, 
				SceneManager.Instance.CurScene.MainCamera, 
				SceneManager.Instance.CurScene.UICamera, 
				rect,
				m_rectCanvas);

			if (true == ForceCheckDistancePosZ || (true == AutoCheckDistancePosZ && null != m_goTarget.GetComponentInParent<Canvas>()))
			{
				var oriDistance = Vector3.Distance(rect.position, SceneManager.Instance.CurScene.UICamera.transform.position);

				var distanceUIToTarget = Vector3.Distance(rect.position, m_goTarget.transform.position);
				var distanceTargetToCam = Vector3.Distance(m_goTarget.transform.position, SceneManager.Instance.CurScene.UICamera.transform.position);

				rect.position = Vector3.MoveTowards(rect.position, SceneManager.Instance.CurScene.UICamera.transform.position, distanceUIToTarget + distanceTargetToCam / 2);
				var newDistance = Vector3.Distance(rect.position, SceneManager.Instance.CurScene.UICamera.transform.position);

				_distanceScaleRate = newDistance / oriDistance;

				rect.localScale = m_vecOriScale * _distanceScaleRate;
			}
			else
			{
				_distanceScaleRate = 1.0f;
			}
		}
		
		public static void SetWorldToCanvas(Vector3 worldPosition, Camera mainCamera, Camera uiCamera, Transform trThis, RectTransform rectCanvas = null)
		{
			if (mainCamera.IsNull())
				return;

			if (uiCamera.IsNull())
				return;

			if (trThis.IsNull())
				return;

			if (rectCanvas == null)
			{
				var goCanvas = GetComponentInRecursiveParent<Canvas>(trThis.gameObject).gameObject;
				goCanvas.TryGetComponent(out rectCanvas);
			}

			var screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, worldPosition);

			Vector2 uiLocalPosition;
			if (true == RectTransformUtility.ScreenPointToLocalPointInRectangle(rectCanvas, screenPoint, uiCamera,
				    out uiLocalPosition))
			{
				trThis.localPosition = uiLocalPosition;
			}
		}


		public static T GetComponentInRecursiveParent<T>(GameObject goParent) where T : Component
		{
			while (null != goParent)
			{
				if (true == goParent.TryGetComponent(out T comp))
				{
					return comp;
				}
				else
				{
					goParent = goParent.transform?.parent?.gameObject;
				}
			}

			return null;
		}

		void UpdateScale()
		{
			if (null == m_cam)
			{
				return;
			}

			if (null == rect)
			{
				return;
			}

			{
				float fScale = m_fOriSize / (m_cam.orthographic ? m_cam.orthographicSize : m_cam.fieldOfView);
			
				rect.localScale = m_vecOriScale * fScale * _distanceScaleRate;
			}
		}
	}
}
