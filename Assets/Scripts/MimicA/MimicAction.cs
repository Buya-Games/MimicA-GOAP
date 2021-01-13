using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicAction
{
    public string goal { get; protected set; }
    public Dictionary<string, bool> requirements { get; protected set; }
    public Dictionary<string, bool> effects { get; protected set; }
    public float requiredRange { get; protected set; }

    protected bool CheckPreconditions (GameWorldData data){
        foreach (string key in requirements.Keys){
            if (!data.Equals(key, requirements[key])) return false;
        }
        return true;
    }

}
