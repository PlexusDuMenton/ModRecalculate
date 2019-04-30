using UnityEngine;

public class Utils
{
    static public float exponentialFormula(float baseValue, float gain, float exp, float level, float other = 0)
    {
        return baseValue + gain * Mathf.Pow((level - 1), exp) + other;
    }
}