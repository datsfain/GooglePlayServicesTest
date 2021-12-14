using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public int Gold { get; set; }
    public int Hearts { get; set; }

    public PlayerData(string data)
    {
        var parts = data.Split(',');
        Gold = int.Parse(parts[0]);
        Hearts = int.Parse(parts[1]);
    }
    public override string ToString() => $"{Gold},{Hearts}";
}