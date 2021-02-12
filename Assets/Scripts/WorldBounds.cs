using UnityEngine;

//sometimes shit goes flying like crazy, so when it does I have these giant colliders surrounding the play area
//when collision detected, it will despawn the object (items, buddies, etc)
public class WorldBounds : MonoBehaviour
{
    GameManager manager;

    void Awake(){
        manager = FindObjectOfType<GameManager>();
    }

    
    void OnTriggerEnter(Collider col){
        Rigidbody rb = col.GetComponent<Rigidbody>();
        if (rb!=null){
            rb.velocity = Vector3.zero;
        }
        manager.spawner.Despawn(col.gameObject);
    }
}
