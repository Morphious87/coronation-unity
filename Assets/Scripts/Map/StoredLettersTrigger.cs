using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class StoredLettersTrigger : MonoBehaviour
{
	public Trigger[] triggers;
	public string requiredStoredLetters;

	HashSet<char> requiredStoredLettersHashSet;
	bool previousActivationState = false;

	void Awake()
	{
		PlayerTyping.instance.onKeyboardRefresh += Refresh;
		requiredStoredLettersHashSet = requiredStoredLetters.ToHashSet();
	}

	void Start()
	{
		Refresh();	
	}

	void Refresh()
	{
		var completed = PlayerTyping.instance.storedLetters.SetEquals(requiredStoredLettersHashSet);

		if (completed == previousActivationState)
			return;

		previousActivationState = completed;

		foreach(var trigger in triggers)
		{
			trigger?.Interact(completed);
		}
	}

#if UNITY_EDITOR
    void OnDrawGizmos() 
	{
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
