using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Position
{
    [SerializeField]
    public string size;
    public int width;
    [SerializeField]
    public int height;
    [SerializeField]
    public int x;
    [SerializeField]
    public int y;

    public Position(string size, int width, int height, int x, int y)
    {
        this.size = size;
        this.width = width;
        this.height = height;
        this.x = x;
        this.y = y;
    }
}
