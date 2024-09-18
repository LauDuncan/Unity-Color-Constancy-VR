using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TrialConfig
{
    public int id { get; set; }
    public string description { get; set; }
    public Color initialRefColor { get; set; }
    public Color initialTestColorUB { get; set; }
    public Color initialTestColorLB { get; set; }
    public float initialStepSize { get; set; }
    public float initialOffset { get; set; }

    public TrialConfig(int id, string description, Color refColor, Color testColorUB, Color testColorLB, float stepSize, float offset)
    {
        this.id = id;
        this.description = description;
        initialRefColor = refColor;
        initialTestColorUB = testColorUB;
        initialTestColorLB = testColorLB;
        initialStepSize = stepSize;
        initialOffset = offset;
    }
}
