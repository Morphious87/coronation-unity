using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : Trigger
{
    public UnityEvent events;

	public override void Interact(bool activate)
	{
		events?.Invoke();
	}
}
