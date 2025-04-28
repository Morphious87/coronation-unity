using UnityEngine;
using UnityEngine.Events;

public class InvertTrigger : Trigger
{
	public Trigger[] triggers;

	void Start()
    {
		//foreach(var trigger in triggers)
		//	trigger.Interact(false);
    }

	public override void Interact(bool activate)
	{
		foreach(var trigger in triggers)
			trigger.Interact(!activate);
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
