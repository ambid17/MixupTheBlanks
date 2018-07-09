using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CreateGroupsParams{
    [SerializeField]
    Group[] groups;

    public CreateGroupsParams(Group[] groups)
    {
        this.groups = groups;
    }
}
