// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class DeliverFood : FrameworkEvent
// {
//     // public static float DeliverRange = 1f;
//     // public DeliverFood(){
//     //     EventName = "Deliver Food";
//     //     MyItem = Items.Food;
//     //     MyRequiredRange = Range.Harvest;
//     // }

//     // public override FrameworkEvent Clone(){
//     //     DeliverFood clone = new DeliverFood();
//     //     clone.EventName = this.EventName;
//     //     clone.MyItem = this.MyItem;
//     //     clone.MyRequiredRange = this.MyRequiredRange;
//     //     return clone;
//     //     //throw new System.NotImplementedException();
//     // }

//     // public override bool CheckPreconditions(Creature agent){
//     //     if (agent.Target != null && agent.TargetDist < DeliverRange){
//     //         return true;
//     //     } else {
//     //         return false;
//     //     }
//     // }
//     // public override bool PerformEvent(Creature agent){
//     //     agent.HeldItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
//     //     Vector3 spawnPos = agent.transform.position;
//     //     spawnPos.y = 2;
//     //     agent.HeldItem.transform.position = spawnPos;
//     //     return true;
//     // }
// }
