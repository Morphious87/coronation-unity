using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public static class Persistence
{
	public static SettingsData settings;
	public static PanelsData panelData;

	[RuntimeInitializeOnLoadMethod]
	public static void Load()
	{
		settings = new SettingsData().Load("settings");

		panelData = new PanelsData().Load("panels");
		panelData.completedPanels ??= new();
		panelData.altPanels ??= new();
		panelData.collectibles ??= new();
	}

	public static void Save()
	{
		settings.Save();
		panelData.Save();
	}
}

public class PersistentData<T> where T : PersistentData<T>, new()
{
	[NonSerialized] public string path;
	public T Load(string dataName)
	{
#if PLATFORM_WEBGL && !UNITY_EDITOR
		path = dataName;
#else
		path = Path.Combine(Application.persistentDataPath, $"{dataName}.json");
#endif

		string text = null;
		
#if PLATFORM_WEBGL && !UNITY_EDITOR
		text = PlayerPrefs.GetString(dataName, null);
#else
		if (File.Exists(path))
			text = File.ReadAllText(path);
#endif
		if (text != null)
			JsonUtility.FromJsonOverwrite(text, this);

		return (T)Convert.ChangeType(this, typeof(T));
	}

	public void Save()
	{
#if UNITY_EDITOR
	return;
#endif
		var text = JsonUtility.ToJson(this);

#if PLATFORM_WEBGL && !UNITY_EDITOR
		PlayerPrefs.SetString(path, text);
		PlayerPrefs.Save();
#else
		File.WriteAllText(path, text);
#endif
	}

#if UNITY_EDITOR
	public void DEBUGSAVE()
	{
		var text = JsonUtility.ToJson(this);
		File.WriteAllText(path, text);
	}
#endif
}

public enum CrosshairType
{
	Default, Big, None
}

[Serializable]
public class SettingsData : PersistentData<SettingsData>
{
	public bool outlines = true;

	public float sensitivity = 0.5f;
	public float playerSpeed = 0.5f;

	public float musicVolume = 0.666f;
	public bool musicEnabled = true;

	public float soundVolume = 1f;
	public bool soundEnabled = true;

	public CrosshairType crosshairType;

	public bool invertY;

	public bool autoRun;
}

[Serializable]
public class PanelsData : PersistentData<PanelsData>
{
	public List<string> completedPanels;
	public StringDictionary altPanels;
	public List<string> collectibles;

	public bool collectedMinus = false;
}

[Serializable] public class StringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField] List<TKey> keys = new List<TKey>();
	[SerializeField] List<TValue> values = new List<TValue>();
	
	public void OnBeforeSerialize()
	{
		keys.Clear();
		values.Clear();
		foreach(KeyValuePair<TKey, TValue> pair in this)
		{
			keys.Add(pair.Key);
			values.Add(pair.Value);
		}
	}
	
	public void OnAfterDeserialize()
	{
		Clear();

		if(keys.Count != values.Count)
			throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

		for(int i = 0; i < keys.Count; i++)
			Add(keys[i], values[i]);
	}
}