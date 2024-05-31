using UnityEngine;
using UnityEditor;

namespace HynesSightEditor
{
    public class BatchTransformTool : EditorWindow
    {
        private static BatchTransformTool _thisWindow;

        #region Member Variables

        // Transform variables.
        private Transform _rootTransform;

        private bool _editXAxis = false, _editYAxis = false, _editZAxis = false;

	    enum EditingMode
	    {
		    Align,
		    Distribute,
		    Grid
	    }

        private EditingMode _editingMode;

        private Transform _distributeEndTransform;

		private int _gridCountX = 1, _gridCountY = 1, _gridCountZ = 1;
        private float _gridSpacingX = 5.0f, _gridSpacingY = 5.0f, _gridSpacingZ = 5.0f;

        // Editor variables.
        private bool _instructionsDroppedDown = false;
        private GUIStyle _wrappedTextStyle;

        #endregion

        #region Methods

        [MenuItem("Tools/Batch Transform Tool %#t")]
        public static void OpenWindow()
        {
            System.Reflection.Assembly assembly = typeof(Editor).Assembly;
            System.Type inspectorWindow = assembly.GetType("UnityEditor.InspectorWindow");

            _thisWindow = EditorWindow.GetWindow<BatchTransformTool>("Transform", true, inspectorWindow);
        }

        private void OnGUI()
        {
            // Formatting Methods
            SetWindowInstance();
            WindowFormatting();

            // GUI Display Methods
            EditorGUILayout.Space();
            GeneralInputSection();
            switch (_editingMode)
            {
                case EditingMode.Align:
                    AlignInputSection();
                    break;

				case EditingMode.Distribute:
                    DistributeInputSection();
                    break;

				case EditingMode.Grid:
                    GridInputSection();
                    break;
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
        /// Section for user input relevent to all Transform Modes.
        /// </summary>
        private void GeneralInputSection()
	    {
		    // Enum dropdown to select editing mode: Align, Distribute or Grid.
		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("Mode:", GUILayout.Width(120));
		    GUILayout.FlexibleSpace();
		    _editingMode = (EditingMode)EditorGUILayout.EnumPopup(_editingMode);
		    EditorGUILayout.EndHorizontal();

		    // Set of toggles to select which axes to edit on the selected transforms.
		    EditorGUILayout.BeginHorizontal();

		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("X-Axis:", GUILayout.Width(50));
		    _editXAxis = EditorGUILayout.Toggle(_editXAxis, GUILayout.Width(30));
		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("Y-Axis:", GUILayout.Width(50));
		    _editYAxis = EditorGUILayout.Toggle(_editYAxis, GUILayout.Width(30));
		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("Z-Axis:", GUILayout.Width(50));
		    _editZAxis = EditorGUILayout.Toggle(_editZAxis, GUILayout.Width(30));
		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.EndHorizontal();
	    }

        /// <summary>
        /// Section for user input when Aligning Transforms.
        /// </summary>
        private void AlignInputSection()
	    {
		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("Align To:", GUILayout.Width(120));
		    _rootTransform = (Transform)EditorGUILayout.ObjectField(_rootTransform, typeof(Transform), true);
		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.Space();

		    if (GUILayout.Button("Align"))
            {
			    Align(SortUtility.SortTransformsSelectionBySiblingIndex(Selection.transforms));
            }
	    }

        /// <summary>
        /// Section for user input when Distributing Transforms.
        /// </summary>
        private void DistributeInputSection()
	    {
		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("First Transform:", GUILayout.Width(120));
		    _rootTransform = (Transform)EditorGUILayout.ObjectField(_rootTransform, typeof(Transform), true);
		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("Second Transform:", GUILayout.Width(120));
		    _distributeEndTransform = (Transform)EditorGUILayout.ObjectField(_distributeEndTransform, typeof(Transform), true);
		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.Space();

		    if (GUILayout.Button("Distribute"))
            {
			    Distribute(SortUtility.SortTransformsSelectionBySiblingIndex(Selection.transforms));
            }
	    }

        /// <summary>
        /// Section for user input when Gridding Transforms.
        /// </summary>
        private void GridInputSection()
	    {
		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.LabelField("Start Position:", GUILayout.Width(120));
		    _rootTransform = (Transform)EditorGUILayout.ObjectField(_rootTransform, typeof(Transform), true);
		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();

			if (_editXAxis)
		    {
				EditorGUILayout.BeginVertical();

			    EditorGUILayout.LabelField("X-Axis:", GUILayout.Width(55));

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Count:", GUILayout.Width(55));
				_gridCountX = EditorGUILayout.IntField(Mathf.Max(1, _gridCountX), GUILayout.Width(25));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Spacing:", GUILayout.Width(55));
				_gridSpacingX = EditorGUILayout.FloatField(Mathf.Max(0.0f, _gridSpacingX), GUILayout.Width(25));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();
		    }
			
			if (_editYAxis)
		    {
				EditorGUILayout.BeginVertical();

				EditorGUILayout.LabelField("Y-Axis:", GUILayout.Width(55));

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Count:", GUILayout.Width(55));
				_gridCountY = EditorGUILayout.IntField(Mathf.Max(1, _gridCountY), GUILayout.Width(25));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Spacing:", GUILayout.Width(55));
				_gridSpacingY = EditorGUILayout.FloatField(Mathf.Max(0.0f, _gridSpacingY), GUILayout.Width(25));
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.EndVertical();
			}

			if (_editZAxis)
		    {
			    EditorGUILayout.BeginVertical();

				EditorGUILayout.LabelField("Z-Axis:", GUILayout.Width(55));

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Count:", GUILayout.Width(55));
				_gridCountZ = EditorGUILayout.IntField(Mathf.Max(1, _gridCountZ), GUILayout.Width(25));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Spacing:", GUILayout.Width(55));
				_gridSpacingZ = EditorGUILayout.FloatField(Mathf.Max(0.0f, _gridSpacingZ), GUILayout.Width(25));
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.EndVertical();
		    }

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

		    if (GUILayout.Button("Grid"))
            {
			    Grid(SortUtility.SortTransformsSelectionBySiblingIndex(Selection.transforms));
            }
	    }

        /// <summary>
        /// Aligns the specified transforms to the given root Transform.
        /// </summary>
        private void Align(Transform[] transformsToAlign_)
		{
			if (null == _rootTransform)
			{
				Debug.LogWarning("No root transform selected for alignment.");
				return;
			}

			if (null == transformsToAlign_ || transformsToAlign_.Length == 0)
			{
				Debug.LogWarning("Received no transforms to align.");
				return;
			}

		    for (int counter = 0; counter < transformsToAlign_.Length; counter++)
		    {
			    Vector3 newPosition = transformsToAlign_[counter].position;

			    if (_editXAxis)
                {
				    newPosition.x = _rootTransform.position.x;
                }

                if (_editYAxis)
                {
				    newPosition.y = _rootTransform.position.y;
                }

			    if (_editZAxis)
                {
				    newPosition.z = _rootTransform.position.z;
                }

			    transformsToAlign_[counter].position = newPosition;
		    }
	    }

        /// <summary>
        /// Distributes the specified Transforms between the given start and end Transforms.
        /// </summary>
        private void Distribute(Transform[] transformsToDistribute_)
		{
			if (null == _rootTransform)
			{
				Debug.LogWarning("No root transform selected for distribution.");
				return;
			}

			if (null == transformsToDistribute_ || transformsToDistribute_.Length == 0)
			{
				Debug.LogWarning("Received no transforms to distribute.");
				return;
			}

			Vector3 directionBetweenTargetPositions = _distributeEndTransform.position - _rootTransform.position;

		    for (int counter = 0; counter < transformsToDistribute_.Length; counter++)
		    {
			    Vector3 newPosition = transformsToDistribute_[counter].position;

			    float distanceFraction = (float)counter / (float)(transformsToDistribute_.Length - 1);

			    if (_editXAxis)
                {
				    newPosition.x = _rootTransform.position.x + (directionBetweenTargetPositions.x * distanceFraction);
                }

			    if (_editYAxis)
                {
				    newPosition.y = _rootTransform.position.y + (directionBetweenTargetPositions.y * distanceFraction);
                }

			    if (_editZAxis)
                {
				    newPosition.z = _rootTransform.position.z + (directionBetweenTargetPositions.z * distanceFraction);
                }

			    transformsToDistribute_[counter].position = newPosition;
		    }
	    }

        /// <summary>
        /// Grids the specified Transforms with the given spacing for each axis.
        /// </summary>
        private void Grid(Transform[] transformsToGrid_)
		{
			if (null == _rootTransform)
			{
				Debug.LogWarning("No root transform selected for gridding.");
				return;
			}

			if (null == transformsToGrid_ || transformsToGrid_.Length == 0)
			{
				Debug.LogWarning("Received no transforms to grid.");
				return;
			}

			for (int counter = 0; counter < transformsToGrid_.Length; counter++)
		    {
			    Vector3 newPosition = _rootTransform.position;

			    if (_editXAxis)
			    {
				    newPosition.x += (_gridSpacingX * (counter % _gridCountX));
				}

				if (_editYAxis)
				{
					newPosition.y += (_gridSpacingY * ((counter / _gridCountX) % _gridCountY));
				}

				if (_editZAxis)
				{
					newPosition.z += (_gridSpacingZ * ((counter / (_gridCountX * _gridCountY)) % _gridCountZ));
				}

			    transformsToGrid_[counter].position = newPosition;
		    }
	    }

        /// <summary>
        /// Formats the EditorWindow.
        /// </summary>
        private void WindowFormatting()
	    {
		    _wrappedTextStyle = new GUIStyle();
		    _wrappedTextStyle.wordWrap = true;
		    _wrappedTextStyle.padding = new RectOffset(10, 10, 0, 0);

		    if (_instructionsDroppedDown)
            {
		        // Additional size for shortcut instructions foldout.
			    _thisWindow.minSize = new Vector2(275, 150);
            }
		    else
            {
		        // Base size of the EditorWindow.
			    _thisWindow.minSize = new Vector2(275, 110);
            }
	    }

#endregion
    }
}
