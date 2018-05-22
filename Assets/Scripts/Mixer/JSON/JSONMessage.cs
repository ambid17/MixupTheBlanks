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
    string method = "createControls";

    [SerializeField]
    Parameters @params;
    public JSONMessage(string sceneID, List<string> controlIDs, List<string> texts, Position[] position)
    {
        @params = new Parameters(sceneID, controlIDs, texts, position);
    }

    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}
