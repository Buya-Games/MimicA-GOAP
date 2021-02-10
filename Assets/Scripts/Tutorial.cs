using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    GameManager manager;
    [SerializeField] string[] tutText;
    [HideInInspector] public bool Tut0WASD, Tut1PickupBerry, Tut2EatBerry, Tut3HardMelee, Tut4ThrowBerryCow, 
        Tut5ThrowPoop, Tut6FeedShroomCow, Tut7GiveBirth, Tut8TeachAny, Tut9TeachFeedPlayer;
    void Awake(){
        manager = GetComponent<GameManager>();
        Tut0WASD = true;
    }

    public void DisplayNextTip(int tipNo){
        manager.ui.textTutorial.gameObject.SetActive(true);
        manager.ui.textTutorial.text = tutText[tipNo];
    }

    public void TryAgain(){
        manager.ui.textTutorial.gameObject.SetActive(true);
        manager.ui.textTutorial.text = "Try again.";
        manager.spawner.SpawnEnvironment(manager.spawner.EmptyNearbyLocation(manager.cow.transform.position,5,10),Spawner.EnvironmentType.Bush);
    }

    public void EndTutorial(){
        manager.Tutorial = false;
        Tut0WASD = true;
        manager.StartGame();
        StartCoroutine(CloseTutorialText());
    }

    public void SpawnBush(){
        manager.spawner.SpawnEnvironment(manager.spawner.EmptyNearbyLocation(manager.cow.transform.position,5,10),Spawner.EnvironmentType.Bush);
    }

    public void SpawnShroom(){
        manager.spawner.SpawnEnvironment(manager.spawner.EmptyNearbyLocation(manager.cow.transform.position,5,10),Spawner.EnvironmentType.Mushroom);
    }

    IEnumerator CloseTutorialText(){
        yield return new WaitForSeconds(10);
        manager.ui.textTutorial.gameObject.SetActive(false);
        
    }

}
