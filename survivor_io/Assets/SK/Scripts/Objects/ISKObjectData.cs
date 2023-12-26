using UnityEngine;

namespace SK
{
    public interface ISKObjectData
    {
	    public GameObject MyObject { get; }
	    public string DataObjectName { get; }
	    public uint DataID { get; }
	    public uint DataSubKey { get; }
    }
}
