using UnityEngine;
using System.Collections.Generic;
public class Fake: GOAPAct
{
    public Fake(){
        
    }

    public override GOAPAct Clone(){
        Fake clone = new Fake();
        return clone;
    }

    public override bool GetTarget(Creature agent){
        return true;
    }

    public override bool CheckRange(Creature agent){
        return true;
    }

    protected override GameObject FindClosestObjectOfLayer(GameObject agent){
        return null;
    }

    public override bool PerformEvent(Creature agent){
        return true;
        
    }
    protected override bool CompleteEvent(Creature agent){
        return true;
    }
}
