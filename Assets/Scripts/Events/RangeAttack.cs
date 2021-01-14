// public class RangeAttack : FrameworkEvent
// {
//     public static float AttackRange = 10f;
//     public float RangeDamage = 1f;
//     public float RangeRadius = 5f;
//     public RangeAttack(){
//         EventName = "Range Attack";
//         MyItem = Items.Bomb;
//         MyRequiredRange = Range.Bomb;
//     }

//     public override FrameworkEvent Clone(){
//         RangeAttack clone = new RangeAttack();
//         clone.EventName = this.EventName;
//         clone.MyItem = this.MyItem;
//         clone.MyRequiredRange = this.MyRequiredRange;
//         clone.RangeDamage = this.RangeDamage;
//         clone.RangeRadius = this.RangeRadius;
//         return clone;
//     }
//     public override bool CheckPreconditions(Creature agent){
//         if (agent.Target != null && agent.TargetDist < AttackRange){
//             return true;
//         } else {
//             return false;
//         }
//     }

//     public override bool PerformEvent(Creature agent){
//         return true;
//     }
// }
