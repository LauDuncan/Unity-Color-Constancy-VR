using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UISlideManager : MonoBehaviour
{
    public TMP_Text headerText;
    public TMP_Text bodyText;
    public GameObject canvas;

    private List<string> headers;
    private List<string> bodies;

    private int currentSlideIndex = 0;

    void Start()
    {
        InitializeSlides();
        if (headers.Count > 0 && bodies.Count > 0)
        {
            ShowCurrentSlide();
        }
        else
        {
            Debug.LogWarning("No slides found. Please add headers and bodies in the Inspector.");
        }
    }

    private void InitializeSlides()
    {
        headers = new List<string>
        {
            "Welcome to the Color Constancy Experiment!",
            "What You Will See",
            "What You Will Do",
            "When You Perceive a Color Match",
            "Important Notes",
            "Let's Begin!"
        };

        bodies = new List<string>
        {
            "In this experiment, you will be helping us understand how changes in lighting affect color perception.",
            "You will be presented with two objects on a table.\n The Reference Object (on the left) will remain constant in color, and the Test Object (on the right) will change its color gradually.",
            "Your task is to observe the color changes in the Test Object, and indicate when you perceive its color to match with its reference!",
            "Press the (B) Button on your Right Controller.\n There will be a reminder of the task after your input, just press the (B) Button again to make it go away.",
            "Press the Menu Button on your Left Controller to view the Trial Menu. You will need to use it to start your trials, and move on to the next scene.",
            "When you are ready, aim your right controller to the circular platform and push the joystick forward to teleport!"
        };
    }

    public void OnContinueButtonPressed()
    {
        currentSlideIndex++;
        if (currentSlideIndex < headers.Count && currentSlideIndex < bodies.Count)
        {
            ShowCurrentSlide();
        }
        else
        {
            canvas.SetActive(false);
        }
    }

    private void ShowCurrentSlide()
    {
        headerText.text = headers[currentSlideIndex];
        bodyText.text = bodies[currentSlideIndex];
    }
}
