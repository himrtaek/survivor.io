using System.Collections.Generic;
using Cysharp.Text;
using JHT.Scripts.Attribute;
using UnityEngine;

namespace SK
{
    public abstract class SKComponentFromDataBase : SKComponentBase
    {
	    [LabelText("ID, SubKey 중복 체크 무시")]
	    [SerializeField] private bool ignoreIDSubKeyCheck;
	    public bool IgnoreIDSubKeyCheck => ignoreIDSubKeyCheck;
	    
	    private bool _dataSubKeyInit;
	    private uint _dataSubKey;

	    public virtual uint DataSubKey
	    {
		    get
		    {
			    if (false == _dataSubKeyInit)
			    {
				    if (TryGetComponent(out SKComponentObjectChild componentObjectChild))
				    {
					    _dataSubKey = componentObjectChild.DataSubKey;
				    }
				    else
				    {
					    _dataSubKey = 0;
				    }
			    }

			    return _dataSubKey;
		    }
	    }
	    
	    private bool _dataObjectNameInit;
	    private string _dataObjectName;

	    public virtual string DataObjectName
	    {
		    get
		    {
			    if (false == _dataObjectNameInit)
			    {
				    if (TryGetComponent(out SKComponentObjectChild componentObjectChild))
				    {
					    _dataObjectName = componentObjectChild.DataObjectName;
				    }
				    else
				    {
					    _dataObjectName = SkObject.DataObjectName;
				    }
			    }

			    return _dataObjectName;
		    }
	    }

	    protected override void Awake()
	    {
		    base.Awake();
		    
		    SkObject.EventManager.AddListener(SKEventManager.SKEventType.ReadyForSpawn, OnReceiveEvent);
	    }
	    
	    private void OnReceiveEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    if (eventParam.EventType != SKEventManager.SKEventType.ReadyForSpawn)
		    {
			    Debug.LogError("Logic Error");
			    return;
		    }

		    ImportFieldFromData();
	    }

	    protected abstract void ImportFieldFromData();

	    protected abstract List<string> ExportFieldToData();

	    public string GetFieldCSVData()
	    {
		    var fieldDataList = ExportFieldToData();
		    if (null == fieldDataList)
		    {
			    return "";
		    }
		    else
		    {
			    fieldDataList.Insert(0, DataObjectName);
			    fieldDataList.Insert(0, DataSubKey.ToString());
			    fieldDataList.Insert(0, SkObject.DataID.ToString());
			    
			    var stringBuilder = ZString.CreateUtf8StringBuilder();
			    for (var index = 0; index < fieldDataList.Count; index++)
			    {
				    var fieldData = fieldDataList[index];
				    if (0 < index)
				    {
					    stringBuilder.Append("\t");
				    }
				    
				    stringBuilder.Append(fieldData);
			    }

			    return stringBuilder.ToString();
		    }
	    }
    }
}
