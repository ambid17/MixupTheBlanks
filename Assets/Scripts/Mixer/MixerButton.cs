using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixerButton
{
    public Position position;
    public int scene;
    public Player player;

    public MixerButton(Player player, int x, int y, int width, int height)
    {
        this.player = player;
        position = new Position("large", width, height, x, y);
    }

    public MixerButton(Player player)
    {
        this.player = player;
        position = new Position("large", 0, 0, 0, 0);
    }
}
