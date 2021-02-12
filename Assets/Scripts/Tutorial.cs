using System.Collections;
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

    public void ResetTutorial(){
        Tut0WASD = true;
        Tut1PickupBerry = false; 
        Tut2EatBerry = false; 
        Tut3HardMelee = false; 
        Tut4ThrowBerryCow = false; 
        Tut5ThrowPoop = false; 
        Tut6FeedShroomCow = false; 
        Tut7GiveBirth = false; 
        Tut8TeachAny = false; 
        Tut9TeachFeedPlayer = false;
    }

    public void EndTutorial(){
        manager.Tutorial = false;
        Tut0WASD = true;
        manager.EndTutorial();
        StartCoroutine(CloseTutorialText());
    }

    public void SpawnBush(){
        manager.spawner.SpawnEnvironment(manager.spawner.EmptyNearbyLocation(manager.cow.transform.position,5,10),Spawner.EnvironmentType.Bush);
    }

    public void SpawnShroom(){
        manager.spawner.SpawnEnvironment(manager.spawner.EmptyNearbyLocation(manager.cow.transform.position,5,10),Spawner.EnvironmentType.Mushroom);
    }

    public IEnumerator CloseTutorialText(){
        yield return new WaitForSeconds(10);
        manager.ui.textTutorial.gameObject.SetActive(false);
        
    }

}
