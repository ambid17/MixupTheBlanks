using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player {
    public string playerName;
    public int currentScore;
    public int overallScore;
    public string answer;
    public string vote;

    public Player(string playerName)
    {
        this.playerName = playerName;
        currentScore = 0;
        overallScore = 0;
        answer = "";
        vote = "";
    }
}
