using UnityEngine;
using UnityEngine.Rendering;

public class MainCamera : MonoBehaviour {
/*
	Portal[] portals;

	void Awake () 
	{
		portals = FindObjectsByType<Portal> (FindObjectsSortMode.None);

	}

	void OnPreCull (ScriptableRenderContext context, Camera camera) 
	{
		for (int i = 0; i < portals.Length; i++) 
			portals[i].PrePortalRender ();

		for (int i = 0; i < portals.Length; i++)
			portals[i].Render ();

		for (int i = 0; i < portals.Length; i++)
			portals[i].PostPortalRender ();
	}

	protected void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += OnPreCull;
	}


	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= OnPreCull;
	}
*/
}