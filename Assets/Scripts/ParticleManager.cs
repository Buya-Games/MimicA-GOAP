using UnityEngine;
public class ParticleManager : MonoBehaviour
{
    GameManager manager;
    [SerializeField] ParticleSystem EatBerry, EatFungus, DestroyBush, DestroyMushroom, BombBoom, DestroyBuddy, DestroyEnemy, DestroyCow;

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

    public void DestroyingEnemy(Vector3 where){
        where.y = 2;
        DestroyEnemy.transform.position = where;
        DestroyEnemy.Play();
    }

    public void DestroyingBuddy(Vector3 where){
        where.y = 2;
        DestroyBuddy.transform.position = where;
        DestroyBuddy.Play();
    }

    public void DestroyingCow(Vector3 where){
        where.y = 2;
        DestroyCow.transform.position = where;
        DestroyCow.Play();
    }

    public void BombExplosion(Vector3 where){
        BombBoom.transform.position = where;
        BombBoom.Play();
        manager.audioManager.PlaySound("bomb",0,1,Random.Range(.9f,1.1f));
    }
    
}
