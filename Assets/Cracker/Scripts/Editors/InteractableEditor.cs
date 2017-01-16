using UnityEngine;
using UnityEditor;

namespace Cracker
{
	[CustomPropertyDrawer(typeof(GrabMechanic))]
	public class GrabMechanicDrawer : PropertyDrawer
	{
		private float currentY = 0;
		private const float propWidth = 230;
		private const float propHeight = 17;

		private int getAmountProps(SerializedProperty prop)
		{
			switch ((GrabMechanic.GrabMechanicType) prop.FindPropertyRelative("type").enumValueIndex)
			{
				case GrabMechanic.GrabMechanicType.ParentTransform:
					return 1;

				case GrabMechanic.GrabMechanicType.FixedJoint:
					return 2;

				case GrabMechanic.GrabMechanicType.SpringJoint:
					return 4;

				default:
					return 0;
			}
		}

		private Rect getNextRect(Rect pos, Rect current)
		{
			return new Rect(pos.x, currentY += propHeight, propWidth, pos.height);
		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			if (prop.isExpanded)
				// add 1 to amount of props, to account for label
				return base.GetPropertyHeight(prop, label) + (propHeight * (1 + getAmountProps(prop)));
			else
				// not folded, so just return one prop ("Grab Mechanic")
				return base.GetPropertyHeight(prop, label);
		}

		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
		{
			var grabType = (GrabMechanic.GrabMechanicType) prop.FindPropertyRelative("type").enumValueIndex;
			EditorGUI.BeginProperty(pos, label, prop);
			pos.width = propWidth;
			pos.height = propHeight;

			prop.isExpanded = EditorGUI.Foldout(pos, prop.isExpanded, label, true);

			if (prop.isExpanded)
			{
				currentY = pos.y;
				var rect = new Rect();
				EditorGUI.indentLevel++;

				rect = getNextRect(pos, rect);
				EditorGUI.PropertyField(rect, prop.FindPropertyRelative("type"));

				if (grabType == GrabMechanic.GrabMechanicType.ParentTransform)
				{
					rect = getNextRect(pos, rect);
					EditorGUI.PropertyField(rect, prop.FindPropertyRelative("restoreParent"));
				}

				if (grabType == GrabMechanic.GrabMechanicType.FixedJoint
					|| grabType == GrabMechanic.GrabMechanicType.SpringJoint)
				{
					rect = getNextRect(pos, rect);
					EditorGUI.PropertyField(rect, prop.FindPropertyRelative("breakForce"));
					rect = getNextRect(pos, rect);
					EditorGUI.PropertyField(rect, prop.FindPropertyRelative("toggleKinematic"));
				}

				if (grabType == GrabMechanic.GrabMechanicType.SpringJoint)
				{
					rect = getNextRect(pos, rect);
					EditorGUI.PropertyField(rect, prop.FindPropertyRelative("springJointStrength"));
					rect = getNextRect(pos, rect);
					EditorGUI.PropertyField(rect, prop.FindPropertyRelative("springJointDamper"));
				}

				EditorGUI.indentLevel--;
			}

			EditorGUI.EndProperty();
		}
	}

	[CustomEditor(typeof(Interactable))]
	[CanEditMultipleObjects]
	public class InteractableEditor : Editor
	{
		private SerializedProperty isGrabbable;
		private SerializedProperty highlightOnSelect;
		private SerializedProperty grabMechanic;
		private SerializedProperty applyVelocity;
		private SerializedProperty velocityMultiplier;
		private SerializedProperty updateTransform;
		private SerializedProperty attachPoint;
		private SerializedProperty pauseCollisions;

		void OnEnable()
		{
			isGrabbable = serializedObject.FindProperty("isGrabbable");
			highlightOnSelect = serializedObject.FindProperty("highlightOnSelect");
			grabMechanic = serializedObject.FindProperty("grabMechanic");
			applyVelocity = serializedObject.FindProperty("applyVelocity");
			velocityMultiplier = serializedObject.FindProperty("velocityMultiplier");
			updateTransform = serializedObject.FindProperty("updateTransform");
			attachPoint = serializedObject.FindProperty("attachPoint");
			pauseCollisions = serializedObject.FindProperty("pauseCollisions");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(isGrabbable);
			EditorGUILayout.PropertyField(highlightOnSelect);

			if (isGrabbable.boolValue)
			{
				EditorGUILayout.Space();
				var grabbableFoldout = EditorUtils.GetFoldoutLayout(isGrabbable, "Grabbable Options");

				if (grabbableFoldout)
				{
					EditorGUILayout.PropertyField(grabMechanic);
					EditorGUILayout.PropertyField(applyVelocity);

					if (applyVelocity.boolValue)
					{
						EditorGUI.indentLevel++;
						EditorGUILayout.PropertyField(velocityMultiplier);
						EditorGUI.indentLevel--;
					}

					EditorGUILayout.PropertyField(updateTransform);

					if (updateTransform.boolValue)
					{
						EditorGUI.indentLevel++;
						EditorGUILayout.PropertyField(attachPoint);
						EditorGUI.indentLevel--;
					}
				}
			}

			EditorGUILayout.Space();
			var globalFoldout = EditorUtils.GetFoldoutLayout(pauseCollisions, "Global Options");
			if (globalFoldout)
			{
				EditorGUILayout.PropertyField(pauseCollisions);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
