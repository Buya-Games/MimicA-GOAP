using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorldData
{
    public  Dictionary<string, bool> data { get; private set; }
    public GameWorldData (){
        data = new Dictionary<string, bool>();
    }
    public void SetData (string key, bool value){
        if (data.ContainsKey(key)){
            data[key] = value;
        } else {
            data.Add(key, value);
        }
    }
    public bool Equals (string key, bool value){
        if (data.ContainsKey(key)){
            return data[key] == value;
        } else {
            return false;
        }
    }
    
}
