using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public Renderer refObjectRenderer;
    public Renderer testObjectRenderer;

    // Methods for setting colors
    public void SetRefObjectColor(Color color)
    {
        if (refObjectRenderer != null)
        {
            refObjectRenderer.material.color = color;
        }
    }

    public void SetTestObjectColor(Color color)
    {
        if (testObjectRenderer != null)
        {
            testObjectRenderer.material.color = color;
        }
    }

    // Methods for getting colors
    public Color GetRefObjectColor()
    {
        if (refObjectRenderer != null)
        {
            return refObjectRenderer.material.color;
        }
        return Color.white;
    }

    public Color GetTestObjectColor()
    {
        if (testObjectRenderer != null)
        {
            return testObjectRenderer.material.color;
        }
        return Color.white;
    }

    // Adjust the hue of the test object
    public void AdjustTestObjectHue(float hueOffset)
    {
        AdjustHue(testObjectRenderer, hueOffset);
    }

    // Adjust the saturation of the test object
    public void AdjustTestObjectSaturation(float saturationIncrement)
    {
        AdjustSaturation(testObjectRenderer, saturationIncrement);
    }

    // Adjust the brightness of the test object
    public void AdjustTestObjectValue(float valueIncrement)
    {
        AdjustValue(testObjectRenderer, valueIncrement);
    }

    private void AdjustHue(Renderer renderer, float hueOffset)
    {
        if (renderer != null)
        {
            Color color = renderer.material.color;
            Color.RGBToHSV(color, out float h, out float s, out float v);

            // Logging Color Data
            DataHandler.Instance.LogColorData(h, s, v);


            h = (h + hueOffset) % 1.0f;
            if (h < 0) h += 1.0f;
            Color newColor = Color.HSVToRGB(h, s, v);
            renderer.material.color = newColor;
        }
    }

    private void AdjustSaturation(Renderer renderer, float saturationIncrement)
    {
        if (renderer != null)
        {
            Color color = renderer.material.color;
            Color.RGBToHSV(color, out float h, out float s, out float v);
            s = (s + saturationIncrement) % 1.0f;
            if (s < 0) s += 1.0f;
            Color newColor = Color.HSVToRGB(h, s, v);
            renderer.material.color = newColor;
        }
    }

    private void AdjustValue(Renderer renderer, float valueIncrement)
    {
        if (renderer != null)
        {
            Color color = renderer.material.color;
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v = (v + valueIncrement) % 1.0f;
            if (v < 0) v += 1.0f;
            Color newColor = Color.HSVToRGB(h, s, v);
            renderer.material.color = newColor;
        }
    }
}
