using UnityEngine;

//a tiny little class for closing GUI if you click on the ground
public class Ground : MonoBehaviour
{
    GameManager manager;
    void Awake()
    {
        manager = FindObjectOfType<GameManager>();
        
    }
    void OnMouseDown(){
        manager.ui.ClearGUI();
    }
}
