using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MixerSceneHolder{
    Dictionary<string, List<MixerScene>> sceneHolder;
    List<MixerScene> sceneList;

    public MixerSceneHolder()
    {
        sceneHolder = new Dictionary<string, List<MixerScene>>();
        sceneList = new List<MixerScene>();
    }

    public void AddSceneList(string forPlayer, List<MixerScene> scenes)
    {
        sceneHolder.Add(forPlayer, scenes);
        sceneList.AddRange(scenes);
    }

    public List<MixerScene> GetScenesForPlayer(string player)
    {
        List<MixerScene> scenes = new List<MixerScene>();
        sceneHolder.TryGetValue(player, out scenes);
        return scenes;
    }

    public List<MixerScene> GetSceneList()
    {
        return sceneList;
    }
}
