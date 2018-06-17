using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ControlsParams : Parameters {
    [SerializeField]
    string sceneID;
    [SerializeField]
    Control[] controls;

    public ControlsParams(string sceneID, string kind, List<string> controlIDs, List<string> texts, Position[] position) : base()
    {
        this.sceneID = sceneID;

        controls = new Control[controlIDs.Count];
        for (int i = 0; i < controlIDs.Count; i++)
        {
            controls[i] = new Control(kind, controlIDs[i], texts[i], position[i]);
        }
    }
}
