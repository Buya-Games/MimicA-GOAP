using UnityEngine;
public interface IThrowable
{
    public void ThrowObject(Vector3 mousePos, float throwStrength);
    public GameObject ThisGameObject();
}
