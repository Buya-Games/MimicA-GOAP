using UnityEngine;
public interface IHittable
{
    public void TakeHit(GameObject attacker, float damage);
}
