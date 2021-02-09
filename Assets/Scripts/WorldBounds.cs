using UnityEngine;

public class WorldBounds : MonoBehaviour
{

    //sometimes shit goes flying like crazy, so when it does I have these giant colliders surrounding the play area
    //when collision detected, it will move the object back in the play area;
    void OnTriggerEnter(Collider col){
        Rigidbody rb = col.GetComponent<Rigidbody>();
        if (rb!=null){
            rb.velocity = Vector3.zero;
        }
        Vector3 where = col.transform.position;
        where.x = Mathf.Clamp(where.x,-100,100);
        where.y = 1;
        where.z = Mathf.Clamp(where.z,-100,100);
        col.transform.position = where;
    }
}
