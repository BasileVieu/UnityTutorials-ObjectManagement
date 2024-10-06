using UnityEngine;

public class FloatRangeSliderAttribute : PropertyAttribute
{
    public float Min { get; private set; }

    public float Max { get; private set; }

    public FloatRangeSliderAttribute(float _min, float _max)
    {
        if (_max < _min)
        {
            _max = _min;
        }

        Min = _min;
        Max = _max;
    }
}