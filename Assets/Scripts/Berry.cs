using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berry : MonoBehaviour, IThrowable
{
    Rigidbody rb;
    float gravity;
    [SerializeField] float height;
    void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        gravity = Physics.gravity.y;
        //_manager = FindObjectOfType<GameManager>();
    }

    public void ThrowObject(Vector3 where, float throwStrength){
        Initialize();
        Vector3 higherPos = transform.position;
        higherPos.y = 5;
        transform.position = higherPos;
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;
        transform.parent = null;
        transform.localScale = Vector3.one * 0.5f;
        rb.velocity = CalculateTraj(where, throwStrength);
    }

    Vector3 CalculateTraj(Vector3 where, float throwStrength){
        height = (transform.position - where).sqrMagnitude/10; 
        float disY = where.y - transform.position.y;
        Vector3 disXZ = new Vector3(where.x - transform.position.x, 0, where.z - transform.position.z);
        float x = (height / disXZ.magnitude) * 2;

        Vector3 velY = Vector3.up * Mathf.Sqrt(-2 * gravity * x);
        Vector3 velXZ = disXZ / (Mathf.Sqrt(-2*x/gravity) + Mathf.Sqrt(2*(disY-x)/gravity));
        velXZ *= throwStrength;
        return velXZ + velY;
    }

    void OnCollisionEnter(Collision col){
        if (col.gameObject.layer == 9){ //if player or companion
            Creature creature = col.gameObject.GetComponent<Creature>();
            if (creature.HeldItem == null){
                creature.HeldItem = this.gameObject;
                Vector3 pos = creature.transform.position;
                pos.y += 3;
                transform.position = pos;
                transform.SetParent(creature.transform);
                transform.rotation = Quaternion.identity;
                transform.localScale = new Vector3(0.5f,0.25f,0.5f);
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }
}
