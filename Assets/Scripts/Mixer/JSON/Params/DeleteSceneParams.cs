using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteSceneParams : Parameters {
    [SerializeField]
    string sceneID;
    [SerializeField]
    string reassignSceneID;

    public DeleteSceneParams(string sceneID, string reassignSceneID)
    {
        this.sceneID = sceneID;
        this.reassignSceneID = reassignSceneID;
    }
}
