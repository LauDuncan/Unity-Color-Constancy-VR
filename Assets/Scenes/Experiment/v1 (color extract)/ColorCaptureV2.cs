using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ColorCaptureV2 : MonoBehaviour
{
    public Camera captureCamera;  // The camera that captures the sphere
    public RenderTexture renderTexture;  // The render texture where the camera outputs its image
    public Material sphereMaterial;  // The material of the sphere
    public Light pointLight1;  // First point light for lighting temperature
    public Light pointLight2;  // Second point light for lighting intensity
    private string inputCSVFilePath = "Assets/DataCleaning/1_combined_HSV_centroid_data_for_render_texture.csv";  // Path to the input CSV file
    private string outputCSVFilePath = "Assets/DataCleaning/2_render_texture_output.csv";  // Path to save the output CSV file

    private List<string[]> outputData = new List<string[]>
    { 
        new string[]
        {
            "ParticipantID", "ColorID", "LightingTemperature", "LightingIntensity",
            "IdealMatch", "TextureRenderCentroidUB", "TextureRenderCentroidLB"
        }
    };

    private List<string[]> inputData;
    private int currentRow = 0;  // Track the current row being processed

    // Dictionary to cache the calculated ideal values
    private Dictionary<string, string> idealMatchCache = new Dictionary<string, string>();

    void Start()
    {

        // Load the participant's CSV data
        inputData = LoadCSV(inputCSVFilePath).Skip(1).ToList();


        // Load the participant's CSV data
        inputData = LoadCSV(inputCSVFilePath).Skip(1).ToList();


        float colorTemperature1 = 6500f;
        float colorTemperature2 = 7000f;
        float colorTemperature3 = 4000f;
        float colorTemperature4 = 10000f;

        // Convert the color temperature to RGB
        Color rgb1 = Mathf.CorrelatedColorTemperatureToRGB(colorTemperature1);
        Color rgb2 = Mathf.CorrelatedColorTemperatureToRGB(colorTemperature2);
        Color rgb3 = Mathf.CorrelatedColorTemperatureToRGB(colorTemperature3);
        Color rgb4 = Mathf.CorrelatedColorTemperatureToRGB(colorTemperature4);

        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            Debug.Log($"entered");
            rgb1 = rgb1.gamma;
            rgb2 = rgb2.gamma;
            rgb3 = rgb3.gamma;
            rgb4 = rgb4.gamma;
        }

        // Convert the RGB values from 0-1 to 0-255 range
        string rgbString1 = $"rgb({rgb1.r * 255}, {rgb1.g * 255}, {rgb1.b * 255})";
        string rgbString2 = $"rgb({rgb2.r * 255}, {rgb2.g * 255}, {rgb2.b * 255})";
        string rgbString3 = $"rgb({rgb3.r * 255}, {rgb3.g * 255}, {rgb3.b * 255})";
        string rgbString4 = $"rgb({rgb4.r * 255}, {rgb4.g * 255}, {rgb4.b * 255})";

        Debug.Log($"RGB for {colorTemperature1}K: {rgbString1}");
        Debug.Log($"RGB for {colorTemperature2}K: {rgbString2}");
        Debug.Log($"RGB for {colorTemperature3}K: {rgbString3}");
        Debug.Log($"RGB for {colorTemperature4}K: {rgbString4}");



        // Start processing data with a coroutine
        StartCoroutine(ProcessTrials());
    }

IEnumerator ProcessTrials()
    {
        while (currentRow < inputData.Count)
        {
            var row = inputData[currentRow];

            // Extract information from the CSV row
            int participantID = int.Parse(row[0]);
            int colorID = int.Parse(row[1]);
            string lightingTemperature = row[2];
            string lightingIntensity = row[3];
            Vector3 centroidUB = ParseVector(row[4]);  // Parse the centroidUB_HSV values from the CSV
            Vector3 centroidLB = ParseVector(row[5]);  // Parse the centroidLB_HSV values from the CSV

            // Create a cache key based on ColorID, LightingTemperature, and LightingIntensity
            string cacheKey = $"{colorID}-{lightingTemperature}-{lightingIntensity}";

            // Adjust scene lighting based on the CSV data
            AdjustLighting(lightingTemperature, lightingIntensity);

            // Check if the ideal value is already calculated and stored in the cache
            string idealMatch;
            if (!idealMatchCache.TryGetValue(cacheKey, out idealMatch))
            {
                // Set the sphere to the reference color and capture the ideal match
                SetSphereColor(GetColorByID(colorID));
                idealMatch = CaptureSphereColor();

                // Store the calculated ideal value in the cache
                idealMatchCache[cacheKey] = idealMatch;
            }

            // Set the sphere to the centroid UB color and capture the render texture
            SetSphereColor(centroidUB);
            string textureRenderCentroidUB = CaptureSphereColor();

            // Set the sphere to the centroid LB color and capture the render texture
            SetSphereColor(centroidLB);
            string textureRenderCentroidLB = CaptureSphereColor();

            // Add captured colors to the row and save to output data
            var newRow = new string[]
            {
                row[0],  // ParticipantID
                row[1],  // ColorID
                row[2],  // LightingTemperature
                row[3],  // LightingIntensity
                idealMatch,  // Ideal Match color
                textureRenderCentroidUB,  // Texture Render Centroid UB
                textureRenderCentroidLB  // Texture Render Centroid LB
            };
            outputData.Add(newRow);

            // Move to the next row
            currentRow++;

            // Save the output data to CSV after each trial (optional)
            SaveCSV(outputCSVFilePath, outputData);

            // Wait for a frame before continuing
            yield return null;
        }

        Debug.Log("Processing completed.");
    }

    // Adjusts the lighting conditions based on CSV data
    void AdjustLighting(string lightingTemperature, string lightingIntensity)
    {
        // Adjust the temperature of the lights
        float kelvin = float.Parse(lightingTemperature.Replace("K", ""));
        pointLight1.colorTemperature = kelvin;
        pointLight2.colorTemperature = kelvin;

        // Adjust the intensity of the lights
        float intensity = 0f;
        switch (lightingIntensity.ToLower())
        {
            case "low":
                intensity = 0.4f;
                break;
            case "normal":
                intensity = 1.0f;
                break;
            case "high":
                intensity = 1.6f;
                break;
        }
        pointLight1.intensity = intensity;
        pointLight2.intensity = intensity;
    }

    // Sets the sphere's material color
    void SetSphereColor(Color color)
    {
        if (sphereMaterial != null)
        {
            sphereMaterial.color = color;
        }
    }

    // Overloaded version to set the sphere's color using a Vector3 for the centroid (HSV values)
    void SetSphereColor(Vector3 hsvVector)
    {
        // Convert HSV to RGB
        Color color = Color.HSVToRGB(hsvVector.x, hsvVector.y, hsvVector.z);

        // Set the sphere's material color
        SetSphereColor(color);
    }

    // Captures the average color of the sphere using the RenderTexture
    string CaptureSphereColor()
    {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        captureCamera.Render();
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Apply gamma correction if in Linear space
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = texture.GetPixel(x, y);
                    color = color.gamma;
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
        }

        string averageColor = GetAverageColor(texture);
        Destroy(texture);  // Clean up to prevent memory leaks
        return averageColor;
    }

    // Computes the average color of the Texture2D
    string GetAverageColor(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        Color colorSum = Color.black;
        int validPixelCount = 0;
        int invalidPixelCount = 0;

        // Center and radius of the circle
        Vector2 center = new Vector2(texture.width / 2, texture.height / 2);
        float radius = Mathf.Min(texture.width, texture.height) / 2;

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
                }
                else
                {
                    invalidPixelCount++;
                }
            }
        }

        Color averageColor = (validPixelCount > 0) ? colorSum / validPixelCount : Color.black;

        float h, s, v;
        Color.RGBToHSV(averageColor, out h, out s, out v);

        Debug.Log($"Average Sphere Color in HSV: hsv({h * 360f:F2},{s * 100f:F2},{v * 100f:F2}). Valid Pixel Count: {validPixelCount}, Invalid Pixel Count: {invalidPixelCount}");

        return $"({h:F8}:{s:F8}:{v:F8})";
    }

    // Parses a CSV string into a Vector3 for color values
    Vector3 ParseVector(string vectorString)
    {
        // Remove the square brackets and any extra spaces
        string cleanedString = vectorString.Trim('[', ']').Trim();

        // Split the string by commas, and ignore any empty entries resulting from extra spaces
        string[] values = cleanedString.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Parse the values to floats and create the Vector3
        return new Vector3(
            float.Parse(values[0]),
            float.Parse(values[1]),
            float.Parse(values[2])
        );
    }


    // Loads a CSV file into a list of string arrays (rows)
    List<string[]> LoadCSV(string filePath)
    {
        List<string[]> data = new List<string[]>();
        using (StreamReader sr = new StreamReader(filePath))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] fields = line.Split(',');
                data.Add(fields);
            }
        }
        return data;
    }

    // Saves a list of string arrays (rows) to a CSV file
    void SaveCSV(string filePath, List<string[]> data)
    {
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            foreach (var row in data)
            {
                sw.WriteLine(string.Join(",", row));
            }
        }
    }

    // Returns the color corresponding to the ColorID
    Color GetColorByID(int colorID)
    {
        switch (colorID)
        {
            case 1:
                return Color.HSVToRGB(240 / 360.0f, 100 / 100.0f, 100 / 100.0f);  // Blue
            case 2:
                return Color.HSVToRGB(0 / 360.0f, 100 / 100.0f, 100 / 100.0f);  // Red
            case 3:
                return Color.HSVToRGB(120 / 360.0f, 100 / 100.0f, 100 / 100.0f);  // Green
            default:
                return Color.black;  // Default or error case
        }
    }
}
