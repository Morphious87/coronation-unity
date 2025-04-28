using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PanelsRemaining : MonoBehaviour
{
	public TextMeshPro text;
	public Trigger[] triggers;

	bool previousActivationState = false;

	public void Awake()
	{
		PlayerTyping.instance.onKeyboardRefresh += Refresh;
	}

	void Refresh()
	{
		int count = 0;
		foreach (var panel in FindObjectsByType<Panel>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			if (panel.receptor)
				continue;

			if (!panel.singleSolution)
				continue;

			if (panel.completed)
				continue;

			count++;
		}

		text.text = count.ToString();

		var completed = count == 0;

		if (completed == previousActivationState)
			return;

		previousActivationState = completed;

		foreach(var trigger in triggers)
		{
			trigger?.Interact(completed);
		}
	}
}
