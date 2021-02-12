using UnityEngine;

//a watered down Item.cs for bushes and mushrooms. just used to manage ownership/targeting. extends same interface as item.cs
public class Resources : MonoBehaviour, ITargettable
{
    public Spawner.EnvironmentType MyType;
    GameManager manager;
    [SerializeField] public GameObject Owner { get; set; }
    public GameObject gameObj { get; set; }

    void Awake(){
        manager = FindObjectOfType<GameManager>();
        SetType();
        gameObj = gameObject;
    }

    void SetType(){
        if (gameObject.layer == 6){
            MyType = Spawner.EnvironmentType.Bush;
        }
        if (gameObject.layer == 8){
            MyType = Spawner.EnvironmentType.Mushroom;
        }
    }
    public void Targeted(GameObject who){
        Owner = who;
    }
    public void NotTargeted(){
        Owner = null;
    }
}
