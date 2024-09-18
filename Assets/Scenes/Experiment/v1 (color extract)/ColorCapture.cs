using UnityEngine;
using System.IO;

public class ColorCapture : MonoBehaviour
{
    public Camera captureCamera; // The camera that captures the sphere
    public RenderTexture renderTexture; // The render texture where the camera outputs its image
    public Material sphereMaterial; // The material of the sphere
    public Light pointLight1;  // First point light for lighting temperature
    public Light pointLight2;  // Second point light for lighting intensity
    
    
    public string inputCSVFilePath = "Assets/InputData.csv";  // Path to the input CSV file
    public string outputCSVFilePath = "Assets/OutputData.csv";  // Path to save the output CSV file


    void Start()
    {
        Color control_blue = Color.HSVToRGB(240 / 360.0f, 100 / 100.0f, 100 / 100.0f);
        Color control_red = Color.HSVToRGB(0 / 360.0f, 100 / 100.0f, 100 / 100.0f);
        Color control_green = Color.HSVToRGB(120 / 360.0f, 100 / 100.0f, 100 / 100.0f);

        SetSphereColor(control_green);

        // Ensure the camera's target texture is set correctly
        captureCamera.targetTexture = renderTexture;

        // Start the color extraction process
        ExtractAndDebugSphereColor();
    }

    void SetSphereColor(Color color)
    {
        if (sphereMaterial != null)
        {
            sphereMaterial.color = color;
        }
        else
        {
            Debug.LogWarning("Sphere material is not assigned.");
        }
    }

    void ExtractAndDebugSphereColor()
    {
        // Create a Texture2D to read the RenderTexture
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;

        // Render the camera's view to the RenderTexture
        captureCamera.Render();

        // Read the RenderTexture into the Texture2D
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Apply gamma correction if using Linear color space
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = texture.GetPixel(x, y);
                    color = color.gamma; // Convert from Linear to Gamma space
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
        }

        // Variables to store the sum of valid colors and the count of valid pixels
        Color colorSum = Color.black;
        int validPixelCount = 0;
        int invalidPixelCount = 0;

        // Center and radius of the circle
        Vector2 center = new Vector2(texture.width / 2, texture.height / 2);
        float radius = Mathf.Min(texture.width, texture.height) / 2;

        // Create a new texture to visualize the mask
        Texture2D maskedTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);

        // Loop through each pixel in the texture
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                // Calculate distance from the center
                float distance = Vector2.Distance(new Vector2(x, y), center);

                // Check if the pixel is within the circle
                if (distance <= radius)
                {
                    Color pixelColor = texture.GetPixel(x, y);

                    // Accumulate color only within the circular area
                    colorSum += pixelColor;
                    validPixelCount++;

                    // Set the masked texture's pixel to the original color
                    maskedTexture.SetPixel(x, y, pixelColor);
                }
                else
                {
                    invalidPixelCount++;

                    // Set the masked texture's pixel to black (or any color to indicate exclusion)
                    maskedTexture.SetPixel(x, y, Color.black);
                }
            }
        }

        maskedTexture.Apply();

        // Calculate the average color of the sphere
        Color averageColor = (validPixelCount > 0) ? colorSum / validPixelCount : Color.black;

        // Convert to HSV
        float h, s, v;
        Color.RGBToHSV(averageColor, out h, out s, out v);
        h *= 360f;
        s *= 100f;
        v *= 100f;

        Debug.Log($"Average Sphere Color in HSV (excluding background): hsv({h:F2},{s:F2},{v:F2})");
        Debug.Log($"hsv({h},{s},{v})");
        Debug.Log($"Valid Pixel Count: {validPixelCount}, Invalid Pixel Count: {invalidPixelCount}");
    }

    void SaveTextureAsPNG(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();
        string filePath = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Saved texture as {filePath}");
    }


}
