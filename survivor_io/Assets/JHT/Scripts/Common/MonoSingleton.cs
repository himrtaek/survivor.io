using Cysharp.Text;
using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
/// 
namespace JHT.Scripts.Common
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static object _syncobj = new ();
        protected static bool appIsClosing;

        public static void Destroy()
        {
	        if (_instance)
	        {
		        Destroy(_instance);   
	        }
	        
	        _instance = null;
        }

        public static T Instance
        {
            get
            {
	            if (!AppMain.Instance.IsMainThread())
	            {
		            UnityMainThreadDispatcher.Instance().Enqueue(() =>
		            {
			            Debug.LogError($"{typeof(T).Name}.Instance => 매인 쓰래드가 아닙니다");
		            });

		            return null;
	            }
	            
#if UNITY_EDITOR
                if (Application.isPlaying && appIsClosing)
                    return null;
#endif
	            
                try
                {
	                lock (_syncobj)
	                {
		                if (_instance.IsNull(false))
		                {
			                T[] objs = FindObjectsOfType<T>();

			                if (objs.Length > 0)
				                _instance = objs[0];

			                if (objs.Length > 1)
				                Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");

			                if (_instance.IsNull(false))
			                {
				                string goName = typeof(T).ToString();
				                GameObject go = GameObject.Find(goName);
				                if (go.IsNull(false))
				                {
#if UNITY_EDITOR
					                Debug.Log(ZString.Format("MonoSingleton {0} Create", goName));
#endif
					                go = new GameObject(goName);
#if UNITY_EDITOR
				                }
				                else
				                {
					                Debug.Log(ZString.Format("MonoSingleton {0} Find", goName));
#endif
				                }

				                if (false == go.TryGetComponent(out _instance))
				                {
					                _instance = go.AddComponent<T>();
				                }
			                }
		                }

		                return _instance;
	                }
                }
                catch (System.Exception e)
                {
	                Debug.LogError(ZString.Format("Singleton<{0}>.Instance : create error : {1}", typeof(T).Name,
		                e.Message));
	                throw;
                }
            }
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed,
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            // release reference on exit
            appIsClosing = true;
        }
    }
}
