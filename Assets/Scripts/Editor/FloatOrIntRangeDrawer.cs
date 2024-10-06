using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FloatRange))][CustomPropertyDrawer(typeof(IntRange))]
public class FloatOrIntRangeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		int originalIndentLevel = EditorGUI.indentLevel;
		float originalLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUI.BeginProperty(_position, _label, _property);

		_position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
		_position.width = _position.width / 2f;
		EditorGUIUtility.labelWidth = _position.width / 2f;
		EditorGUI.indentLevel = 1;
		EditorGUI.PropertyField(_position, _property.FindPropertyRelative("min"));
		_position.x += _position.width;
		EditorGUI.PropertyField(_position, _property.FindPropertyRelative("max"));

		EditorGUI.EndProperty();
		EditorGUI.indentLevel = originalIndentLevel;
		EditorGUIUtility.labelWidth = originalLabelWidth;
	}
}