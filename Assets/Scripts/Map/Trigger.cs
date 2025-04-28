using UnityEngine;
using UnityEditor;

public abstract class Trigger : MonoBehaviour
{
    public abstract void Interact(bool activate);

#if UNITY_EDITOR
    protected void OnDrawGizmos() 
    {
        Handles.Label(transform.position, gameObject.name);
    }
#endif
}
