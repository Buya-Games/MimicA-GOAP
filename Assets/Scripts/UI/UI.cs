using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public TMP_Text textPlayerPopulation, textTutorial;
    [SerializeField] TMP_Text textTitle, textResult;
    [SerializeField] GameObject btnStartGame;
    public Toggle ToggleTutorial;
    [SerializeField] TMP_Text textPopUpPrefab;
    Queue<TMP_Text> popUpQueue = new Queue<TMP_Text>();
    [SerializeField] TMP_Text textGUIName, textGUIGoals, textGUIActions;
    [SerializeField] GameObject canvasGUI;
    [HideInInspector] public bool GUI;
    [SerializeField] GameObject panelPause;

    void Start(){
        InitQueues();
        
    }

    void InitQueues(){
        for (int i = 0;i<5;i++){
            TMP_Text newPopUp = Instantiate(textPopUpPrefab);
            popUpQueue.Enqueue(newPopUp);
            newPopUp.gameObject.SetActive(false);
        }
    }

    public void DisplayMessage(Vector3 where, string message, string subText = ""){
        string text = message + subText;
        PopUp(where,text);

    }

    public void StartGame(){
        panelPause.SetActive(false);
        textTitle.gameObject.SetActive(false);
        textResult.gameObject.SetActive(false);
    }

    public void EndGame(bool win = true){
        panelPause.SetActive(true);
        btnStartGame.GetComponentInChildren<TMP_Text>().text = "Play Again";

        if (win){
            textResult.text = "You won!";
        } else {
            textResult.text = "You lost :(";
        }
        textResult.gameObject.SetActive(true);
    }

    public void PauseGame(bool pause = true){
        panelPause.SetActive(pause);
        btnStartGame.GetComponentInChildren<TMP_Text>().text = "Restart";
    }

    public void DisplayAction(Vector3 where, GOAPAct action, string subText = ""){
        string text = ActionToText(action,action.ActionLayer) + " " + LayerToText(action.ActionLayer) + " " + LayerToText(action.ActionLayer2) + subText;
        PopUp(where,text);
    }

    void PopUp (Vector3 where, string text){
        where.z += Random.Range(-10,10f);
        TMP_Text popUp;
        if (popUpQueue.Count > 0){
            popUp = popUpQueue.Dequeue();
        } else {
            popUp = Instantiate(textPopUpPrefab);
        }
        popUp.text = text;
        popUp.transform.position = where;
        popUp.transform.localScale = Vector3.zero;
        popUp.gameObject.SetActive(true);
        popUp.transform.DOScale(Vector3.one,0.1f).OnComplete(() => {
            //popUp.transform.DOPunchScale(Vector3.one*1.2f,0.2f,0,0);
            popUp.transform.DOMoveY(where.y+28,6).OnComplete(() => popUp.gameObject.SetActive(false));
            popUpQueue.Enqueue(popUp);
        });
    }

    string LayerToText(int layer){
        string text = "";
        if (layer == 4){
            text = "ground";
        }
        if (layer == 6){
            text = "bush";
        }
        if (layer == 7){
            text = "food";
        }
        if (layer == 8){
            text = "fungus";
        }
        if (layer == 9){
            text = "shroom";
        }
        if (layer == 10){
            text = "poop (food)";
        }
        if (layer == 11){
            text = "enemy";
        }
        if (layer == 12){
            text = "buddy";
        }
        if (layer == 13){
            text = "player";
        }
        if (layer == 14){
            text = "cow";
        }
        if (layer == 16){
            text = "poop (shroom)";
        }
        return text;
    }

    string ActionToText(GOAPAct action, int layer){
        string text = "";
        if (action is Eat){
            text = "Ate";
        }
        if (action is Follow){
            text = "Following";
        }
        if (action is MeleeAttack){
            if (layer == 11 || layer == 12 || layer == 13 || layer == 14){
                text = "Attacked";
            } else {
                text = "Harvested (" + action.ActionSkill.ToString("F2") + ")";
            }
        }
        if (action is PickupItem){
            text = "Picked Up";
        }
        if (action is ThrowItem){
            text = "Threw";
        }
        return text;
    }

    string BuddyGUIActions(CreatureLogic buddy){
        string buddyActions = "Actions: ";
        foreach (GOAPAct act in buddy.availableActions){
            buddyActions = buddyActions + " " + ActionToText(act,act.ActionLayer).ToLower() + 
                " " + LayerToText(act.ActionLayer).ToLower() + " " + LayerToText(act.ActionLayer2).ToLower() + ", ";
        }
        buddyActions = buddyActions.Substring(0,buddyActions.Length-2);//removing ", " behind the last item
        return buddyActions;
    }
    string BuddyGUIGoals(Queue<GameState.State> buddy){
        string buddyGoals = "Goals: ";
        foreach (GameState.State state in buddy){
            buddyGoals = buddyGoals + " " + state.ToString().Substring(4).ToLower() + ", ";
        }
        buddyGoals = buddyGoals.Substring(0,buddyGoals.Length-2);//removing ", " behind the last item
        return buddyGoals;
    }

    public void DisplayGUI(CreatureLogic buddy, Queue<GameState.State> buddyGoals){
        GUI = true;
        Vector3 buddyPos = buddy.transform.position;
        buddyPos.y+=5;
        canvasGUI.transform.position = buddyPos;
        textGUIName.text = buddy.name;
        textGUIGoals.text = BuddyGUIGoals(buddyGoals);
        textGUIActions.text = BuddyGUIActions(buddy);
        canvasGUI.SetActive(true);
    }

    public void ClearGUI(){
        GUI = false;
        canvasGUI.SetActive(false);
    }
    
}
