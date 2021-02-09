using UnityEngine;
public interface ITargettable
{
    GameObject gameObj { get; set; }
    [SerializeField] public GameObject Owner { get; set; }
    public void Targeted(GameObject who);
    public void NotTargeted();

}
