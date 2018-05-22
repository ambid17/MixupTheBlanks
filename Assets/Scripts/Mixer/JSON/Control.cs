using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Control
{
    [SerializeField]
    string kind = "button";
    [SerializeField]
    string controlID;
    [SerializeField]
    Position[] position;
    [SerializeField]
    string text;
    [SerializeField]
    int cost = 10;

    public Control(string controlID, string text, Position position)
    {
        this.controlID = controlID;
        this.text = text;
        this.position = new Position[] {
            new Position("small", position.width, position.height, position.x, position.y)
            , new Position("medium", position.width, position.height, position.x, position.y)
            , position };
    }
}
