using UnityEngine;
public interface IThrowable
{
    public void ThrowObject(Vector3 mousePos, float throwStrength, bool feeding = false);
    public GameObject ThisGameObject();
}
