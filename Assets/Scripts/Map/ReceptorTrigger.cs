using UnityEngine;
using System.Linq;
using TMPro;

public class ReceptorTrigger : MonoBehaviour
{
	public Panel[] receptors;
	public TextMeshPro[] indicators;

	public string[] solutions;
	public Trigger[] triggers;

	string previousInput;
	Trigger previousTrigger;

	void Awake()
	{
		PlayerTyping.instance.onKeyboardRefresh += Refresh; 
	}

	void Start()
	{
		Refresh();	
	}

	void Refresh()
	{
		var input = string.Join(string.Empty, receptors.Select(x => x.input));

		for (var i = 0; i < indicators.Length; i++)
			indicators[i].gameObject.SetActive(receptors[i].input != string.Empty);

		if (input == previousInput)
			return;

		previousInput = input;

		for (var i = 0; i < solutions.Length; i++)
		{
			if (input != solutions[i])
				continue;

			Solve(i);
			return;
		}

		if (previousTrigger != null)
		{
			previousTrigger.Interact(false);
			previousTrigger = null;
			AudioManager.Play(SoundType.LowUnsuccess);
			foreach(var indicator in indicators)
				indicator.color = Color.black;
		}
	}

	void Solve(int solutionIndex)
	{
		var trigger = triggers[solutionIndex];
		trigger.Interact(true);

		if (previousTrigger != null)
			previousTrigger.Interact(false);

		previousTrigger = trigger;

		AudioManager.Play(SoundType.LowSuccess);
		foreach(var indicator in indicators)
			indicator.color = Color.green;
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

		foreach(var receptor in receptors)
		{
			if (receptor == null)
				continue;

			Gizmos.DrawLine(transform.position, receptor.transform.position);
		}
	}
#endif
}
