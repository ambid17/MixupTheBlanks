using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MixerInput: MixerControl
{
    public MixerInput(string scene, int x, int y, int width, int height) : base()
    {
        this.scene = scene;
        position = new Position("large", width, height, x, y);
    }
}
