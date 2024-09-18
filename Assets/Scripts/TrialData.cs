using System.Collections.Generic;
using UnityEngine;

public class TrialData
{
    public string ParticipantID { get; set; }
    public string LightingCondition { get; set; }
    public string SceneDescription { get; set; }
    public int TrialID { get; set; }
    public string TrialDescription { get; set; }
    public float InitialStepSize { get; set; }
    public Vector3 InitialRefColorHSV { get; set; }
    public Color InitialRefColorRGB { get; set; }
    public Vector3 InitialTestColorHSV { get; set; }
    public Color InitialTestColorRGB { get; set; }
    public List<(Vector3, float)> MatchesColorCoordinatesHSV { get; set; }
    public List<(Vector3, float)> ChangesColorCoordinatesHSV { get; set; }
    public List<(Color, float)> MatchesColorCoordinatesRGB { get; set; }
    public List<(Color, float)> ChangesColorCoordinatesRGB { get; set; }
    public float CalculatedThreshold { get; set; }
}
