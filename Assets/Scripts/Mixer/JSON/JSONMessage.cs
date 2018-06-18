using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JSONMessage {
    [SerializeField]
    string type = "method";
    [SerializeField]
    int id = 123;
    [SerializeField]
    string method;

    [SerializeField]
    Parameters @params;
	
    public JSONMessage(MethodType methodType, Parameters parameters){
        method = methodType.ToString();
        @params = parameters;
    }
	

	public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }

    public enum MethodType
    {
        createControls, deleteControls, createScenes, deleteScene, onSceneDelete
    }
}
