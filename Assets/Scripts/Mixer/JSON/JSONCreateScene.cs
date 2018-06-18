using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JSONCreateScene : MonoBehaviour {
    [SerializeField]
    string type = "method";
    [SerializeField]
    int id = 123;
    [SerializeField]
    string method;

    [SerializeField]
    CreateScenesParams @params;

    public JSONCreateScene(GameManager.MethodType methodType, CreateScenesParams parameters)
    {
        method = methodType.ToString();
        @params = parameters;
    }


    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}
