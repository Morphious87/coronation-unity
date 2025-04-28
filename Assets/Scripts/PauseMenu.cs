using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;

public class PauseMenu : MonoBehaviour
{
	public GameObject settingsScreen;
	public GameObject pauseScreen;

	public GameObject exitButton;

	public TextMeshProUGUI crosshair;
	public UniversalRendererData renderData;

	public static bool paused { get; protected set; }

	void Awake()
	{
		SetPaused(false);

#if UNITY_WEBGL && !UNITY_EDITOR
		exitButton.SetActive(false);
#endif
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			SetPaused(!paused);

		// this shouldn't be here... but oh well
		crosshair.text = Persistence.settings.crosshairType is CrosshairType.Big ? "+" : Persistence.settings.crosshairType is CrosshairType.None ? "" : ".";
		if (renderData.TryGetRendererFeature<FullScreenPassRendererFeature>(out var outlineFeature))
			outlineFeature.SetActive(Persistence.settings.outlines);
	}

	public void SetPaused(bool paused)
	{
		PauseMenu.paused = paused;
		PlayerTyping.instance.SetTyping(false);
		Time.timeScale = paused ? 0 : 1;
		Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
		AudioListener.pause = paused;

		if (paused) 
			ShowScreen(PauseScreen.Pause);
		else 
			ShowScreen(PauseScreen.None);
	}

	public void Exit()
	{
		Application.Quit();
	}

	public enum PauseScreen
	{
		None, Pause, Settings
	}

	PauseScreen previousScreen;

	public void ShowScreen(int screen) => ShowScreen((PauseScreen)screen);

	public void ShowScreen(PauseScreen screen)
	{
		settingsScreen.SetActive(screen is PauseScreen.Settings);
		pauseScreen.SetActive(screen is PauseScreen.Pause);

		if (previousScreen != screen && previousScreen is PauseScreen.Settings)
			Persistence.settings.Save();

		previousScreen = screen;
	}
}
