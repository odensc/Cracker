using UnityEngine;
using UnityEditor;

public class EditorUtils
{
	public static bool GetFoldoutLayout(SerializedProperty prop, string content, GUIStyle style = null)
	{
		style = style ?? new GUIStyle(EditorStyles.foldout);
		style.fontStyle = FontStyle.Bold;
		return prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, content, style);
	}

	public static void DrawArrow(Vector3 position, Vector3 direction, float size = 1, Color? color = null)
	{
		if (color == null)
			color = GetAxisColor(direction);

		Handles.color = Handles.xAxisColor;
		Handles.ArrowCap(0, position, Quaternion.LookRotation(direction), size);
	}

	public static Color GetAxisColor(Vector3 direction)
	{
		if (direction.x < 0 || direction.x > 0)
			return Handles.xAxisColor;
		else if (direction.y < 0 || direction.y > 0)
			return Handles.yAxisColor;
		else if (direction.z < 0 || direction.z > 0)
			return Handles.zAxisColor;
		else
			return Handles.xAxisColor;
	}
}
