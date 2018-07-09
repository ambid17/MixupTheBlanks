using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DeleteControlsParams : Parameters {
    [SerializeField]
    string sceneID;
    [SerializeField]
    string[] controlIDs;

    public DeleteControlsParams(string sceneID, string[] controlIDs)
    {
        this.sceneID = sceneID;
        this.controlIDs = controlIDs;
    }
}
