using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataHandler
{

    private static DataHandler _instance;
    public static DataHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DataHandler();
            }
            return _instance;
        }
    }

    // Participant ID
    public int ParticipantID { get; set; }

    // File path to save CSV to
    private string ExportFilePath;

    // Data structures for storing tracked data metrics
    private List<InputData> inputDataList = new List<InputData>();
    private List<TrialResultData> trialResultDataList = new List<TrialResultData>();
    private List<ColorData> colorDataList = new List<ColorData>();
    private List<HeadTrackingData> headTrackingDataList = new List<HeadTrackingData>();

    // Structure to store input data
    public struct InputData
    {
        public int participantId;
        public int trialId;
        public string trialDesc;
        public (float hue, float saturation, float value) userMatch;
        public (float hue, float saturation, float value) boundLimit;
        public float matchHue;
        public float boundHue;
        public float stepSize;
        public float stepSizeInHueAdjustment;
        public string staircaseType;
        public float timeDelta;

        public InputData(int participantId, int trialId, string trialDesc, Color match, Color bound, float stepSize, string staircaseType)
        {
            this.participantId = participantId;   
            
            this.trialId = trialId;
            this.trialDesc = trialDesc;
            this.userMatch = UtilityMethods.serializeColorToHSVTuple(match);
            this.boundLimit = UtilityMethods.serializeColorToHSVTuple(bound);
            this.matchHue = UtilityMethods.serializeColorToHueValue(match);
            this.boundHue = UtilityMethods.serializeColorToHueValue(bound);
            this.stepSize = stepSize;
            this.staircaseType = staircaseType;
            this.timeDelta = Time.time;

            // calculate stepSize in hue adjustment
            this.stepSizeInHueAdjustment = stepSize * 360f;

        }
    }

    // Structure to store trial result data
    public struct TrialResultData
    {
        public int participantId;
        public int trialId;
        public List<Color> matchPointsUB;
        public List<Color> matchPointsLB;
        public float thresholdUBHue;
        public float thresholdLBHue;
        public float constancyThreshold;

        public TrialResultData(int participantId, int trialId, List<Color> matchPointsUB, List<Color> matchPointsLB)
        {
            // INPUT FOR PARTICIPANT ID
            this.participantId = participantId;


            this.trialId = trialId;
            this.matchPointsUB = matchPointsUB;
            this.matchPointsLB = matchPointsLB;

            (Color thresholdUB, Color thresholdLB, float constancyThreshold) = UtilityMethods.CalculateThresholdForLBUB(matchPointsUB, matchPointsLB);

            this.thresholdUBHue = UtilityMethods.serializeColorToHueValue(thresholdUB);
            this.thresholdLBHue = UtilityMethods.serializeColorToHueValue(thresholdLB);
            this.constancyThreshold = constancyThreshold;
        }
    }

    public struct ColorData
    {
        public float hue;
        public float saturation;
        public float brightness;
        public float timeDelta;

        public ColorData(float hue, float saturation, float brightness)
        {
            this.hue = hue;
            this.saturation = saturation;
            this.brightness = brightness;
            this.timeDelta = Time.time;
        }
    }

    private void generateExportFilePath()
    {
        string resultsFolder = System.IO.Path.Combine(Application.dataPath, "Participant Results");
        string participantResults = System.IO.Path.Combine(resultsFolder, $"Participant-{ParticipantID}");

        if (!System.IO.Directory.Exists(resultsFolder))
        {
            System.IO.Directory.CreateDirectory(resultsFolder);
        }
        
        if (!System.IO.Directory.Exists(participantResults))
        {
            System.IO.Directory.CreateDirectory(participantResults);
        }

        ExportFilePath = participantResults;

    }

    // Method to record input data
    public void LogInputData(int trialId, string trialDesc, Color match, Color bound, float stepSize, string staircaseType)
    {
        Debug.Log($"DATA HANDLER - Input Data Logged {match}");
        InputData inputData = new InputData(ParticipantID, trialId, trialDesc, match, bound, stepSize, staircaseType);
        inputDataList.Add(inputData);
    }

    // Method to record trial result data
    public void LogTrialResultData(int trialId, List<Color> matchPointsUB, List<Color> matchPointsLB)
    {
        Debug.Log("DATA HANDLER - Trial Data Logged");
        TrialResultData trialResultData = new TrialResultData(ParticipantID, trialId, matchPointsUB, matchPointsLB);
        trialResultDataList.Add(trialResultData);
    }

    public void LogColorData(float hue, float saturation, float brightness)
    {
        Debug.Log("DATA HANDLER - Color Data Logged");
        ColorData colorData = new ColorData(hue, saturation, brightness);
        colorDataList.Add(colorData);
    }

    public void LogHeadTrackingData(HeadTrackingData data)
    {
        Debug.Log("DATA HANDLER - Head Data Logged");
        headTrackingDataList.Add(data);
        Debug.Log($"position: {data.position}, rotation: {data.rotation}, {data.timeDelta}");
    }

    // Saves user's input data and trial results to CSV
    public void ExportTrialDataToCSV()
    {
        generateExportFilePath();
        string sceneName = ExperimentController.Instance.GetSceneName();

        string inputDataFilename = System.IO.Path.Combine(ExportFilePath, $"Participant{ParticipantID}-{sceneName}-SingleInputData.csv");
        string trialDataFilename = System.IO.Path.Combine(ExportFilePath, $"Participant{ParticipantID}-{sceneName}-AllTrialsData.csv");
        string colorDataFilename = System.IO.Path.Combine(ExportFilePath, $"Participant{ParticipantID}-{sceneName}-ColorData.csv");
        string headTrackingDataFilename = System.IO.Path.Combine(ExportFilePath, $"Participant{ParticipantID}-{sceneName}-HeadTrackingData.csv");


        SaveInputDataToCSV(inputDataFilename);
        SaveColorDataToCSV(colorDataFilename);
        SaveTrialResultDataToCSV(trialDataFilename);
        SaveHeadTrackingDataToCSV(headTrackingDataFilename);

    }

    private void SaveColorDataToCSV(string filePath)
    {
        List<string> lines = new List<string>
        {
            "TimeDelta,Hue,Saturation,Brightness"
        };

        foreach (var colorData in colorDataList)
        {
            string line = $"{colorData.timeDelta},{colorData.hue},{colorData.saturation},{colorData.brightness}";
            lines.Add(line);
        }

        File.WriteAllLines(filePath, lines);

    }

    public void SaveHeadTrackingDataToCSV(string filePath)
    {
        List<string> lines = new List<string>
        {
            "TimeDelta,Position,Rotation,TotalMovementDuration,CumulativeRotation,AngularVelocity,RotationDirection"
        };
        foreach (var headData in headTrackingDataList)
        {
            string line = headData.ToCSVString();
            lines.Add(line);
        }

        File.WriteAllLines(filePath, lines);
        
    }


    private void SaveInputDataToCSV(string filePath)
    {
        List<string> lines = new List<string>
        {
            "ParticipantId,TrialId,TrialDescription,UserMatchHSV,BoundLimitHSV,MatchHue,BoundHue,StepSize,StepSizeInHueAdjustment,StaircaseType"
        };

        foreach (var inputData in inputDataList)
        {
            string usrMatchHSV = $"({inputData.userMatch.hue}:{inputData.userMatch.saturation}:{inputData.userMatch.value})";
            string boundLimitHSV = $"({inputData.boundLimit.hue}:{inputData.boundLimit.saturation}:{inputData.boundLimit.value})";

            string line = $"{inputData.participantId},{inputData.trialId},{inputData.trialDesc}," +
                          $"{usrMatchHSV}," +
                          $"{boundLimitHSV}," +
                          $"{inputData.matchHue},{inputData.boundHue},{inputData.stepSize},{inputData.stepSizeInHueAdjustment}," +
                          $"{inputData.staircaseType}";
            lines.Add(line);
        }

        File.WriteAllLines(filePath, lines);
    }
    
    // Method to save trial result data to CSV
    private void SaveTrialResultDataToCSV(string filePath)
    {
        List<string> lines = new List<string>
        {
            "ParticipantId,TrialId,MatchPointsUB,MatchPointsLB,ThresholdUBHue,ThresholdLBHue,ConstancyThreshold"
        };

        foreach (var trialResultData in trialResultDataList)
        {
            string matchPointsUB = SerializeColorList(trialResultData.matchPointsUB);
            string matchPointsLB = SerializeColorList(trialResultData.matchPointsLB);

            string line = $"{trialResultData.participantId},{trialResultData.trialId}," +
                          $"{matchPointsUB},{matchPointsLB}," +
                          $"{trialResultData.thresholdUBHue},{trialResultData.thresholdLBHue}," +
                          $"{trialResultData.constancyThreshold}";
            lines.Add(line);
        }

        File.WriteAllLines(filePath, lines);
    }

    // Private method to serialize a list of Colors to a string
    private string SerializeColorList(List<Color> colorList)
    {
        List<string> colorStrings = new List<string>();

        foreach (var color in colorList)
        {
            var hsv = UtilityMethods.serializeColorToHSVTuple(color);
            string colorString = $"({hsv.hue}:{hsv.saturation}:{hsv.value})";
            colorStrings.Add(colorString);
        }

        return string.Join(";", colorStrings);
    }
}
