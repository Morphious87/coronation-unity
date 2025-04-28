using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Panel : MonoBehaviour
{
	[Header("Components")]
	public TextMeshPro upper;
	public TextMeshPro middle;
	public TextMeshPro bottom;
	public MeshRenderer background;
	public GameObject multipleSolutionsDot;
	public Color _unsolvedColor, _hoveredColor, _solvedColor, _unsolvableColor;

	[Header("Config")]
	public string upperText;
	public string middleText;
	public string bottomText;
	public bool reversedPuzzle;
	public string identifier;
	public bool receptor;

	[Header("Multiple Solutions")]
	public string[] altSolutions;
	public Trigger[] triggers;

	// private

	[NonSerialized] public string UID;

	[NonSerialized] public string input = "";
	[NonSerialized] public bool hint = false;
	[NonSerialized] public bool completed = false;
	[NonSerialized] public bool hovered = false;
	[NonSerialized] public bool unsolvable = false;

	public string solution => reversedPuzzle ? upperText : bottomText;
	public bool singleSolution => altSolutions.Length == 0;

	[NonSerialized] public int longestSolutionLength = 0;
	[NonSerialized] public int lastTriggeredSolution = 0;

	void Awake()
	{
		background.material = Instantiate(background.material);
		upper.enableAutoSizing = false;
		middle.enableAutoSizing = false;
		bottom.enableAutoSizing = false;

		longestSolutionLength = solution.Length;
		foreach(var altSolution in altSolutions)
			if (altSolution.Length > longestSolutionLength) 
				longestSolutionLength = altSolution.Length;

		PlayerTyping.instance.onKeyboardRefresh += RefreshPanel;

		UID = gameObject.name;
		if (identifier != string.Empty)
			UID += $".{identifier}";
	}

	void Start()
	{
		var panelData = Persistence.panelData;
		if (singleSolution)
		{
			if (panelData.completedPanels.Contains(UID))
				Solve();
		}
		else
		{
			if (panelData.altPanels.Keys.Contains(UID))
				input = panelData.altPanels[UID];
		}
		panelData.Save();

		if (input == string.Empty)
			Clear();
	}

	public bool CheckSolve()
	{
		if (receptor)
			return false;

		if (solution.Length == 0)
			return input.Length != 0;

		bool CheckSolution(string i, string s) => s.Length == 1 ? i.EndsWith(s) : i == s;

		for(var i = 0; i < altSolutions.Length; i++)
			if (CheckSolution(input, altSolutions[i]))
			{
				Solve(i + 1);
				return true;
			}

		if (CheckSolution(input, solution) && !completed)
		{
			Solve();
			return true;
		}

		return false;
	}

	public void RefreshPanel()
	{
		middle.text = middleText;

		if (receptor && input != string.Empty && !PlayerTyping.instance.storedLetters.Contains(input[0]))
			input = string.Empty;

		// control
		var control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift);
		var skipSolve = control && input.Length < longestSolutionLength;
		if (!completed && !skipSolve)
			if (Application.isPlaying && CheckSolve())
				return;

		// visual stuff
		upper.text = Application.isPlaying && reversedPuzzle ? (completed ? input : Blankify(upperText, input)) : upperText;
		bottom.text = Application.isPlaying && !reversedPuzzle ? (completed ? input : Blankify(bottomText, input)) : bottomText;

		if (multipleSolutionsDot)
			multipleSolutionsDot.SetActive(!singleSolution);

		if (!Application.isPlaying)
			return;

		unsolvable = !IsSolvable(completed ? input : solution);

		background.material.color = unsolvable ? _unsolvableColor : 
									completed ? _solvedColor : 
									hovered   ? _hoveredColor : 
												_unsolvedColor;
	}

	public bool IsSolvable(string solutionToCheck)
	{
		if (receptor)
			return true;

		var typing = PlayerTyping.instance;

		var solutionSet = solutionToCheck.ToHashSet();

		if (typing.storedLetters.Overlaps(solutionSet))
			return false;

		var moreThanEnough = typing.infiniteLetters.IsSupersetOf(solutionSet);
		if (moreThanEnough)
			return true;
		
		var enoughRegular = typing.unlockedLetters.IsSupersetOf(solutionSet);
		if (!enoughRegular)
			return false;

		// checking this last because it's the most expensive check to do
		var repeatedLetters = solutionToCheck.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();
		var enoughCyan = typing.infiniteLetters.IsSupersetOf(repeatedLetters);
		if (!enoughCyan)
			return false;
		
		return true;
	}

	public void SetHovered(bool hovered)
	{
		this.hovered = hovered;
		RefreshPanel();
	}

	public void Clear()
	{
		if (completed)
		{
			AudioManager.Play(SoundType.Unsuccess);

			if (singleSolution)
				foreach(var trigger in triggers)
					trigger.Interact(false);
			else if (lastTriggeredSolution < triggers.Length)
				triggers[lastTriggeredSolution]?.Interact(false);

			if (hint && solution.Length <= 1)
				hint = false;
		}

		completed = false;
		input = hint ? solution.Substring(0, 1) : "";

		RefreshPanel();

		var panelData = Persistence.panelData;
		if (singleSolution)
		{
			if (panelData.completedPanels.Contains(UID))
				panelData.completedPanels.Remove(UID);
		}
		else
		{
			if (panelData.altPanels.Keys.Contains(UID))
				panelData.altPanels.Remove(UID);
		}
		panelData.Save();
	}

	public void Solve(int solutionIndex = 0)
	{
		if (completed)
			return;

		completed = true;
		input = solutionIndex == 0 ? solution : altSolutions[solutionIndex - 1];

		RefreshPanel();

		AudioManager.Play(SoundType.Success);

		if (singleSolution)
			foreach(var trigger in triggers)
				trigger.Interact(true);
		else if (solutionIndex < triggers.Length)
		{
			triggers[solutionIndex]?.Interact(true);
			lastTriggeredSolution = solutionIndex;
		}

		var panelData = Persistence.panelData;
		if (singleSolution)
		{
			if (!panelData.completedPanels.Contains(UID))
				panelData.completedPanels.Add(UID);
		}
		else
		{
			if (panelData.altPanels.Keys.Contains(UID))
				panelData.altPanels[UID] = input;
			else
				panelData.altPanels.Add(UID, input);
		}

		panelData.Save();
	}

	static string Blankify(string target, string input)
	{
		if (target.Length == 0)
			return "";

		var subString = input?.Length > 0 ? input.Substring((input.Length - 1) / target.Length * target.Length) : "";
		return subString.PadRight(target.Length, '_');
	}

	void OnDestroy()
	{
		PlayerTyping.instance.onKeyboardRefresh -= RefreshPanel;
	}

#if UNITY_EDITOR
	public void OnValidate()
	{
		if (Application.isPlaying)
			return;

		upperText = upperText.ToUpper();
		bottomText = bottomText.ToUpper();

		if (PrefabStageUtility.GetCurrentPrefabStage() == null)
			gameObject.name = "Panel: " + upperText + " -> " + bottomText;
	}

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

#if UNITY_EDITOR
[CustomEditor(typeof(Panel))]
public class PanelInspector : Editor
{
	List<Texture2D> symbolTextures;
	(string, string)[][] symbols;

	private void OnEnable()
	{
		symbols = new (string, string)[][]
		{
			new (string, string)[]
			{
				("smile","@"),
				("synonym","ʖ"),
				("soundalike","ʚ"),
				("add","ʘ"),
				("category","ʛ"),
				("triangle","ʒ"),
				("surrounded","ʓ"),
				("time","ʗ"),
				("energy","ʔ"),
				("sound","ʐ"),
			},

			new (string, string)[]
			{
				("plural","ʜ"),
				("sweeten","ʑ"),
				("gender","ʕ"),
				("context","ʝ"),
				("scramble","ʞ"),
				("house","ʡ"),
				("action","ʟ"),
				("update","Ø"),
				("pig","ʢ"),
			},

			new (string, string)[]
			{
				("repetitive1","ɩ"),
				("repetitive2","ɪ"),
				("repetitive3","ɫ"),
				("repetitive4","ɬ"),
				("repetitive5","ɭ"),
				("flip","ɮ"),
				("weird","☹"),
				("music","♫"),
			},
			
			new (string, string)[]
			{
				("split","|"),
				("splitdouble","ǁ"),
				("splitupper","ɨ"),
			},

			new (string, string)[]
			{
				("negation","̓"),
				("wordification","̒"),
				("repeat1","̘"),
				("repeat2","̙"),
				("repeat3","̝"),
				("repeat4","̞"),
			},
		};

		symbolTextures = new();
		for (int i = 0, x = 0; x < symbols.Length; x++)
		{
			var symbolsRow = symbols[x];

			for (int y = 0; y < symbolsRow.Length; y++)
			{
				var symbol = symbolsRow[y];
				var texture = Resources.Load<Texture2D>(symbol.Item1);
				symbolTextures.Add(texture);

				i++;
			}
		}
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();

		base.OnInspectorGUI();

		if (GUILayout.Button("Check overlapping IDs (Playmode only!)"))
		{
			var hashset = new HashSet<string>();
			var duplicates = new HashSet<string>();

			foreach(var panel in FindObjectsByType<Panel>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID))
			{
				if (hashset.Contains(panel.UID))
				{
					duplicates.Add(panel.UID);
					continue;
				}
				
				hashset.Add(panel.UID);
			}

			if (duplicates.Count == 0)
				Debug.Log("No duplicates!");
			else
				Debug.Log($"FOUND DUPLICATES: {string.Join('\n', duplicates)}");
		}

		GUILayout.Space(20);
		GUILayout.EndVertical();
		GUILayout.BeginHorizontal();
		EditorGUILayout.Space();
		GUILayout.Label("Symbol Editor", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
		EditorGUILayout.Space();
		GUILayout.EndHorizontal();
		GUILayout.BeginVertical();

		for (int i = 0, x = 0; x < symbols.Length; x++)
		{
			var symbolsRow = symbols[x];

			GUILayout.EndVertical();
			GUILayout.BeginHorizontal();
			
			EditorGUILayout.Space();
			for (int y = 0; y < symbolsRow.Length; y++)
			{
				var symbol = symbolsRow[y];
				GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
				{
					fixedWidth = 24,
					fixedHeight = 24,
					normal = { background = symbolTextures[i] } // Set the texture
				};

				if (GUILayout.Button(GUIContent.none, buttonStyle))
				{
					var panel = target as Panel;
					panel.middleText += symbol.Item2;
				}

				i++;
			}
			EditorGUILayout.Space();

			GUILayout.EndHorizontal();
			GUILayout.BeginVertical();
		}

		GUILayout.EndVertical();
		GUILayout.BeginHorizontal();
		EditorGUILayout.Space();
		if (GUILayout.Button("<", new GUIStyle(GUI.skin.button) { fixedWidth = 24, fixedHeight = 24 }))
		{
			var panel = target as Panel;
			panel.middleText = panel.middleText.Substring(0, panel.middleText.Length - 1);
		}
		EditorGUILayout.Space();
		GUILayout.EndHorizontal();
		GUILayout.BeginVertical();

		if (EditorGUI.EndChangeCheck())
		{
			var panel = target as Panel; 
			EditorUtility.SetDirty(panel.gameObject);
			foreach(var component in panel.GetComponentsInChildren<Component>())
				EditorUtility.SetDirty(component);

			PrefabUtility.RecordPrefabInstancePropertyModifications(panel.gameObject);
			panel.RefreshPanel();
		}
	}
}
#endif

[Serializable]
public class PanelEvent : UnityEvent<string,GameObject> {}
