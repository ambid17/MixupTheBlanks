using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MixerButton : MixerControl
{
    [SerializeField]
    public Player player;

    public MixerButton(Player player) : base()
    {
        this.player = player;
    }
}
