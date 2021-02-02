using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBounds : MonoBehaviour
{

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
        Debug.Log("caught " + col.gameObject.name);
    }
}
