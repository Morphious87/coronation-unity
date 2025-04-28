using UnityEngine;
using System;
using UnityEditor;

public class TriggerCounter : Trigger
{
    public int targetCount = 1;
    public Trigger[] triggers;

    [NonSerialized] public int count = 0;
    
    bool previousActivationState = false;

    public override void Interact(bool activate)
	{
        count += activate ? 1 : -1;

        var state = count >= targetCount; 
            
        if (state != previousActivationState)
        {
            previousActivationState = state;
            foreach(var trigger in triggers)
                trigger.Interact(state);
        }

        Debug.Log(transform.gameObject.name + ": " + count);
    }

#if UNITY_EDITOR
    new void OnDrawGizmos() 
    {
        base.OnDrawGizmos();

        if (triggers == null)
            return;

        Gizmos.color = Color.green;
        foreach(var trigger in triggers)
        {
            if (trigger == null)
                continue;

            Gizmos.DrawLine(transform.position, trigger.transform.position);
        }
    }
#endif
}
