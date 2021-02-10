using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public static class GhettoAnimations
{
    public static IEnumerator AnimSwing(Transform melee){
        float rot = -120;
        Vector3 meleeRot = new Vector3(0,rot,0);
        melee.gameObject.SetActive(true);
        while (rot <120){
            rot+=10;
            meleeRot = new Vector3(0,rot,0);
            melee.localRotation = Quaternion.Euler(meleeRot);
            yield return null;
        }
        melee.gameObject.SetActive(false);
    }

    public static IEnumerator FallOver(Transform deadThing){
        int posNeg = Random.Range(0,2)*2-1;//using this so it falls left or right
        float fallAngle = 0;
        Vector3 rot = deadThing.rotation.eulerAngles;
        while (fallAngle > 90 * posNeg){
            fallAngle-=2;
            rot.z = fallAngle;
            deadThing.transform.rotation = Quaternion.Euler(rot);
            yield return null;
        }
        yield return new WaitForSeconds(5);
        deadThing.transform.DOScale(Vector3.zero,0.2f).OnComplete(() => {Wipe(deadThing);});//does this do anything??
    }

    static void Wipe(Transform deadThing){
        GameObject.Destroy(deadThing.gameObject);
    }

    // public static IEnumerator AnimHitRB(Vector3 dir, Rigidbody rb){
    //     float hitForce = 15;
    //     while (hitForce > 0){
    //         rb.velocity = dir * hitForce;
    //         hitForce -= Time.deltaTime;
    //         yield return null;
    //     } 
    // }

    // public static IEnumerator AnimHit(Transform hitObject, Vector3 startPos, Vector3 hitPos){
    //     hitPos*=2;
    //     hitPos.y = 0;
    //     // Vector3 startPos = transform.position;
    //     // Vector3 hitPos = _attackTarget.position;
    //     float attackSpeed = 3;
    //     float percent = 0;

    //     while (percent <= 1){
    //         percent += Time.deltaTime * attackSpeed;
    //         float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
    //         hitObject.position = Vector3.Lerp(startPos,hitPos,interpolation);

    //         yield return null;
    //     }
    // }
    
}
