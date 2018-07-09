using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DeleteGroupsParams{
    [SerializeField]
    string groupID;
    [SerializeField]
    string reassignGroupID;

    public DeleteGroupsParams(string groupID, string reassignGroupID)
    {
        this.groupID = groupID;
        this.reassignGroupID = reassignGroupID;
    }
}
