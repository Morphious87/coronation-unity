
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

// Modified version of https://youtu.be/EFh7tniBqkk?si=cpv2tVHuG_5GyaVT

[InitializeOnLoad]
public static class HierarchyIconDisplay
{
	//
	//	PREFERENCES YOU CAN CHANGE
	//

	const bool KeepPrefabIcon = false;
	// true: the old prefab icon is kept
	// false: a prefab indicator is shown in the top right of the icon

	const bool ShowDefaultScriptIcon = true;
	// true: components with no special icon show the C# script icon
	// false: components with no special icon show the old cube icon

	const bool UseFolderIcon = true;
	// true: objects with no components, with at least 1 child, display as a folder
	// false: folders are ignored

	// vars
	static bool _hierarchyHasFocus = false;
	static EditorWindow _hierarchyEditorWindow;
	static bool _mouseDownOverNextSelection = false;
	static Texture _defaultScriptIcon;

	static HierarchyIconDisplay()
	{
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
		EditorApplication.update += OnEditorUpdate;

		_defaultScriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
	}

	static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
	{
		if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject obj)
			return;

		var isPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(obj);
		if (KeepPrefabIcon && isPrefab)
			return;

		var components = obj.GetComponents<Component>();
		if (components == null || components.Length == 0)
			return;

		Component primaryComponent = components.Length > 1 ? components[1] : components[0];

		Type type = primaryComponent.GetType();

		// display tmp or image instead of canvas renderer
		if (type == typeof(CanvasRenderer)) 
		{
			if (components.Length > 2)
			{
		  		primaryComponent = components[2];
		  		type = primaryComponent.GetType();
			}
			else
			{
				primaryComponent = components[0];
		  		type = primaryComponent.GetType();
			}
		}

		GUIContent content;

		// determine the icon to display
		if (UseFolderIcon && primaryComponent is Transform tr && tr.childCount != 0)
		{
			// check if the gameobject is collapsed or expanded, using reflection
			// https://discussions.unity.com/t/how-to-check-if-item-is-expanded-folded-or-collapsed-unfolded-in-the-hierarchy-view/716986
			var _sceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
			var _getExpandedIDs = _sceneHierarchyWindowType.GetMethod("GetExpandedIDs", BindingFlags.NonPublic | BindingFlags.Instance);
			var _lastInteractedHierarchyWindow = _sceneHierarchyWindowType.GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);
			if (_lastInteractedHierarchyWindow == null) return;
			var _expandedIDs = _getExpandedIDs.Invoke(_lastInteractedHierarchyWindow.GetValue(null), null) as int[];
	
			var isExpanded = _expandedIDs.Contains(obj.GetInstanceID());

			content = EditorGUIUtility.IconContent(isExpanded ? "FolderOpened Icon" : "Folder Icon");
		}
		else
		{
			content = EditorGUIUtility.ObjectContent(primaryComponent, type);
			content.text = null;
			content.tooltip = type.Name;
		}

		if (content.image == null)
			return;

		if (!ShowDefaultScriptIcon && content.image == _defaultScriptIcon)
			return;

		var isHovered = selectionRect.Contains(Event.current.mousePosition);

		var e = Event.current;
		if (e.type is EventType.MouseDown && Event.current.button == 0)
			_mouseDownOverNextSelection = isHovered;
		else if (e.type is EventType.MouseUp && Event.current.button == 0)
			_mouseDownOverNextSelection = false;

		var isSelecting = isHovered && _mouseDownOverNextSelection;
		var isSelected = Selection.instanceIDs.Contains(instanceID);

		var color = UnityEditorBackgroundColor.Get(_mouseDownOverNextSelection ? isSelecting : isSelected, isHovered, _hierarchyHasFocus);
		var backgroundRect = selectionRect;
		backgroundRect.width = 18.5f;
		EditorGUI.DrawRect(backgroundRect, color);

		Color originalColor = GUI.color;
        if (!obj.activeInHierarchy)
        {
            Color transparentIconColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            GUI.color = transparentIconColor;
        }

		EditorGUI.LabelField(selectionRect, content);

		if (isPrefab)
		{
			var prefabIconRect = selectionRect;
			prefabIconRect.height = selectionRect.height * 0.5f;
			prefabIconRect.x += selectionRect.height * 0.5f;
			EditorGUI.LabelField(prefabIconRect, EditorGUIUtility.IconContent("Prefab Icon"));
		}

        GUI.color = originalColor;
	}

	static void OnEditorUpdate()
	{
		var hierarchyWindowType = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor");
		var foundWindows = Resources.FindObjectsOfTypeAll(hierarchyWindowType);

		if (_hierarchyEditorWindow == null && foundWindows?.Length > 0)
			_hierarchyEditorWindow = foundWindows[0] as EditorWindow;

		_hierarchyHasFocus = EditorWindow.focusedWindow != null &&
			EditorWindow.focusedWindow == _hierarchyEditorWindow;
	}

	public static class UnityEditorBackgroundColor
	{
		static readonly Color k_defaultColor = new Color(0.7843f, 0.7843f, 0.7843f);
		static readonly Color k_defaultProColor = new Color(0.2196f, 0.2196f, 0.2196f);

		static readonly Color k_selectedColor = new Color(0.22745f, 0.447f, 0.6902f);
		static readonly Color k_selectedProColor = new Color(0.1725f, 0.3647f, 0.5294f);

		static readonly Color k_selectedUnFocusedColor = new Color(0.68f, 0.68f, 0.68f);
		static readonly Color k_selectedUnFocusedProColor = new Color (0.3f, 0.3f, 0.3f);

		static readonly Color k_hoveredColor = new Color (0.698f, 0.698f, 0.698f);
		static readonly Color k_hoveredProColor = new Color (0.2706f, 0.2706f, 0.2706f);

		public static Color Get(bool isSelected, bool isHovered, bool isWindowFocused)
		{
			var pro = EditorGUIUtility.isProSkin;

			if (isSelected)
			{
				if (isWindowFocused)
					return pro ? k_selectedProColor : k_selectedColor;
				else
					return pro ? k_selectedUnFocusedProColor : k_selectedUnFocusedColor;
			}
			else if (isHovered)
				return pro ? k_hoveredProColor : k_hoveredColor;
			else
				return pro ? k_defaultProColor : k_defaultColor;
		}
	}
}
#endif
