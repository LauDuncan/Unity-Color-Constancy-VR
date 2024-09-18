using System.Collections.Generic;
using UnityEngine;

public class CoroutineState
{
    public float StepSize { get; set; }

    // (lower bound of range (ref color), upper bound of range (prevInput + 0.5 range))
    public (Color refColor, Color testColorBound) Range { get; set; } 
    public string staircaseType { get; set; }

    public CoroutineState(float stepSize, (Color, Color) range, string staircase)
    {
        StepSize = stepSize;
        Range = range;
        staircaseType = staircase;
    }
}
