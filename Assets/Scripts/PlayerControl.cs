using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControl : MonoBehaviour
{
    Player player;
    Vector3 keybInputDirection, lookTarget;
    Rigidbody rb;
    [HideInInspector] public float moveSpeed;
    [SerializeField] Transform head;

    // Awake is called before Start so I use it to initialize all the core stuff
    void Awake(){
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called every frame
    void Update(){
        keybInputDirection = new Vector3(Input.GetAxisRaw ("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;//when move with WSAD or arrow keys
        MoveMouse();
        if (Input.GetKeyDown(KeyCode.Space)){
            player.Interact();
        }
    }

    // FixedUpdate is called once per physics frame (everytime physics is re-calculated)
    void FixedUpdate(){
        rb.velocity = Vector3.zero;
        if (keybInputDirection != Vector3.zero){
            rb.MovePosition(rb.position + keybInputDirection * Time.fixedDeltaTime * moveSpeed);
            Quaternion rot = Quaternion.LookRotation(keybInputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation,rot,Time.fixedDeltaTime * 10);
        }
        if (lookTarget != Vector3.zero){
            LookAtTarget();
        }
    }

    public Vector3 MousePos(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast (ray, out hit, 100)){
            return hit.point;
        } else {
            return Vector3.zero;
        }
    }

    public void MoveMouse(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit, 100)){
            lookTarget = hit.point;
        } else {
            lookTarget = transform.forward * 100;
        }
    }

    void LookAtTarget(){
        Vector3 lookDir = (lookTarget - head.transform.position);
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(lookDir.x,0,lookDir.z));
        head.rotation = Quaternion.Slerp(head.rotation,lookRot,Time.fixedDeltaTime * 10);
    }
}
