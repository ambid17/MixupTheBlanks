using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MixerScene
{
    [SerializeField]
    public string sceneID;
    [SerializeField]
    public List<MixerButton> buttons;

    [SerializeField]
    public MixerScene previousScene;
    [SerializeField]
    public MixerScene nextScene;
    [SerializeField]
    public Player player;

    public MixerScene(string sceneID, Player player)
    {
        this.sceneID = sceneID;
        this.player = player;
        buttons = new List<MixerButton>();
    }
}
