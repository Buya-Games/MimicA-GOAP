using UnityEngine;

//fake action I created for the fake parent node in GOAPAct. Only need this for debug
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

    protected override GameObject FindClosestObjectOfLayer(Creature agent){
        return null;
    }

    public override bool PerformEvent(Creature agent){
        return true;
        
    }
    protected override bool CompleteEvent(Creature agent){
        return true;
    }
}
