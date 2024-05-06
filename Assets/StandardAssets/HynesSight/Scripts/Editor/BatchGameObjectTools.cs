using HynesSight;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HynesSightEditor
{
	public class BatchGameObjectTools : EditorWindow
	{
		enum Indexing
		{
			None,
			Numeric,
			Alphanumeric
		}

		static BatchGameObjectTools _activeWindow = null;

		Vector2 _scrollViewCurrent = Vector2.zero;

		bool _searchSelectionExpanded = false;
		bool _renameExpanded = false;
		bool _replaceGameObjectsExpanded = false;

		// START Replace
		GameObject _replaceGameObjectsTemplate = null;

		// START Rename
		string _batchName = "";
		char[] _alphabet = new char[26] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
		Indexing _indexing = Indexing.None;

		// START Search Selection
		bool _searchSelectionIncludeChildren = true;
		string _searchSelectionString = string.Empty;
		List<Object> _selectionSearchObjects = new List<Object>();

		[MenuItem("Tools/Batch GameObject Tools")]
		static void Init()
		{
			_activeWindow = (BatchGameObjectTools)GetWindow(typeof(BatchGameObjectTools), utility: false, "Batch GO Tools");
			_activeWindow.Show();
		}

		void OnGUI()
		{
			_scrollViewCurrent = EditorGUILayout.BeginScrollView(_scrollViewCurrent, GUILayout.ExpandWidth(true));

			GUISection_SearchSelection();

			EditorGUILayout.Space();

			GUISection_Rename();

			EditorGUILayout.Space();

			GUISection_ReplaceGameObjects();

			EditorGUILayout.EndScrollView();
		}

		void GUISection_SearchSelection()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_searchSelectionExpanded = EditorGUILayout.Foldout(_searchSelectionExpanded, "Search Scene Selection", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_searchSelectionExpanded)
			{
				float originalLabelWidth = EditorGUIUtility.labelWidth;

				EditorGUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 110f;
				_searchSelectionIncludeChildren = EditorGUILayout.Toggle("Include Children", _searchSelectionIncludeChildren);
				GUILayout.FlexibleSpace();
				EditorGUIUtility.labelWidth = originalLabelWidth;
				EditorGUILayout.EndHorizontal();

				const string searchTextFieldName = "SearchSelectionTextField";

				GUI.SetNextControlName(searchTextFieldName);

				_searchSelectionString = EditorGUILayout.TextField(_searchSelectionString, EditorStyles.toolbarSearchField);

				GUILayout.BeginHorizontal();

				bool searchPressed = GUILayout.Button(new GUIContent("Search",
					"Searches all selected objects in the Scene and stores them to a visible array. You can search by Components owned using \"t:Example\" (similar to Unity's built-in search)."));

				bool textFieldReturnPressed =
					GUI.GetNameOfFocusedControl() == searchTextFieldName &&
					(Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

				bool clearPressed = GUILayout.Button(new GUIContent("Clear", "Clears any currently displayed Search results."));

				bool selectResultsPressed = GUILayout.Button(new GUIContent("Select Results", "Changes the current Selection in the Editor to all current results from the search."));

				if (clearPressed || _searchSelectionString == null || _searchSelectionString.Length <= 0)
				{
					_selectionSearchObjects = new List<Object>();

					if (clearPressed)
						EditorGUI.FocusTextInControl(searchTextFieldName);
				}
				else if (selectResultsPressed)
				{
					Selection.objects = _selectionSearchObjects.ToArray();

					FocusWindowIfItsOpen<SceneView>();
				}
				else if ((searchPressed || textFieldReturnPressed) && Selection.objects.Length > 0)
				{
					string noCaseSearchString = _searchSelectionString.ToLowerInvariant();

					List<Object> objectsToSearch = new List<Object>(Selection.objects);
					if (_searchSelectionIncludeChildren)
					{
						foreach (Transform _selectedTransform in Selection.transforms)
						{
							List<GameObject> allChildren = _selectedTransform.gameObject.GetChildGameObjects();
							foreach (GameObject child in allChildren)
								objectsToSearch.AddUnique(child);
						}
					}

					// Note: Tried to make use of UnityEngine.Search, but couldn't find an exposed way to use its search expressions.
					//		 So instead, I've just hacked together some manual type filtering similar to theirs using "t:".
					//		 Could probably also have made things neater if I knew more Regex.

					string typeSearchString = "";
					int typeSearchStartIndex = noCaseSearchString.IndexOf("t:") + "t:".Length;
					if (typeSearchStartIndex >= "t:".Length)
					{
						int typeSearchEndIndex = noCaseSearchString.IndexOf(" ", typeSearchStartIndex);
						if (typeSearchEndIndex < 0)
							typeSearchEndIndex = noCaseSearchString.Length;

						int typeSearchLength = typeSearchEndIndex - typeSearchStartIndex;
						typeSearchString = noCaseSearchString.Substring(typeSearchStartIndex, typeSearchLength);
					}

					string nameSearchString =
						typeSearchString.Length <= 0 ? noCaseSearchString
						: noCaseSearchString.Replace("t:" + typeSearchString, string.Empty);

					string[] namesToSearch = nameSearchString.Split(new string[] { ",", " " }, System.StringSplitOptions.RemoveEmptyEntries);

					// Strip square brackets in case they were used like in Unity search expressions.
					// Note: we must not do this before separating out the _nameSearchString
					typeSearchString = typeSearchString.Replace("[", string.Empty);
					typeSearchString = typeSearchString.Replace("]", string.Empty);

					string[] typesToSearch = typeSearchString.Split(",");

					_selectionSearchObjects = new List<Object>(objectsToSearch.Count);

					foreach (Object objectToSearch in objectsToSearch)
					{
						// Check for a name match, exclude if none.
						bool nameCollisionFound = false;

						foreach (string nameToSearch in namesToSearch)
						{
							if (!objectToSearch.name.ToLowerInvariant().Contains(nameToSearch))
							{
								nameCollisionFound = true;
								break;
							}
						}

						if (nameCollisionFound)
							continue;

						// Check for a type match, include if any.
						List<Object> objectsToTypeSearch = new List<Object> { objectToSearch };
						if (objectToSearch is GameObject)
						{
							objectsToTypeSearch.AddRange(((GameObject)objectToSearch).GetComponents<Component>());
						}

						bool typeFound = false;

						foreach (string typeToSearch in typesToSearch)
						{
							foreach (Object objectToTypeSearch in objectsToTypeSearch)
							{
								// Check all derived types up the inheritance chain.
								for (System.Type type = objectToTypeSearch.GetType(); type != null; type = type.BaseType)
								{
									if (type.Name.ToLowerInvariant().Contains(typeToSearch))
									{
										typeFound = true;
										break;
									}
								}
							}
						}

						if (!typeFound)
							continue;

						_selectionSearchObjects.Add(objectToSearch);
					}
				}

				GUILayout.EndHorizontal();

				if (_selectionSearchObjects != null && _selectionSearchObjects.Count > 0)
				{
					EditorGUILayout.BeginVertical(EditorStyles.helpBox);

					for (int i = _selectionSearchObjects.Count - 1; i >= 0; --i)
					{
						EditorGUILayout.BeginHorizontal();

						// GUI.enabled allows us to make uneditable object fields for the search results.
						bool previousGuiEnable = GUI.enabled;
						GUI.enabled = false;
						EditorGUILayout.ObjectField(_selectionSearchObjects[i], typeof(Object), allowSceneObjects: true);
						GUI.enabled = previousGuiEnable;

						GUIContent deleteButtonContent = EditorGUIUtility.IconContent("TreeEditor.Trash");
						deleteButtonContent.tooltip = "Deletes this entry from the results.";

						bool deleteResultButtonPressed = GUILayout.Button(deleteButtonContent, GUILayout.Width(25f));

						if (deleteResultButtonPressed)
							_selectionSearchObjects.RemoveAt(i);

						EditorGUILayout.EndHorizontal();
					}

					EditorGUILayout.EndVertical();
				}
			}

			GUILayout.EndVertical();
		}

		void GUISection_Rename()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_renameExpanded = EditorGUILayout.Foldout(_renameExpanded, "Rename", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_renameExpanded)
			{
				// Input field for setting the batch name.
				EditorGUILayout.BeginHorizontal();
				GUI.SetNextControlName("Batch Name");
				EditorGUILayout.LabelField("Name", GUILayout.Width(60));
				GUILayout.FlexibleSpace();
				_batchName = EditorGUILayout.TextField(_batchName, GUILayout.Width(80));
				EditorGUILayout.EndHorizontal();

				// Dropdown list for setting the indexing style.
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Indexing", GUILayout.Width(60));
				GUILayout.FlexibleSpace();
				_indexing = (Indexing)EditorGUILayout.EnumPopup(_indexing, GUILayout.Width(80));
				EditorGUILayout.EndHorizontal();

				if (!GUILayout.Button("Rename"))
					Rename();
			}

			GUILayout.EndVertical();
		}

		void GUISection_ReplaceGameObjects()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_replaceGameObjectsExpanded = EditorGUILayout.Foldout(_replaceGameObjectsExpanded, "Replace GameObjects", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_replaceGameObjectsExpanded)
			{
				_replaceGameObjectsTemplate = (GameObject)EditorGUILayout.ObjectField("Template", _replaceGameObjectsTemplate, typeof(GameObject), allowSceneObjects: true);

				bool replacePressed = GUILayout.Button(new GUIContent("Replace",
					"All selected GameObjects will be replaced by the specified 'Template' GameObject."));

				if (replacePressed && _replaceGameObjectsTemplate != null)
				{
					Undo.SetCurrentGroupName("Replace selected GameObjects");
					int undoGroup = Undo.GetCurrentGroup();

					// Make sure we don't include the object that acts as our template if it's in the scene when doing the replace.
					List<Transform> transformsToReplace = new List<Transform>(Selection.transforms);
					transformsToReplace.Remove(_replaceGameObjectsTemplate.transform);

					foreach (Transform _transformToReplace in transformsToReplace)
					{
						GameObject newGameObject = null;

						// If selected in the scene, we want to do a normal Instantiate, otherwise we need to do a PrefabUtility instantiate to preserve the link to the asset.
						GameObject templatePrefab = PrefabUtility.GetCorrespondingObjectFromSource(_replaceGameObjectsTemplate);
						if (templatePrefab == _replaceGameObjectsTemplate)
							newGameObject = (GameObject)PrefabUtility.InstantiatePrefab(_replaceGameObjectsTemplate, _transformToReplace.gameObject.scene);
						else
							newGameObject = Instantiate(_replaceGameObjectsTemplate);

						Undo.RegisterCreatedObjectUndo(newGameObject, string.Format("replace selected GameObjects with {0}.", _replaceGameObjectsTemplate.name));

						newGameObject.transform.SetParent(_transformToReplace.parent);
						newGameObject.transform.SetSiblingIndex(_transformToReplace.GetSiblingIndex());
						newGameObject.transform.localPosition = _transformToReplace.localPosition;
						newGameObject.transform.localRotation = _transformToReplace.localRotation;
						newGameObject.transform.localScale = _transformToReplace.localScale;

						Undo.DestroyObjectImmediate(_transformToReplace.gameObject);
					}

					Undo.CollapseUndoOperations(undoGroup);
				}
			}

			GUILayout.EndVertical();
		}

		void Rename()
		{
			GameObject[] gameObjectsToRename = Selection.gameObjects;
			gameObjectsToRename.SortBySiblingIndex();

			if (gameObjectsToRename == null || gameObjectsToRename.Length == 0 || Selection.transforms[0].parent == null)
			{
				Debug.LogWarning("No GameObjects selected for rename.");
				return;
			}

			if (_batchName == "")
				_batchName = Selection.transforms[0].parent.GetChild(0).name;

			for (int i = 0; i < gameObjectsToRename.Length; i++)
			{
				switch (_indexing)
				{
					case Indexing.None:
						gameObjectsToRename[i].name = _batchName;
						break;

					case Indexing.Numeric:
						gameObjectsToRename[i].name = _batchName;
						gameObjectsToRename[i].name += " " + i;
						break;

					case Indexing.Alphanumeric:
						gameObjectsToRename[i].name = _batchName;
						gameObjectsToRename[i].name += " " + (i / 26);
						gameObjectsToRename[i].name += _alphabet[i % 26];
						break;
				}
			}
		}
	}
}
