using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))]
public class FloatRangeSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        int originalIndentLevel = EditorGUI.indentLevel;
        EditorGUI.BeginProperty(_position, _label, _property);

        _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
        EditorGUI.indentLevel = 0;
        SerializedProperty minProperty = _property.FindPropertyRelative("min");
        SerializedProperty maxProperty = _property.FindPropertyRelative("max");
        float minValue = minProperty.floatValue;
        float maxValue = maxProperty.floatValue;
        float fieldWidth = _position.width / 4f - 4f;
        float sliderWidth = _position.width / 2f;
        _position.width = fieldWidth;
        minValue = EditorGUI.FloatField(_position, minValue);
        _position.x += fieldWidth + 4f;
        _position.width = sliderWidth;
        var limit = attribute as FloatRangeSliderAttribute;
        EditorGUI.MinMaxSlider(_position, ref minValue, ref maxValue, limit.Min, limit.Max);
        _position.x += sliderWidth + 4f;
        _position.width = fieldWidth;
        maxValue = EditorGUI.FloatField(_position, maxValue);
        if (minValue < limit.Min)
        {
            minValue = limit.Min;
        }
        else if (minValue > limit.Max)
        {
            minValue = limit.Max;
        }

        if (maxValue < minValue)
        {
            maxValue = minValue;
        }
        else if (maxValue > limit.Max)
        {
            maxValue = limit.Max;
        }

        minProperty.floatValue = minValue;
        maxProperty.floatValue = maxValue;

        EditorGUI.EndProperty();
        EditorGUI.indentLevel = originalIndentLevel;
    }
}