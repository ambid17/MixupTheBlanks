using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Group{
    [SerializeField]
    string groupID;
    [SerializeField]
    string sceneID;

    public Group(string groupID, string sceneID)
    {
        this.groupID = groupID;
        this.sceneID = sceneID;
    }
}
