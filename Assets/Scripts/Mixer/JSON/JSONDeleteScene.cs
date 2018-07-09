using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JSONDeleteScene {
    [SerializeField]
    string type = "method";
    [SerializeField]
    int id = 123;
    [SerializeField]
    string method;

    [SerializeField]
    DeleteSceneParams @params;

    public JSONDeleteScene(GameManager.MethodType methodType, DeleteSceneParams parameters)
    {
        method = methodType.ToString();
        @params = parameters;
    }


    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}
