using UnityEngine;

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
