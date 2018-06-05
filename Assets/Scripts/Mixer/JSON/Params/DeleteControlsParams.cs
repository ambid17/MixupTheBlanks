using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteControlsParams : Parameters {
    string sceneID;
    string[] controlIDs;

    public DeleteControlsParams(string sceneID, string[] controlIDs)
    {
        this.sceneID = sceneID;
        this.controlIDs = controlIDs;
    }
}
