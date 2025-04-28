using UnityEngine;
using TMPro;

public class AntiLetter : MonoBehaviour
{
	public char letter;

	void Start()
	{
		PlayerTyping.instance.onKeyboardRefresh += Refresh;
	}

	void OnTriggerEnter(Collider other)
	{
		Collect();
	}

	void Refresh()
	{
		gameObject.SetActive(!PlayerTyping.instance.storedLetters.Contains(letter));
	}

	public void Collect()
	{
		AudioManager.Play(SoundType.Pickup);

		PlayerTyping.instance.storedLetters.Add(letter);
		PlayerTyping.instance.onKeyboardRefresh.Invoke();
	}

#if UNITY_EDITOR
	public void OnValidate()
	{
		letter = letter.ToString().ToUpper()[0];
		gameObject.name = $"Anti {letter}";
		foreach(var text in GetComponentsInChildren<TextMeshPro>())
			text.text = letter.ToString();
	}
#endif
}
