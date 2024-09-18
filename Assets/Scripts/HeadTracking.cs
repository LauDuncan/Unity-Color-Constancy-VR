using System.Collections.Generic;
using UnityEngine;

public class HeadTracking : MonoBehaviour
{
    public Camera headCamera; // Assign this in the inspector to the camera of the XR Rig
    public float dataCaptureInterval = 0.1f; // Data capture interval in seconds
    public float positionThreshold = 0.025f; // Minimum movement in meters to register as movement
    public float rotationThreshold = 2.0f; // Minimum rotation in degrees to register as movement
    public bool isLogging = false; // Flag to control data logging

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float timer;
    private float cumulativeRotation;
    private float movementDuration;


    private void Start()
    {
        if (headCamera != null)
        {
            lastPosition = headCamera.transform.position;
            lastRotation = headCamera.transform.rotation;
        }
        timer = 0f;
        cumulativeRotation = 0f;
        movementDuration = 0f;
    }

    private void Update()
    {
        if (!isLogging) return; // Do not log data if logging is turned off

        if (headCamera == null)
        {
            Debug.LogWarning("Head camera is not assigned.");
            return;
        }

        timer += Time.deltaTime;

        if (timer >= dataCaptureInterval)
        {
            timer = 0f;
            Vector3 currentPosition = headCamera.transform.position;
            Quaternion currentRotation = headCamera.transform.rotation;

            if (HasSignificantMovement(currentPosition, currentRotation))
            {
                float timeDelta = Time.time;
                Vector3 rotationDelta = currentRotation.eulerAngles - lastRotation.eulerAngles;
                float angularVelocity = rotationDelta.magnitude / dataCaptureInterval;
                float rotationAngle = Quaternion.Angle(lastRotation, currentRotation);

                cumulativeRotation += rotationAngle;
                movementDuration += dataCaptureInterval;

                // Determine if rotation is left (negative) or right (positive)
                Vector3 crossProduct = Vector3.Cross(lastRotation * Vector3.forward, currentRotation * Vector3.forward);
                float rotationDirection = Vector3.Dot(crossProduct, Vector3.up) > 0 ? rotationAngle : -rotationAngle;

                DataHandler.Instance.LogHeadTrackingData(new HeadTrackingData(currentPosition, currentRotation, timeDelta, movementDuration, cumulativeRotation, angularVelocity, rotationDirection));

                Debug.Log($"position: {currentPosition},rotation: {currentRotation}, time: {timeDelta}, moveDuration: {movementDuration}, cumulativeRotation{cumulativeRotation}, angVelocity{angularVelocity}, rotationDirection{rotationDirection}");

                lastPosition = currentPosition;
                lastRotation = currentRotation;
            }
        }
    }

    private bool HasSignificantMovement(Vector3 currentPosition, Quaternion currentRotation)
    {
        float positionDelta = Vector3.Distance(currentPosition, lastPosition);
        float rotationDelta = Quaternion.Angle(currentRotation, lastRotation);
        return positionDelta > positionThreshold || rotationDelta > rotationThreshold;
    }

    public void StartLogging()
    {
        isLogging = true;
    }

    public void StopLogging()
    {
        isLogging = false;
    }
}
