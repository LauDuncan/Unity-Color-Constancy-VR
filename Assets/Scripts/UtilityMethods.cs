using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public static class UtilityMethods
{

    // Calculates the Euclidean Distance between two Colors
    public static float calcEuclideanDistanceOfTwoColors(Color color1, Color color2)
    {
        Color.RGBToHSV(color1, out float h1, out float s1, out float v1);
        Color.RGBToHSV(color2, out float h2, out float s2, out float v2);

        return Mathf.Sqrt(Mathf.Pow(h1 - h2, 2) + Mathf.Pow(s1 - s2, 2) + Mathf.Pow(v1 - v2, 2));
    }

    // Calculates a new color value given a color and a hue offset value
    public static Color getNewColorWithOffset(Color color, float offset)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        h += offset;
        if (h > 1) h -= 1.0f;
        if (h < 0) h += 1.0f;
        return Color.HSVToRGB(h, s, v);
    }

    public static (Color thresholdUB, Color thresholdLB, float constancyThreshold) CalculateThresholdForLBUB(List<Color> matchPointsUB, List<Color> matchPointsLB)
    {
        if (matchPointsUB.Count >= 3 && matchPointsLB.Count >= 3)
        {
            Color thresholdUB = Mean(matchPointsUB.GetRange(matchPointsUB.Count - 3, 3));
            Color thresholdLB = Mean(matchPointsLB.GetRange(matchPointsLB.Count - 3, 3));
            float constancyThreshold = calcEuclideanDistanceOfTwoColors(thresholdUB, thresholdLB);

            Debug.Log($"UB Threshold: {thresholdUB}, LB Threshold: {thresholdLB}, Color Constancy Threshold: {constancyThreshold}");

            return (thresholdUB, thresholdLB, constancyThreshold);
        }
        else
        {
            Debug.LogWarning($"Not enough data points to calculate threshold.");
            return (Color.white, Color.white, 0f);
        }
    }

    public static Color CalculateThresholdSingleBound(List<Color> matchPoints)
    {
        return Mean(matchPoints.GetRange(matchPoints.Count - 3, 3));
    }

    public static (float hue, float saturation, float value) serializeColorToHSVTuple(Color inputColor)
    {
        Color.RGBToHSV(inputColor, out float h, out float s, out float v);
        return (h,s,v);
    }

    public static float serializeColorToHueValue(Color inputColor)
    {
        Color.RGBToHSV(inputColor, out float hue, out _, out _);
        return hue;
    }

    private static Color Mean(List<Color> colors)
    {
        float h = 0, s = 0, v = 0;
        foreach (var color in colors)
        {
            Color.RGBToHSV(color, out float hue, out float sat, out float val);
            h += hue;
            s += sat;
            v += val;
        }
        h /= colors.Count;
        s /= colors.Count;
        v /= colors.Count;

        return Color.HSVToRGB(h, s, v);
    }
}
