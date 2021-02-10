using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    GameManager manager;
    [SerializeField] ParticleSystem EatBerry, EatFungus, DestroyBush, DestroyMushroom, BombBoom;

    void Awake(){
        manager = FindObjectOfType<GameManager>();
    }

    public void EatingBerry(Vector3 where){
        EatBerry.transform.position = where;
        EatBerry.Play();
    }
    public void EatingFungus(Vector3 where){
        EatFungus.transform.position = where;
        EatFungus.Play();
    }
    public void DestroyingBush(Vector3 where){
        where.y = 2;
        DestroyBush.transform.position = where;
        DestroyBush.Play();
    }
    public void DestroyingMushroom(Vector3 where){
        where.y = 2;
        DestroyMushroom.transform.position = where;
        DestroyMushroom.Play();
    }

    public void BombExplosion(Vector3 where){
        BombBoom.transform.position = where;
        BombBoom.Play();
        manager.audioManager.PlaySound("bomb",0,1,Random.Range(.9f,1.1f));
    }
    
}
