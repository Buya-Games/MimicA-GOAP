using UnityEngine;


//handles player input and passes it to Player.cs
[RequireComponent(typeof(Rigidbody))]
public class PlayerControl : MonoBehaviour
{
    GameManager manager;
    Player player;
    Vector3 keybInputDirection, lookTarget;
    Rigidbody rb;
    [HideInInspector] public float moveSpeed;
    [SerializeField] Transform head;
    [SerializeField] LayerMask throwingLM;
    [HideInInspector] public bool mouseHighlight;
    MeshRenderer highlightedObject;
    Material highlightedObjectdefaultMat;
    [SerializeField] Material highlightMat;

    // Awake is called before Start so I use it to initialize all the core stuff
    void Awake(){
        manager = FindObjectOfType<GameManager>();
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called every frame
    void Update(){
        if (manager.PlayerAlive){
            keybInputDirection = new Vector3(Input.GetAxisRaw ("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;//when move with WSAD or arrow keys
            MoveMouse();
            if (Input.GetKeyDown(KeyCode.Space)){
                player.Interact();
            }
            if (Input.GetKeyDown(KeyCode.E)){
                player.EatItem();
            }
        }
    }

    // FixedUpdate is called once per physics frame (everytime physics is re-calculated)
    void FixedUpdate(){
        rb.velocity = Vector3.zero;
        if (manager.PlayerAlive){
            if (keybInputDirection != Vector3.zero){
                rb.MovePosition(rb.position + keybInputDirection * Time.fixedDeltaTime * moveSpeed);
                Quaternion rot = Quaternion.LookRotation(keybInputDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation,rot,Time.fixedDeltaTime * 10);
            }
            if (lookTarget != Vector3.zero){
                LookAtTarget();
            }
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

    public int ThrowingTarget(Vector3 pos){ //looks at where mouse is to figure out what you're trying to throw at (cow, enemy, random spot)
        int targetLayer = 4;
        Collider[] nearbyObjects = Physics.OverlapSphere(pos,2,throwingLM);//check surroundings for stuff
        if (nearbyObjects.Length>0){
            GameObject closest = Tools.FindClosestColliderInGroup(nearbyObjects,pos);
            targetLayer = closest.layer;
        }
        return targetLayer;
    }

    public void MoveMouse(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit, 100)){
            lookTarget = hit.point;

            if (highlightedObject != null && !Tools.ContainsLayer(throwingLM,hit.transform.gameObject.layer)){
                ClearHighlight();
            }
            if (mouseHighlight){
                MouseHighlight(hit.point);
            }
        } else {
            if (highlightedObject != null){
                ClearHighlight();
            }
            lookTarget = transform.forward * 100;
        }
    }

    void MouseHighlight(Vector3 mousePos){
        Collider[] nearbyObjects = Physics.OverlapSphere(mousePos,2,throwingLM);//check surroundings for stuff
        if (nearbyObjects.Length>0){
            GameObject closest = Tools.FindClosestColliderInGroup(nearbyObjects,mousePos);
            if (closest != null){
                if (highlightedObject != null && closest != highlightedObject){
                    highlightedObject.material = highlightedObjectdefaultMat;
                }
                highlightedObject = closest.GetComponent<MeshRenderer>();
                if (highlightedObject == null){
                    highlightedObject = closest.GetComponentInChildren<MeshRenderer>();
                }
                if (highlightedObject != null){
                    highlightedObjectdefaultMat = highlightedObject.material;
                    highlightedObject.material = highlightMat;
                }
            }
        }
    }

    void ClearHighlight(){
        highlightedObject.material = highlightedObjectdefaultMat;
        highlightedObject = null;
    }

    //moves the head of the player toward the mouse position. just giving it a bit more life
    void LookAtTarget(){
        Vector3 lookDir = (lookTarget - head.transform.position);
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(lookDir.x,0,lookDir.z));
        head.rotation = Quaternion.Slerp(head.rotation,lookRot,Time.fixedDeltaTime * 10);
    }

    public void StopMovement(){
        rb.velocity = Vector3.zero;
    }
}
