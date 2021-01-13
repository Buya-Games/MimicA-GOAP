using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Creature
{
    public Enemy FindNearestEnemy(){
        Enemy[] enemies = manager.AliveEnemies.ToArray();
        Enemy closestEnemy = null;
        float enemyDist = Mathf.Infinity;
        foreach (Enemy e in enemies)
        {
            if (closestEnemy == null){
                closestEnemy = e;
                enemyDist = Mathf.Abs(Vector3.Distance(closestEnemy.transform.position, this.transform.position));
            } else {
            // Check if this enemy is closer
                float dist = Mathf.Abs(Vector3.Distance(closestEnemy.transform.position, this.transform.position));
                if (dist < enemyDist){
                    closestEnemy = e;
                    enemyDist = dist;
                }
            }
        }
        // if any enemies were found, return closest one, or return nothing;
        return closestEnemy;
    }

    
}
