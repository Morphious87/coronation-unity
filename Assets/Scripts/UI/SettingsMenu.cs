using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	public ToggleGroup crosshairGroup;
	public Toggle toggleCrosshairDefault, toggleCrosshairBigger, toggleCrosshairNone;
	public Toggle toggleAutoRun, toggleOutlines;
	public Toggle toggleMusic, toggleSounds;
	public Slider sliderMusic, sliderSounds;
	public Slider sliderSpeed;
	public Toggle toggleInvertY;
	public Slider sliderSensitivity;

	public SettingsData settings => Persistence.settings;

	bool ignoreUIRefresh = false;

	void Awake()
	{
		AddListeners(new Selectable[]
		{
			toggleCrosshairDefault, toggleCrosshairBigger, toggleCrosshairNone,
			sliderSpeed, toggleAutoRun, 
			toggleOutlines,
			toggleMusic, toggleSounds,
			sliderMusic, sliderSounds,
			toggleInvertY,
			sliderSensitivity,
		});
	}

    void AddListeners(Selectable[] elements)
	{
		foreach(var element in elements)
			if (element is Toggle toggle)
				toggle.onValueChanged.AddListener(delegate { Apply(); });
			else if (element is Slider slider)
				slider.onValueChanged.AddListener(delegate { Apply(); });
	}

	void Start()
	{
		SetUI();
	}

	void SetUI()
	{
		ignoreUIRefresh = true;

		toggleCrosshairDefault.isOn = settings.crosshairType is CrosshairType.Default;
		toggleCrosshairBigger.isOn = settings.crosshairType is CrosshairType.Big;
		toggleCrosshairNone.isOn = settings.crosshairType is CrosshairType.None;

		sliderMusic.value = settings.musicVolume;
		sliderSounds.value = settings.soundVolume;
		toggleMusic.isOn = settings.musicEnabled;
		toggleSounds.isOn = settings.soundEnabled;

		toggleInvertY.isOn = settings.invertY;
		toggleOutlines.isOn = settings.outlines;
		toggleAutoRun.isOn = settings.autoRun;

		sliderSensitivity.value = settings.sensitivity;
		sliderSpeed.value = settings.playerSpeed;

		ignoreUIRefresh = false;
	}

	void Apply()
	{
		if (ignoreUIRefresh)
			return;

		// apply to persistence
		settings.crosshairType = toggleCrosshairBigger.isOn ? CrosshairType.Big : toggleCrosshairNone.isOn ? CrosshairType.None : CrosshairType.Default;

		settings.musicVolume = sliderMusic.value;
		settings.soundVolume = sliderSounds.value;
		settings.musicEnabled = toggleMusic.isOn;
		settings.soundEnabled = toggleSounds.isOn;

		settings.invertY = toggleInvertY.isOn;
		settings.outlines = toggleOutlines.isOn;

		settings.playerSpeed = sliderSpeed.value;
		settings.autoRun = toggleAutoRun.isOn;

		settings.sensitivity = sliderSensitivity.value;
	}
}
