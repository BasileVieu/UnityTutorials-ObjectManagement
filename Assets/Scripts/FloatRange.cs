using UnityEngine;

[System.Serializable]
public struct FloatRange
{
    public float min, max;

    public float RandomValueInRange => Random.Range(min, max);
}