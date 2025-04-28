using UnityEngine;
using System.Linq;
using System;
using DG.Tweening;
using System.Collections.Generic;

public class PlayerTyping : MonoBehaviour
{
	static PlayerTyping _instance; 
	public static PlayerTyping instance => _instance ?? (_instance = FindAnyObjectByType<PlayerTyping>());

	public float range;

	Ray ray;
	RaycastHit hit;
	Collider lastHit;
	[NonSerialized]	public Panel lastHoveredPanel;

	public bool typing;

	Panel activePanel;

	Keyboard keyboard => Keyboard.instance;

	Tween unpauseTween;

	public const string LETTERS = "-QWERTYUIOPASDFGHJKLZXCVBNM";
	readonly KeyCode[] keycodes = LETTERS.Select(x => x == '-' ? KeyCode.Minus : Enum.Parse<KeyCode>(x.ToString())).ToArray();

	public HashSet<char> unlockedLetters { get; private set; }
	public HashSet<char> infiniteLetters { get; private set; }

	public HashSet<char> storedLetters { get; private set; }
	public HashSet<char> typedLetters { get; private set; }

	public Action onKeyboardRefresh = () => {};

	void Awake()
	{        
		unlockedLetters = new(LETTERS.Substring(1));
		infiniteLetters = new(unlockedLetters);
		storedLetters = new();
		typedLetters = new();
	}

	void Start()
	{
		SetTyping(false);

		DOVirtual.DelayedCall(0f, () => onKeyboardRefresh.Invoke());
	}

	void Update()
	{
		if (PauseMenu.paused)
			return;

		if (unpauseTween?.active == true)
			return;

		DoInput();
		DoRaycast();

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Return))
			SetTyping(!typing);
	}

	public void SetTyping(bool typing)
	{
		this.typing = typing;
		keyboard.Show(typing);

		if (typing)
		{
			Player.instance.lockInput = true;
			if (lastHoveredPanel != null)
				typedLetters = lastHoveredPanel.input.ToHashSet();
		}
		else
		{
			var panelToClear = activePanel;
			unpauseTween = DOVirtual.DelayedCall(.25f, () => 
			{
				Player.instance.lockInput = false;
				if (panelToClear != null && !panelToClear.completed && !panelToClear.receptor)
					panelToClear.Clear();

				typedLetters.Clear();

			}).SetUpdate(true).Done();
		}

		onKeyboardRefresh.Invoke();

		activePanel = typing ? lastHoveredPanel : null;

		Player.instance.velocity = Vector3.Scale(Player.instance.velocity, Vector3.up); // clear horizontal velocity
	}

	void DoInput()
	{
		DoDebugInput();

		if (!Input.anyKeyDown)
			return;

		if (activePanel == null)
			return;

		if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
		{
			if(activePanel.receptor)
			{
				if (activePanel.input != string.Empty)
					storedLetters.Remove(activePanel.input[0]);
			}
			if (activePanel.completed)
			{
				activePanel.Clear();

				typedLetters = activePanel.input.ToHashSet();
			}
			else if (activePanel.input.Length > 0)
			{
				var letter = activePanel.input[^1];
				activePanel.input = activePanel.input.Substring(0, activePanel.input.Length - 1);

				typedLetters.Remove(letter);
			}

			onKeyboardRefresh.Invoke();
		}

		if (activePanel.completed)
			return;

		if (activePanel.unsolvable && activePanel.singleSolution)
			return;

		if (Input.GetKeyDown(KeyCode.CapsLock) || Input.GetKeyDown(KeyCode.Insert))
		{
			if (!activePanel.hint && !activePanel.completed && activePanel.singleSolution && !activePanel.receptor)
			{
				activePanel.hint = true;
				activePanel.Clear();

				typedLetters = activePanel.input.ToHashSet();
				onKeyboardRefresh.Invoke();
			}
		}

		for(var i = 0; i < keycodes.Length; i++)
		{
			if (!Input.GetKeyDown(keycodes[i]))
				continue;

			var letter = LETTERS[i];

			if (!unlockedLetters.Contains(letter))
				continue;

			if (typedLetters.Contains(letter) && !infiniteLetters.Contains(letter))
				continue;

			if (storedLetters.Contains(letter))
				continue;

			if (activePanel.receptor)
			{
				if (activePanel.input != string.Empty)
					storedLetters.Remove(activePanel.input[0]);

				storedLetters.Add(letter);
				activePanel.input = letter.ToString();

				onKeyboardRefresh.Invoke();
				continue;
			}

			// wrap text
			if (activePanel.input.Length + 1 > activePanel.solution.Length && activePanel.solution.Length > 0)
			{
				if (activePanel.singleSolution)
					activePanel.input = activePanel.input.Substring(activePanel.solution.Length);

				typedLetters.Clear();
			}

			if (!infiniteLetters.Contains(letter))
				typedLetters.Add(letter);

			onKeyboardRefresh.Invoke();

			activePanel.input = activePanel.input + letter;
		}

		activePanel.RefreshPanel();

		if (activePanel.completed)
			SetTyping(false);
	}

	void DoRaycast()
	{
		ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
		Physics.Raycast(ray, out hit, range, LayerMask.GetMask("Panel"));

		if (hit.collider == lastHit)
			return;
		
		// unhover previous
		if (lastHoveredPanel != null) 
			lastHoveredPanel.SetHovered(false);

		// hover new
		if (hit.collider != null)
		{
			var panel = hit.collider.transform.GetComponentInParent<Panel>();
			panel.SetHovered(true);

			lastHoveredPanel = panel;
		}
		else
		{
			lastHoveredPanel = null;
		}

		lastHit = hit.collider;
	}

	public void UnlockLetter(int letter)
	{
		unlockedLetters.Add(LETTERS[letter]);
		onKeyboardRefresh.Invoke();
	}

	public void UnlockCyanLetter(int letter)
	{
		infiniteLetters.Add(LETTERS[letter]);
		onKeyboardRefresh.Invoke();
	}

	public void StoreLetter(int letter)
	{
		storedLetters.Add(LETTERS[letter]);
		onKeyboardRefresh.Invoke();
	}

	public void RecoverLetters()
	{
		if (storedLetters.Count == 0)
			return;

		storedLetters.Clear();
		onKeyboardRefresh.Invoke();
		AudioManager.Play(SoundType.Pickup);
	}

	void DoDebugInput()
	{
#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.R))
		{
			if (lastHoveredPanel != null && activePanel == null)
				lastHoveredPanel.Solve(0);
		}

		if (Input.GetKey(KeyCode.E))
		{
			if (lastHoveredPanel != null && activePanel == null)
				lastHoveredPanel.Clear();
		}

		if (Input.GetKeyDown(KeyCode.F1))
		{
			Debug.Log("saving");
			Persistence.panelData.DEBUGSAVE();
			Persistence.settings.DEBUGSAVE();
		}
		else if (Input.GetKeyDown(KeyCode.F2))
		{
			Debug.Log("deleting save");
			System.IO.File.Delete(Persistence.settings.path);
			System.IO.File.Delete(Persistence.panelData.path);
		}
#endif
	}
}
