using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JSONDeleteControls {
    [SerializeField]
    string type = "method";
    [SerializeField]
    int id = 123;
    [SerializeField]
    string method;

    [SerializeField]
    DeleteControlsParams @params;

    public JSONDeleteControls(GameManager.MethodType methodType, DeleteControlsParams parameters)
    {
        method = methodType.ToString();
        @params = parameters;
    }


    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}
