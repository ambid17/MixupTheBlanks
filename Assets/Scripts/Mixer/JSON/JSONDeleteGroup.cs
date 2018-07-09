using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JSONDeleteGroup {
    [SerializeField]
    string type = "method";
    [SerializeField]
    int id = 123;
    [SerializeField]
    string method;

    [SerializeField]
    DeleteGroupsParams @params;

    public JSONDeleteGroup(GameManager.MethodType methodType, DeleteGroupsParams parameters)
    {
        method = methodType.ToString();
        @params = parameters;
    }


    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}
