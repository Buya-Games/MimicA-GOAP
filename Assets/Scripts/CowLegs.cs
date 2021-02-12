using System.Collections;
using UnityEngine;

//moving cow's legs via procedural and IK
public class CowLegs : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float maxDist, ignoreOppositeLegDist;
    [SerializeField] CowLegs oppositeLeg;
    [SerializeField] float moveDuration;
    [SerializeField] AnimationCurve legHeightCurve;
    public bool grounded = true;
    [SerializeField] float overshoot;

    void Update(){
        float legDist = Tools.GetDist(transform.gameObject,target.gameObject);
        if (legDist > maxDist && grounded && (oppositeLeg.grounded || legDist > ignoreOppositeLegDist)){
            grounded = false;
            StartCoroutine(MoveToTarget());
        }
    }

    IEnumerator MoveToTarget(){
        Vector3 startPos = transform.position;

        Vector3 moveDir = (target.position-transform.position).normalized;
        Vector3 targetPos = target.position + (moveDir * overshoot);

        float timeElapsed = 0;
        float legHeight = 0;
        while (timeElapsed<moveDuration){
            timeElapsed+=Time.deltaTime;
            float normalizedTime = timeElapsed/moveDuration;
            legHeight = legHeightCurve.Evaluate(normalizedTime);
            transform.position = Vector3.Lerp(startPos,new Vector3(targetPos.x,legHeight,targetPos.z),normalizedTime);
            yield return null;
        }
        grounded = true;
    }
}
