using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Control
{
    [SerializeField]
    string kind;
    [SerializeField]
    string controlID;
    [SerializeField]
    Position[] position;
    [SerializeField]
    string text;
    [SerializeField]
    int cost = 10;

    public Control(string kind, string controlID, string text, Position position)
    {
        this.kind = kind;
        this.controlID = controlID;
        this.text = text;
        this.position = new Position[] { position };
    }
}
