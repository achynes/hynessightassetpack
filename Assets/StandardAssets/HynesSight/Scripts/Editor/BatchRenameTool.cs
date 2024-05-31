using UnityEngine;
using UnityEditor;

namespace HynesSightEditor
{
    /// <summary>
    /// Editor tool for renaming GameObjects to share one name, with options for indexing.
    /// </summary>
    public class BatchRenameTool : EditorWindow
    {
	    enum Indexing
	    {
		    None, Numeric, Alphabetical, Alphanumeric
	    }

	    private static BatchRenameTool _thisWindow;

        #region Member Variables

        private string _batchName = "";

        private char[] _alphabet = new char[26] {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};

        private Indexing _indexing = Indexing.None;

        private bool _shortcutInstructionsDroppedDown;

        private GUIStyle _wrappedTextStyle;

        private static bool _newWindowOpened = false;

        #endregion


	    #region Methods

        [MenuItem("Tools/Batch Rename Tool %#r")]
        public static void OpenWindow()
        {
            _thisWindow = EditorWindow.GetWindow<BatchRenameTool>("Rename", true);
            _newWindowOpened = true;
        }

        private void OnGUI()
	    {
		    // Formatting Methods
		    SetWindowInstance();
		    WindowFormatting();

		    // GUI Display Methods
		    EditorGUILayout.Space();
		    InputSection();
		    EditorGUILayout.Space();
		    RenameButton();
		    ShortcutInstructionsFoldout();

		    BatchNameFieldFocusing();
		    KeyboardShortcuts();
	    }

        /// <summary>
        /// Fields for setting batch name and indexing style.
        /// </summary>
        private void InputSection()
	    {
		    // Input field for setting the batch name.
		    EditorGUILayout.BeginHorizontal();
		    GUI.SetNextControlName("Batch Name");
		    EditorGUILayout.LabelField("Name:", GUILayout.Width(60));
		    GUILayout.FlexibleSpace();
		    _batchName = EditorGUILayout.TextField(_batchName, GUILayout.Width(80));
		    EditorGUILayout.EndHorizontal();

		    // Dropdown list for setting the indexing style.
		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("Indexing:", GUILayout.Width(60));
		    GUILayout.FlexibleSpace();
		    _indexing = (Indexing)EditorGUILayout.EnumPopup(_indexing, GUILayout.Width(80));
		    EditorGUILayout.EndHorizontal();
	    }

        /// <summary>
        /// Button to rename the GameObjects with the above name and indexing, also accessible with the return key.
        /// </summary>
        private void RenameButton()
	    {
		    if (GUILayout.Button("Rename"))
            {
			    RenameSelectedGameObjects(SortUtility.SortGameObjectsSelectionBySiblingIndex(Selection.gameObjects), _indexing);
            }
	    }

        /// <summary>
        /// Keyboard shortcuts for auto-renaming with a selected indexing style, and a Return-Key event for confirming the current selected options in the window.
        /// </summary>
        private void KeyboardShortcuts()
	    {
			if (Selection.transforms == null || Selection.transforms.Length == 0 || Selection.transforms[0].parent == null)
            {
                return;
            }

		    for (int counter = 0; counter < 4; counter++)
		    {
			    if (Event.current.Equals(Event.KeyboardEvent(counter.ToString()))
                    && RenameSelectedGameObjects(SortUtility.SortGameObjectsSelectionBySiblingIndex(Selection.gameObjects), (Indexing)counter))
			    {
					_thisWindow.Close();
			    }	
		    }
	    }

        private void SetWindowInstance()
	    {
		    if (_thisWindow == null)
            {
			    _thisWindow = this;
            }
	    }

        /// <summary>
        /// Focuses the batch name text field when the window is opened.
        /// </summary>
        private void BatchNameFieldFocusing()
	    {
		    // Auto-focuses the field when the window is opened.
		    if (_newWindowOpened)
		    {
			    EditorGUI.FocusTextInControl("Batch Name");
			    _newWindowOpened = false;
		    }
	    }

        /// <summary>
        /// Foldout with keyboard shortcut instructions.
        /// </summary>
        private void ShortcutInstructionsFoldout()
	    {
		    _shortcutInstructionsDroppedDown = EditorGUILayout.Foldout(_shortcutInstructionsDroppedDown, "Keyboard Shortcuts");

		    if (_shortcutInstructionsDroppedDown)
            {
			    EditorGUILayout.LabelField("0: None\n1: Numeric\n2: Alphabetical\n3: Alphanumeric", _wrappedTextStyle);
            }
	    }

        /// <summary>
        /// Renames the selected GameObjects, with optional indexing.
        /// </summary>
        /// <returns><c>true</c>, if the selected GameObjects were renamed successfully, <c>false</c> otherwise.</returns>
        /// <param name="gameObjectsToRename_">GameObjects to rename.</param>
        /// <param name="indexing_">Indexing style; numeric, alphabetical, or alphanumeric.</param>
        private bool RenameSelectedGameObjects(GameObject[] gameObjectsToRename_, Indexing indexing_ = Indexing.None)
	    {
		    if (gameObjectsToRename_ == null || gameObjectsToRename_.Length == 0 || Selection.transforms[0].parent == null)
            {
                return false;
            }

		    if (_batchName == "")
            {
			    _batchName = Selection.transforms[0].parent.GetChild(0).name;
            }

		    for (int counter = 0; counter < gameObjectsToRename_.Length; counter++)
		    {
			    switch (indexing_)
			    {
				    case Indexing.None:
					    gameObjectsToRename_[counter].name = _batchName;
					    break;

				    case Indexing.Numeric:
					    gameObjectsToRename_[counter].name = _batchName;
					    gameObjectsToRename_[counter].name += " " + counter;
					    break;

				    case Indexing.Alphabetical:
					    if (gameObjectsToRename_.Length > _alphabet.Length)
					    {
						    Debug.LogWarning("Error: too many GameObjects selected to use alphabetical indexing.");
						    return false;
					    }
					    gameObjectsToRename_[counter].name = _batchName;
					    gameObjectsToRename_[counter].name += " " + _alphabet[counter % 26];
					    break;

					case Indexing.Alphanumeric:
					    gameObjectsToRename_[counter].name = _batchName;
					    gameObjectsToRename_[counter].name += " " + (counter/26);
					    gameObjectsToRename_[counter].name += _alphabet[counter % 26];
					    break;
			    }
		    }

		    return true;
	    }

        /// <summary>
        /// Formats the EditorWindow.
        /// </summary>
        private void WindowFormatting()
	    {
		    _wrappedTextStyle = new GUIStyle();
		    _wrappedTextStyle.wordWrap = true;
		    _wrappedTextStyle.padding = new RectOffset(10, 10, 0, 0);

		    if (_shortcutInstructionsDroppedDown)
		    {
		        // Additional size for shortcut instructions foldout.
			    _thisWindow.minSize = new Vector2(150, 150);
			    _thisWindow.maxSize = new Vector2(150, 150);
		    }
		    else
		    {
		        // Base size of the EditorWindow.
			    _thisWindow.minSize = new Vector2(150, 90);
			    _thisWindow.maxSize = new Vector2(150, 90);
		    }
	    }

	    #endregion
    }
}