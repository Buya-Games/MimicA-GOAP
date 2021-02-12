using System.Collections;
using UnityEngine;

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
    
}
