using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Parameters
{
    [SerializeField]
    string sceneID;
    [SerializeField]
    Control[] controls;

    public Parameters(string sceneID, List<string> controlIDs, List<string> texts, Position[] position)
    {
        this.sceneID = sceneID;

        controls = new Control[controlIDs.Count];
        for (int i = 0; i < controlIDs.Count; i++)
        {
            controls[i] = new Control(controlIDs[i], texts[i], position[i]);
        }
    }
}