using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JSONCreateGroups {
    [SerializeField]
    string type = "method";
    [SerializeField]
    int id = 123;
    [SerializeField]
    string method;

    [SerializeField]
    CreateGroupsParams @params;

    public JSONCreateGroups(GameManager.MethodType methodType, CreateGroupsParams parameters)
    {
        method = methodType.ToString();
        @params = parameters;
    }


    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}
