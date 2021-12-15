using Realms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : RealmObject
{
    [Required]
    [PrimaryKey]
    public string Id { get; set; }
    public int Gold { get; set; }
    public int Hearts { get; set; }

    public PlayerData() { }
    public PlayerData(string Id)
    {
        this.Id = Id;
        Gold = 0;
        Hearts = 0;
    }
    public PlayerData(string Id, string data)
    {
        this.Id = Id;
        var fields = data.Split(',');

        this.Gold = int.Parse(fields[0]);
        this.Hearts = int.Parse(fields[1]);
    }
    public override string ToString() => $"{Gold},{Hearts}";


    public static readonly string DefaultId = "default_id";

}