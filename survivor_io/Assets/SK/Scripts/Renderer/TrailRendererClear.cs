using UnityEngine;

namespace SK
{
	public class TrailRendererClear : MonoBehaviour
	{
		[SerializeField] private TrailRenderer trailRenderer;

		private void OnDisable()
		{
			trailRenderer.Clear();
		}
	}
}
