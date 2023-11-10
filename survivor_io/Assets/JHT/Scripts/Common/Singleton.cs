using System.Diagnostics;
using Cysharp.Text;
using Debug = UnityEngine.Debug;

namespace JHT.Scripts.Common
{
	/// <summary>
	/// 싱글톤
	/// </summary>
	public abstract class Singleton<T> where T : class, new()
	{
		private static T _instance;

		public static T Instance
		{
			get
			{
				if (null == _instance)
				{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
					Stopwatch sw = new Stopwatch();
					sw.Start();
					string typeName = typeof(T).ToString();
					Debug.Log(ZString.Format("Singleton {0} Create Start", typeName));
#endif
					_instance = new T();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
					Debug.Log(ZString.Format("Singleton {0} Create Finish({1:##,##0})", typeName, sw.ElapsedMilliseconds));
#endif
				}

				return _instance;
			}
		}
	}
}
