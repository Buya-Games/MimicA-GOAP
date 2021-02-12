using UnityEngine;
public interface ITargettable
{
    GameObject gameObj { get; set; }
    public void Targeted(GameObject who);
    public void NotTargeted();

    public GameObject GetOwner();

}
