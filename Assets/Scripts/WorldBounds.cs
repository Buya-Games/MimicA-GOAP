using UnityEngine;

public class WorldBounds : MonoBehaviour
{
    Transform cow;

    void Awake(){
        cow = FindObjectOfType<Cow>().transform;
    }

    //sometimes shit goes flying like crazy, so when it does I have these giant colliders surrounding the play area
    //when collision detected, it will move the object back in the play area;
    void OnTriggerEnter(Collider col){
        Rigidbody rb = col.GetComponent<Rigidbody>();
        if (rb!=null){
            rb.velocity = Vector3.zero;
        }
        Vector3 where = col.transform.position;
        where.x = cow.position.x + Mathf.Clamp(where.x,-80,80);
        where.y = 1;
        where.z = cow.position.z + Mathf.Clamp(where.z,-80,80);
        col.transform.position = where;
    }
}
