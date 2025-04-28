using UnityEngine;

public class Keyboard : MonoBehaviour
{
	static Keyboard _instance; 
	public static Keyboard instance => _instance ?? (_instance = FindAnyObjectByType<Keyboard>());

	public Key[] keys;
	public GameObject minusKey;
	public GameObject background;
	public GameObject backgroundMinus;

	public void UnlockMinus(bool unlock = true)
	{
		background.SetActive(!unlock);
		backgroundMinus.SetActive(unlock);
		minusKey.SetActive(unlock);
	}

	public void Show(bool show)
	{
		gameObject.SetActive(show);
	}

	void Awake()
	{
		PlayerTyping.instance.onKeyboardRefresh += Refresh;   
	}

    void Start()
    {
        Refresh();
    }

    public void Refresh()
	{
		var typing = PlayerTyping.instance;

		for (var i = 0; i < keys.Length; i++)
		{
			var key = keys[i];
			var c = PlayerTyping.LETTERS[i];


			var available = typing.unlockedLetters.Contains(c) && !typing.storedLetters.Contains(c);
			key.box.gameObject.SetActive(available);

			if (!key.box.gameObject.activeSelf)
				continue;

			var typed = typing.typedLetters.Contains(c) && !typing.infiniteLetters.Contains(c);
			key.text.color = typed ? Color.gray : Color.white;

			var infinite = typing.infiniteLetters.Contains(c);
			key.box.sprite = infinite ? key.spriteCyan : key.spriteNormal;
		}

		UnlockMinus(typing.unlockedLetters.Contains('-'));
	}
}
