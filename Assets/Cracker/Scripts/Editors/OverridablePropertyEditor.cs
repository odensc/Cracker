using UnityEngine;
using UnityEditor;

namespace Cracker
{
	[CustomPropertyDrawer(typeof(OverridableProperty<>))]
	public class OverridablePropertyDrawer : PropertyDrawer
	{
		private float currentY = 0;
		private const float propWidth = 230;
		private const float propHeight = 17;

		private Rect getNextRect(Rect pos, Rect current)
		{
			return new Rect(pos.x, currentY += propHeight, propWidth, pos.height);
		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			var option = (OverridableProperty<bool>.Option) prop.FindPropertyRelative("option").enumValueIndex;
			if (option == OverridableProperty<bool>.Option.Override)
				return base.GetPropertyHeight(prop, label) + propHeight;
			else
				// not overridden, so just return one prop ("Grab Mechanic")
				return base.GetPropertyHeight(prop, label);
		}

		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
		{
			var option = (OverridableProperty<bool>.Option) prop.FindPropertyRelative("option").enumValueIndex;
			EditorGUI.BeginProperty(pos, label, prop);
			pos.width = propWidth;
			pos.height = propHeight;

			EditorGUI.PropertyField(pos, prop.FindPropertyRelative("option"), label);

			if (option == OverridableProperty<bool>.Option.Override)
			{
				currentY = pos.y;
				var rect = new Rect();
				EditorGUI.indentLevel++;

				rect = getNextRect(pos, rect);
				//EditorGUI.PropertyField(rect, prop.FindPropertyRelative("value"));

				EditorGUI.indentLevel--;
			}

			EditorGUI.EndProperty();
		}
	}
}