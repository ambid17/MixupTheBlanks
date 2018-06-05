using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateScenesParams : Parameters {
    [SerializeField]
    ControlsParams[] scenes;

    public CreateScenesParams(ControlsParams[] scenes)
    {
        this.scenes = scenes;
    }
}
