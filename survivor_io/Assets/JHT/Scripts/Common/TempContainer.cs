using System.Collections.Generic;
using UnityEngine;

namespace JHT.Scripts.Common
{
    public static class TempContainer
    {
	    public static ListObjectPool<Vector2> Vector2ListObjectPool = new();
	    public static ListObjectPool<List<Vector2>> Vector2ListListObjectPool = new();
	    public static ListObjectPool<Vector3> Vector3ListObjectPool = new();
	    public static IListObjectPool<Vector3> Vector3IListObjectPool = new();
	    public static IListObjectPool<IList<Vector3>> Vector3ListListObjectPool = new();
	    public static HashSetObjectPool<int> IntHashSetObjectPool = new();

	    public class ListObjectPool<T> 
	    {
		    private List<List<T>> _usingTempVector2List = new();
		    private Queue<List<T>> _nonUsingTempVector2List = new();
		    
		    public List<T> Get()
		    {
			    var list = 0 < _nonUsingTempVector2List.Count ? _nonUsingTempVector2List.Dequeue() : new List<T>(); 
			    _usingTempVector2List.Add(list);
			    return list;
		    }
	    
		    public void Return(List<T> list)
		    {
			    list.Clear();

			    if (false == _usingTempVector2List.Contains(list).IsFalse())
			    {
				    _usingTempVector2List.Remove(list);
			    }

			    if (false == (false == _nonUsingTempVector2List.Contains(list)).IsFalse())
			    {
				    _nonUsingTempVector2List.Enqueue(list);
			    }
		    }
	    }
	    
	    public class IListObjectPool<T> 
	    {
		    private List<IList<T>> _usingTempVector2List = new();
		    private Queue<IList<T>> _nonUsingTempVector2List = new();
		    
		    public IList<T> Get()
		    {
			    IList<T> list = 0 < _nonUsingTempVector2List.Count ? _nonUsingTempVector2List.Dequeue() : new List<T>(); 
			    _usingTempVector2List.Add(list);
			    return list;
		    }
	    
		    public void Return(IList<T> list)
		    {
			    list.Clear();
			    
			    if (false == _usingTempVector2List.Contains(list).IsFalse())
			    {
				    _usingTempVector2List.Remove(list);
			    }

			    if (false == (false == _nonUsingTempVector2List.Contains(list)).IsFalse())
			    {
				    _nonUsingTempVector2List.Enqueue(list);
			    }
		    }
	    }

	    public class HashSetObjectPool<T> 
	    {
		    private List<HashSet<T>> _usingTempVector2List = new();
		    private Queue<HashSet<T>> _nonUsingTempVector2List = new();
		    
		    public HashSet<T> Get()
		    {
			    var list = 0 < _nonUsingTempVector2List.Count ? _nonUsingTempVector2List.Dequeue() : new HashSet<T>(); 
			    _usingTempVector2List.Add(list);
			    return list;
		    }
	    
		    public void Return(HashSet<T> list)
		    {
			    list.Clear();

			    if (false == _usingTempVector2List.Contains(list).IsFalse())
			    {
				    _usingTempVector2List.Remove(list);
			    }

			    if (false == (false == _nonUsingTempVector2List.Contains(list)).IsFalse())
			    {
				    _nonUsingTempVector2List.Enqueue(list);
			    }
		    }
	    }
    }
}
