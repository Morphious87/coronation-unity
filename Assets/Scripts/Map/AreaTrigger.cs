using UnityEngine;
using System;
using UnityEditor;

[RequireComponent(typeof(BoxCollider))]
public class AreaTrigger : MonoBehaviour
{
	public Trigger[] triggers;
	public bool onlyOnEnter;

	[NonSerialized] new public BoxCollider collider;
	bool previousActivationState = false;

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
    }

    void Start()
    {
		if (!onlyOnEnter)
			foreach(var trigger in triggers)
				trigger.Interact(false);
    }

    void Update()
    {
		var pos = Player.instance.transform.position; 

		var active = collider.bounds.Contains(pos);

		if (active != previousActivationState && (!onlyOnEnter || active))
			foreach(var trigger in triggers)
				trigger.Interact(active);

		previousActivationState = active;
    }

#if UNITY_EDITOR
    void OnDrawGizmos() 
	{
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
