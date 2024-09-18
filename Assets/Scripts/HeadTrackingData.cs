using System.Collections.Generic;
using UnityEngine;

public class HeadTrackingData
{
    public Vector3 position;
    public Quaternion rotation;
    public float timeDelta;
    public float movementDuration;
    public float cumulativeRotation;
    public float angularVelocity;
    public float rotationDirection; // Positive for right, negative for left

    public HeadTrackingData(Vector3 position, Quaternion rotation, float timeDelta, float movementDuration, float cumulativeRotation, float angularVelocity, float rotationDirection)
    {
        this.position = position;
        this.rotation = rotation;
        this.timeDelta = timeDelta;
        this.movementDuration = movementDuration;
        this.cumulativeRotation = cumulativeRotation;
        this.angularVelocity = angularVelocity;
        this.rotationDirection = rotationDirection;
    }
    public string ToCSVString()
    {
        string posString = $"{position.x}:{position.y}:{position.z}";
        string rotString = $"{rotation.x}:{rotation.y}:{rotation.z}:{rotation.w}";

        return $"{timeDelta},{posString},{rotString},{movementDuration},{cumulativeRotation},{angularVelocity},{rotationDirection}";
    }

}