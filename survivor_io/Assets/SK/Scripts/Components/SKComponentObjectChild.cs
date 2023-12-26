using UnityEngine;
using UnityEditor;

namespace SK
{
    public class SKComponentObjectChild : SKComponentBase, ISKObjectData
    {
	    public GameObject MyObject => gameObject;
	    
	    public uint DataID => SkObject.DataID;
	    
	    [SerializeField] private uint dataSubKey;
	    [SerializeField] private string dataObjectName;

	    public uint DataSubKey => dataSubKey;
	    public string DataObjectName => dataObjectName;
	    
	    #if UNITY_EDITOR
	    public void ChangeSerializeDataObjectName(string newDataObjectName)
	    {
		    dataObjectName = newDataObjectName;
			EditorUtility.SetDirty(gameObject);
	    }
	    #endif
    }
}
