using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Player {
    [SerializeField]
    public string playerName;
    [SerializeField]
    public int currentScore;
    [SerializeField]
    public int overallScore;
    [SerializeField]
    public string answer;
    [SerializeField]
    public string vote;
    [SerializeField]
    public string currentScene;

    public Player(string playerName)
    {
        this.playerName = playerName;
        currentScore = 0;
        overallScore = 0;
        answer = "";
        vote = "";
        currentScene = "";
    }
}
