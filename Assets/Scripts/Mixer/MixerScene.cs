using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixerScene
{
    public List<MixerButton> buttons;
    public int id;
    public bool isOn;

    public MixerScene(int id)
    {
        this.id = id;
        buttons = new List<MixerButton>();
        isOn = id == 0 ? true : false;
    }
}
