using UnityEngine;

//this is a stupid class I made just for fun. Cow will look at any stuff thrown near its head
public class CowHead : MonoBehaviour
{
    [SerializeField] Transform body;
    bool colliding = false;
    Transform lookAtTarget;
    [SerializeField] LayerMask cowHeadLM;
    
    void Update(){
        if (colliding){
            Vector3 lookDir = (lookAtTarget.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(lookDir),Time.fixedDeltaTime * 10);
        } else {
            transform.rotation = Quaternion.Slerp(transform.rotation,body.rotation,Time.fixedDeltaTime * 10);
        }
    }

    void FixedUpdate(){
        LookForNearbyObjects();
    }

    void LookForNearbyObjects(){
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position,7,cowHeadLM);//check surroundings for stuff
        if (nearbyObjects.Length > 0){
            colliding = true;
            lookAtTarget = nearbyObjects[0].transform;//it only looks for the first item in the array, which can be rather random 
        } else {
            colliding = false;
        }
    }
}
