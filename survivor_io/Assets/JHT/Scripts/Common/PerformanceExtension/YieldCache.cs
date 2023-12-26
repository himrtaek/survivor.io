using System.Collections.Generic;
using UnityEngine;


namespace JHT.Scripts.Common.PerformanceExtension
{
	static class YieldCache
	{
		public static readonly WaitForEndOfFrame WaitForEndOfFrame = new();
		public static readonly WaitForFixedUpdate WaitForFixedUpdate = new();
	
		private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsByTime = new();
		private static readonly Dictionary<float, WaitForSecondsRealtime> WaitForSecondsRealtimeByTime = new();

		public static void ClearCache()
		{
			WaitForSecondsByTime.Clear();
			WaitForSecondsRealtimeByTime.Clear();
		}
	
		public static WaitForSeconds WaitForSeconds(float time)
		{
			if (!WaitForSecondsByTime.TryGetValue(time, out var waitForSeconds))
				WaitForSecondsByTime.Add(time, waitForSeconds = new WaitForSeconds(time));
			return waitForSeconds;
		}
	
		public static WaitForSecondsRealtime WaitForSecondsRealtime(float time)
		{
			if (!WaitForSecondsRealtimeByTime.TryGetValue(time, out var waitForSecondsRealTime))
				WaitForSecondsRealtimeByTime.Add(time, waitForSecondsRealTime = new WaitForSecondsRealtime(time));
			return waitForSecondsRealTime;
		}
	}
}