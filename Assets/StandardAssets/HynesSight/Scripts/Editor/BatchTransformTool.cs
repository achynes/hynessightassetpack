using HynesSight;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HynesSightEditor
{
    public class BatchTransformTool : EditorWindow
	{
		enum ArithmeticOperation
		{
			Add,
			Subtract,
			Multiply,
			Divide
		}

		enum TransformPart
		{
			Position,
			Rotation,
			Scale
		}

		static BatchTransformTool _activeWindow;
		
		Vector2 _scrollViewCurrent = Vector2.zero;

		bool _alignExpanded = false;
		bool _distributeExpanded = false;
		bool _gridExpanded = false;
		bool _lookAtExpanded = false;
		bool _snapToExpanded = false;
		bool _modifyTransformsExpanded = false;

		// START Look At
		Transform _lookAtTarget = null;

		bool _lookAtX = false;
		bool _lookAtY = true;
		bool _lookAtZ = false;

		// START Align
		Transform _alignTarget = null;

		bool _alignX = true;
		bool _alignY = true;
		bool _alignZ = true;

		// START Distribute
		Transform _distributeStartTransform = null;
		Transform _distributeEndTransform = null;

		bool _distributePosition = true;
		bool _distributePositionX = true;
		bool _distributePositionY = true;
		bool _distributePositionZ = true;

		bool _distributeRotation = false;
		bool _distributeRotationX = true;
		bool _distributeRotationY = true;
		bool _distributeRotationZ = true;

		bool _distributeScale = false;
		bool _distributeScaleX = true;
		bool _distributeScaleY = true;
		bool _distributeScaleZ = true;

		// START Grid
		Transform _gridStartTransform = null;
		Transform _gridEndTransform = null;

		int _gridCountX = 0;
		int _gridCountY = 0;
		int _gridCountZ = 0;

		// START Snap To
		Transform _transformToSnap = null;
		Transform _snapAnchorTransform = null;

		bool _snapPosition = true;
		bool _snapPositionX = true;
		bool _snapPositionY = true;
		bool _snapPositionZ = true;

		bool _snapRotation = true;
		bool _snapRotationX = true;
		bool _snapRotationY = true;
		bool _snapRotationZ = true;

		// START Modify
		ArithmeticOperation _modifyTransformsOperation = ArithmeticOperation.Multiply;
		TransformPart _modifyTransformsPart = TransformPart.Position;

		Vector3 _modifyTransformsAmount = Vector3.one;
		bool _modifyTransformsAmountTweaked = false;

        [MenuItem("Tools/Batch Transform Tool")]
        public static void OpenWindow()
        {
            System.Reflection.Assembly assembly = typeof(Editor).Assembly;
            System.Type inspectorWindow = assembly.GetType("UnityEditor.InspectorWindow");

            _activeWindow = EditorWindow.GetWindow<BatchTransformTool>("Transform", true, inspectorWindow);
        }

        private void OnGUI()
        {
			_scrollViewCurrent = EditorGUILayout.BeginScrollView(_scrollViewCurrent, GUILayout.ExpandWidth(true));

			EditorGUILayout.Space();

			GUISection_Align();

			EditorGUILayout.Space();

			GUISection_Distribute();

			EditorGUILayout.Space();

			GUISection_Grid();

			EditorGUILayout.Space();

			GUISection_LookAt();

			EditorGUILayout.Space();

			GUISection_SnapTo();

			EditorGUILayout.Space();

			GUISection_Modify();
		}

		void GUISection_Align()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_alignExpanded = EditorGUILayout.Foldout(_alignExpanded, "Align", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_alignExpanded)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Align to:", GUILayout.Width(120));
				_alignTarget = (Transform)EditorGUILayout.ObjectField(_alignTarget, typeof(Transform), true);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

				EditorGUILayoutHelpers.RowOfToggles("X", ref _alignX, "Y", ref _alignY, "Z", ref _alignZ);
				EditorGUILayout.Space();
				
				if (GUILayout.Button("Align"))
					Align();
			}
		}

		void GUISection_Distribute()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_distributeExpanded = EditorGUILayout.Foldout(_distributeExpanded, "Distribute", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_distributeExpanded)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Start Transform:", GUILayout.Width(120));
				_distributeStartTransform = (Transform)EditorGUILayout.ObjectField(_distributeStartTransform, typeof(Transform), true);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("End Transform:", GUILayout.Width(120));
				_distributeEndTransform = (Transform)EditorGUILayout.ObjectField(_distributeEndTransform, typeof(Transform), true);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginVertical(EditorStyles.helpBox);

				float originalLabelWidth = EditorGUIUtility.labelWidth;

				GUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 60f;
				_distributePosition = EditorGUILayout.Toggle("Position:", _distributePosition);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				if (_distributePosition)
				{
					EditorGUIUtility.labelWidth = 15f;
					EditorGUILayoutHelpers.RowOfToggles("X", ref _distributePositionX, "Y", ref _distributePositionY, "Z", ref _distributePositionZ);
				}

				GUILayout.EndVertical();

				GUILayout.BeginVertical(EditorStyles.helpBox);

				GUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 60f;
				_distributeRotation = EditorGUILayout.Toggle("Rotation:", _distributeRotation);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				if (_distributeRotation)
				{
					EditorGUIUtility.labelWidth = 15f;
					EditorGUILayoutHelpers.RowOfToggles("X", ref _distributeRotationX, "Y", ref _distributeRotationY, "Z", ref _distributeRotationZ);
				}

				GUILayout.EndVertical();

				GUILayout.BeginVertical(EditorStyles.helpBox);

				GUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 60f;
				_distributeScale = EditorGUILayout.Toggle("Scale:", _distributeScale);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				if (_distributeScale)
				{
					EditorGUIUtility.labelWidth = 15f;
					EditorGUILayoutHelpers.RowOfToggles("X", ref _distributeScaleX, "Y", ref _distributeScaleY, "Z", ref _distributeScaleZ);
				}

				GUILayout.EndVertical();

				EditorGUILayout.Space();

				if (GUILayout.Button("Distribute"))
					Distribute();
			}
		}

		void GUISection_Grid()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_gridExpanded = EditorGUILayout.Foldout(_gridExpanded, "Grid", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_gridExpanded)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Start Transform:", GUILayout.Width(120));
				_gridStartTransform = (Transform)EditorGUILayout.ObjectField(_gridStartTransform, typeof(Transform), true);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("End Transform:", GUILayout.Width(120));
				_gridEndTransform = (Transform)EditorGUILayout.ObjectField(_gridEndTransform, typeof(Transform), true);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

				EditorGUILayoutHelpers.RowOfIntFields(delayed: false, "X Count", ref _gridCountX, "Y Count", ref _gridCountY, "Z Count", ref _gridCountZ);

				EditorGUILayout.Space();

				if (GUILayout.Button("Grid"))
					Grid();
			}
		}

		void GUISection_LookAt()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_lookAtExpanded = EditorGUILayout.Foldout(_lookAtExpanded, "Look at", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_lookAtExpanded)
			{
				_lookAtTarget = (Transform)EditorGUILayout.ObjectField("Target:", _lookAtTarget, typeof(Transform), allowSceneObjects: true);

				float originalLabelWidth = EditorGUIUtility.labelWidth;

				GUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 15f;
				_lookAtX = EditorGUILayout.Toggle("X:", _lookAtX);
				_lookAtY = EditorGUILayout.Toggle("Y:", _lookAtY);
				_lookAtZ = EditorGUILayout.Toggle("Z:", _lookAtZ);
				EditorGUIUtility.labelWidth = originalLabelWidth;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				bool lookAtPressed = GUILayout.Button(new GUIContent("Look at",
					"All selected GameObjects will look at the selected 'Target' along the selected axes."));

				if (lookAtPressed && _lookAtTarget != null)
				{
					Undo.RecordObjects(Selection.transforms, string.Format("make selected Transforms look at {0}.", _lookAtTarget.name));

					foreach (Transform selectedTransform in Selection.transforms)
					{
						Vector3 newRotation = Quaternion.LookRotation(_lookAtTarget.position - selectedTransform.position).eulerAngles;

						if (!_lookAtX)
							newRotation.x = selectedTransform.eulerAngles.x;
						if (!_lookAtY)
							newRotation.y = selectedTransform.eulerAngles.y;
						if (!_lookAtZ)
							newRotation.z = selectedTransform.eulerAngles.z;

						selectedTransform.eulerAngles = newRotation;
					}
				}
			}

			GUILayout.EndVertical();
		}

		void GUISection_SnapTo()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_snapToExpanded = EditorGUILayout.Foldout(_snapToExpanded, "Snap to", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_snapToExpanded)
			{
				_transformToSnap = (Transform)EditorGUILayout.ObjectField("Snapped Object:", _transformToSnap, typeof(Transform), allowSceneObjects: true);
				_snapAnchorTransform = (Transform)EditorGUILayout.ObjectField("Anchor Object:", _snapAnchorTransform, typeof(Transform), allowSceneObjects: true);

				GUILayout.BeginVertical(EditorStyles.helpBox);

				float originalLabelWidth = EditorGUIUtility.labelWidth;

				GUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 60f;
				_snapPosition = EditorGUILayout.Toggle("Position:", _snapPosition);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				if (_snapPosition)
				{
					GUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 15f;
					_snapPositionX = EditorGUILayout.Toggle("X:", _snapPositionX);
					_snapPositionY = EditorGUILayout.Toggle("Y:", _snapPositionY);
					_snapPositionZ = EditorGUILayout.Toggle("Z:", _snapPositionZ);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}

				GUILayout.EndVertical();

				GUILayout.BeginVertical(EditorStyles.helpBox);

				GUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 60f;
				_snapRotation = EditorGUILayout.Toggle("Rotation:", _snapRotation);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				if (_snapRotation)
				{
					GUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 15f;
					_snapRotationX = EditorGUILayout.Toggle("X:", _snapRotationX);
					_snapRotationY = EditorGUILayout.Toggle("Y:", _snapRotationY);
					_snapRotationZ = EditorGUILayout.Toggle("Z:", _snapRotationZ);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}

				EditorGUIUtility.labelWidth = originalLabelWidth;

				GUILayout.EndVertical();

				bool snapPressed = GUILayout.Button(new GUIContent("Snap",
					"The specified 'Snapped Object' will snap to the position/rotation of the specified 'Anchor Object'. All selected GameObjects will follow relative to the change."));

				if (snapPressed && _transformToSnap != null && _snapAnchorTransform != null)
				{
					Vector3 positionDelta = _snapAnchorTransform.position - _transformToSnap.position;
					Vector3 rotationDelta = _snapAnchorTransform.eulerAngles - _transformToSnap.eulerAngles;

					if (!_snapPositionX)
						positionDelta.x = 0f;
					if (!_snapPositionY)
						positionDelta.y = 0f;
					if (!_snapPositionZ)
						positionDelta.z = 0f;

					if (!_snapRotationX)
						rotationDelta.x = 0f;
					if (!_snapRotationY)
						rotationDelta.y = 0f;
					if (!_snapRotationZ)
						rotationDelta.z = 0f;

					List<Transform> allTransformsToSnap = new List<Transform>(Selection.transforms);
					allTransformsToSnap.AddUnique(_transformToSnap);

					allTransformsToSnap.Remove(_snapAnchorTransform);

					List<Transform> extraneousTransforms = new List<Transform>(allTransformsToSnap.Count);

					foreach (Transform oneTransform in allTransformsToSnap)
					{
						foreach (Transform otherTransform in allTransformsToSnap)
						{
							if (oneTransform != otherTransform && oneTransform.IsChildOf(otherTransform))
								extraneousTransforms.Add(oneTransform);
						}
					}

					// We trim off the Child leaves of the selected Transform tree to avoid any given Transform being moved more than once (via multiple parents being snapped).
					foreach (Transform extraneousTransform in extraneousTransforms)
						allTransformsToSnap.Remove(extraneousTransform);

					Undo.RecordObjects(allTransformsToSnap.ToArray(), string.Format("snap {0} to {1}, and bring selected transforms.", _transformToSnap.name, _snapAnchorTransform.name));

					foreach (Transform transformToSnap in allTransformsToSnap)
					{
						if (_snapPosition)
							transformToSnap.position += positionDelta;

						if (_snapRotation)
							transformToSnap.eulerAngles += rotationDelta;
					}
				}
			}

			GUILayout.EndVertical();
		}

		void GUISection_Modify()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);

			_modifyTransformsExpanded = EditorGUILayout.Foldout(_modifyTransformsExpanded, "Modify", toggleOnLabelClick: true, EditorStyles.foldoutHeader);

			if (_modifyTransformsExpanded)
			{
				ArithmeticOperation previousOperation = _modifyTransformsOperation;
				_modifyTransformsOperation = (ArithmeticOperation)EditorGUILayout.EnumPopup("Op:", _modifyTransformsOperation);

				bool zeroIdentity = _modifyTransformsOperation == ArithmeticOperation.Add || _modifyTransformsOperation == ArithmeticOperation.Subtract;

				// If the user hasn't already stored off a new value, set a good identity value for them when selecting an operation.
				if (_modifyTransformsOperation != previousOperation && !_modifyTransformsAmountTweaked)
				{
					_modifyTransformsAmount = zeroIdentity ? Vector3.zero : Vector3.one;
				}

				_modifyTransformsPart = (TransformPart)EditorGUILayout.EnumPopup("Target:", _modifyTransformsPart);

				Vector3 previousModifyAmount = _modifyTransformsAmount;
				_modifyTransformsAmount = EditorGUILayout.Vector3Field("Amount:", _modifyTransformsAmount);

				if (_modifyTransformsAmount != previousModifyAmount)
					_modifyTransformsAmountTweaked = true;

				bool modifyTransformsPressed = GUILayout.Button(new GUIContent("Modify",
					"All selected GameObjects will look have the selected part of their Transform modified using the selected operation by the input Vector."));

				if (modifyTransformsPressed)
				{
					string lastVerb = zeroIdentity ? "to" : "by";
					Undo.RecordObjects(Selection.transforms, string.Format("{0} {1} of selected Transforms {2} {3}.", _modifyTransformsOperation, _modifyTransformsPart, lastVerb, _modifyTransformsAmount));

					System.Func<Vector3, Vector3, Vector3> operationFunctor = (_vectorA, _vectorB) =>
					{
						switch (_modifyTransformsOperation)
						{
							case ArithmeticOperation.Add:
								return _vectorA + _vectorB;
							case ArithmeticOperation.Subtract:
								return _vectorA - _vectorB;
							case ArithmeticOperation.Multiply:
								return Vector3.Scale(_vectorA, _vectorB);
							case ArithmeticOperation.Divide:
								{
									Vector3 _invertedDivisor = Vector3.one;
									if (_vectorB.x != 0f)
										_invertedDivisor.x = 1f / _invertedDivisor.x;
									else
										Debug.LogErrorFormat("X component of Amount is 0 when trying to divide.");
									if (_vectorB.y != 0f)
										_invertedDivisor.y = 1f / _invertedDivisor.y;
									else
										Debug.LogErrorFormat("Y component of Amount is 0 when trying to divide.");
									if (_vectorB.z != 0f)
										_invertedDivisor.z = 1f / _invertedDivisor.z;
									else
										Debug.LogErrorFormat("Z component of Amount is 0 when trying to divide.");

									return Vector3.Scale(_vectorA, _invertedDivisor);
								}
							default:
								{
									Debug.LogErrorFormat("{0} is not handled, code should be extended to include it.", _modifyTransformsOperation);
									return Vector3.zero;
								}
						}
					};

					foreach (Transform selectedTransform in Selection.transforms)
					{
						switch (_modifyTransformsPart)
						{
							case TransformPart.Position:
								{
									selectedTransform.localPosition = operationFunctor(selectedTransform.localPosition, _modifyTransformsAmount);
									break;
								}
							case TransformPart.Rotation:
								{
									selectedTransform.localEulerAngles = operationFunctor(selectedTransform.localEulerAngles, _modifyTransformsAmount);
									break;
								}
							case TransformPart.Scale:
								{
									selectedTransform.localScale = operationFunctor(selectedTransform.localScale, _modifyTransformsAmount);
									break;
								}
						}
					}
				}
			}

			GUILayout.EndVertical();
		}

		void Align()
		{
			if (null == _alignTarget)
			{
				Debug.LogWarning("No target Transform set for alignment.");
				return;
			}

			Transform[] transformsToAlign = Selection.transforms;

			if (null == transformsToAlign || transformsToAlign.Length == 0)
			{
				Debug.LogWarning("No Transforms selected to align.");
				return;
			}

			foreach (Transform transformToAlign in transformsToAlign)
			{
				Vector3 newPosition = transformToAlign.position;

				if (_alignX)
					newPosition.x = _alignTarget.position.x;

				if (_alignY)
					newPosition.y = _alignTarget.position.y;

				if (_alignZ)
					newPosition.z = _alignTarget.position.z;

				transformToAlign.position = newPosition;
			}
		}

		void Distribute() {
			if (null == _distributeStartTransform)
			{
				Debug.LogWarning("No Start Transform specified for distribution.");
				return;
			}

			if (null == _distributeEndTransform)
			{
				Debug.LogWarning("No End Transform specified for distribution.");
				return;
			}

			Transform[] transformsToDistribute = Selection.transforms;
			transformsToDistribute.SortBySiblingIndex();

			if (null == transformsToDistribute || transformsToDistribute.Length == 0)
			{
				Debug.LogWarning("Received no transforms to distribute.");
				return;
			}

			Vector3 positionStartEndDelta = _distributeEndTransform.position - _distributeStartTransform.position;
			Vector3 rotationStartEndDelta = _distributeEndTransform.eulerAngles - _distributeStartTransform.eulerAngles;
			Vector3 scaleStartEndDelta = _distributeEndTransform.localScale - _distributeStartTransform.localScale;

			float distributionMax = (float)(transformsToDistribute.Length - 1);

			for (int i = 0; i < transformsToDistribute.Length; i++)
			{
				Transform transformToDistribute = transformsToDistribute[i];
				float distributionFactor = (float)i / distributionMax;

				if (_distributePosition)
				{
					Vector3 newPosition = transformToDistribute.position;

					if (_distributePositionX)
						newPosition.x = _distributeStartTransform.position.x + (positionStartEndDelta.x * distributionFactor);

					if (_distributePositionY)
						newPosition.y = _distributeStartTransform.position.y + (positionStartEndDelta.y * distributionFactor);

					if (_distributePositionZ)
						newPosition.z = _distributeStartTransform.position.z + (positionStartEndDelta.z * distributionFactor);

					transformToDistribute.position = newPosition;
				}

				if (_distributeRotation)
				{
					Vector3 newEulerAngles = transformToDistribute.eulerAngles;

					if (_distributeRotationX)
						newEulerAngles.x = _distributeStartTransform.eulerAngles.x + (rotationStartEndDelta.x * distributionFactor);

					if (_distributeRotationY)
						newEulerAngles.y = _distributeStartTransform.eulerAngles.y + (rotationStartEndDelta.y * distributionFactor);

					if (_distributeRotationZ)
						newEulerAngles.z = _distributeStartTransform.eulerAngles.z + (rotationStartEndDelta.z * distributionFactor);

					transformToDistribute.eulerAngles = newEulerAngles;
				}

				if (_distributeScale)
				{
					Vector3 newScale = transformToDistribute.localScale;

					if (_distributeScaleX)
						newScale.x = _distributeStartTransform.localScale.x + (scaleStartEndDelta.x * distributionFactor);

					if (_distributeScaleY)
						newScale.y = _distributeStartTransform.localScale.y + (scaleStartEndDelta.y * distributionFactor);

					if (_distributeScaleZ)
						newScale.z = _distributeStartTransform.localScale.z + (scaleStartEndDelta.z * distributionFactor);

					transformToDistribute.localScale = newScale;
				}
			}
		}

		void Grid()
		{
			if (null == _gridStartTransform)
			{
				Debug.LogWarning("No Start Transform specified for gridding.");
				return;
			}

			if (null == _gridEndTransform)
			{
				Debug.LogWarning("No End Transform specified for gridding.");
				return;
			}

			Transform[] transformsToGrid = Selection.transforms;
			transformsToGrid.SortBySiblingIndex();

			if (null == transformsToGrid || transformsToGrid.Length == 0)
			{
				Debug.LogWarning("No Transforms were selected for gridding.");
				return;
			}

			Vector3 _gridSpacing = (_gridEndTransform.position - _gridStartTransform.position).DivideElementWise(new Vector3(_gridCountX, _gridCountY, _gridCountZ));

			for (int i = 0; i < transformsToGrid.Length; i++)
			{
				Vector3 newPosition = _gridStartTransform.position;

				if (_gridCountX > 0)
					newPosition.x += (_gridSpacing.x * (i % _gridCountX));

				if (_gridCountY > 0)
					newPosition.y += (_gridSpacing.y * ((i / _gridCountX) % _gridCountY));

				if (_gridCountZ > 0)
					newPosition.z += (_gridSpacing.z * ((i / (_gridCountX * _gridCountY)) % _gridCountZ));

				transformsToGrid[i].position = newPosition;
			}
		}
    }
}
