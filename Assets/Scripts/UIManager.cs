using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public TrialManager trialManager;
    public ColorManager colorManager;

    // Text UI
    public TMP_Text trialInfoText;
    public TMP_Text devTrialInformation;
    public TMP_Text pauseModalText;

    // Dropdown UI
    public TMP_Dropdown uiModeDropdown;

    // UI Panels
    public GameObject instructionSlides;
    public GameObject trialNav;
    public GameObject participantUIPanel;
    public GameObject developerUIPanel;
    public GameObject developerCanvas;
    public GameObject completionModal;
    public GameObject finishExperimentModal;
    public GameObject pauseModal;

    void Start()
    {
        // Initialize UI elements (show / hide)

        string sceneName = ExperimentController.Instance.GetSceneName();

        Debug.Log($"scene name: {sceneName}");
        
        if (sceneName == "IntroductionScene")
        {
            instructionSlides.SetActive(true);
        } 
        else
        {
            instructionSlides.SetActive(false);
        }
        

        trialNav.SetActive(false);
        participantUIPanel.SetActive(true);
        developerUIPanel.SetActive(false);
        developerCanvas.SetActive(false);
        completionModal.SetActive(false);
        finishExperimentModal.SetActive(false);
        pauseModal.SetActive(false);

        // Set the default value for the UI mode dropdown to be participant mode
        uiModeDropdown.value = 0;

        // Add listener for dropdown value change
        uiModeDropdown.onValueChanged.AddListener(delegate { OnUIModeChanged(); });

        // Initialize initial UI mode
        OnUIModeChanged();
    }

    void Update()
    {
        // Update UI elements with the latest data
        UpdateTrialHeader();
        UpdateDevConsoleTrialInfo();
    }

    public void OnUIModeChanged()
    {
        int selectedMode = uiModeDropdown.value;
        bool isParticipantMode = (selectedMode == 0);

        participantUIPanel.SetActive(isParticipantMode);
        developerUIPanel.SetActive(!isParticipantMode);
        developerCanvas.SetActive(!isParticipantMode);
    }

    public void ReloadScene()
    {
        ExperimentController.Instance.ReloadScene();
    }

    public void NextScene()
    {
        ExperimentController.Instance.LoadNextScene();
    }

    public void ShowCompletionModal()
    {
        completionModal.SetActive(true);
    }

    public void HideCompletionModal()
    {
        completionModal.SetActive(false); 
    }

    public void ShowFinishExperimentModal()
    {
        finishExperimentModal.SetActive(true);
    }

    void UpdateTrialHeader()
    {
        if (trialManager != null)
        {
            int trialIndex = trialManager.GetCurrentTrialIndex();
            int totalTrials = trialManager.GetTotalTrials();

            if (trialIndex <= totalTrials)
            {
                trialInfoText.text = $"Trial: {trialIndex} / {totalTrials}";
            }
            else
            {
                trialInfoText.text = "All Trials Complete!";
            }
        }
    }

    public void ShowPauseModal(string message)
    {

        pauseModal.SetActive(true);
        pauseModalText.text = message;

    }

    public void HidePauseModal()
    {
        pauseModal.SetActive(false);
    }

    private string CompileTrialInfo(TrialConfig config, CoroutineState staircase, float stepSize, Color refColor, Color testColor)
    {
        // Get HSV values for reference and test colors
        Color.RGBToHSV(refColor, out float refHue, out float refSaturation, out float refValue);
        Color.RGBToHSV(testColor, out float testHue, out float testSaturation, out float testValue);

        // Get staircase description
        string staircaseDescription = "Currently no staircase running.";

        if (staircase != null)
        {
            staircaseDescription = GetStaircaseDescription(staircase);
        }

        return $"Trial Description: {config.description}\n\n" +
               $"Staircase: {staircaseDescription}\n\n" +
               $"Reference Object HSV:\n" +
               $"Hue: {refHue * 360f:F2}\n" +
               $"Saturation: {refSaturation * 100f:F2}\n" +
               $"Value: {refValue * 100f:F2}\n\n" +
               $"Test Object HSV:\n" +
               $"Hue: {testHue * 360f:F2}\n" +
               $"Saturation: {testSaturation * 100f:F2}\n" +
               $"Value: {testValue * 100f:F2}";
    }


    private string GetStaircaseDescription(CoroutineState staircase)
    {
        string adjustment = "increment";
        if (staircase.staircaseType == "UB") adjustment = "decrement";
        
        return $"Currently running {staircase.staircaseType} staircase, {adjustment}ing towards reference hue at step size ({staircase.StepSize:F7}).";
    }


    void UpdateDevConsoleTrialInfo()
    {
        TrialConfig trialConfig = trialManager.GetCurrentTrialConfig();
        if (trialConfig == null)
        {
            devTrialInformation.text = "No more trials, all trials ran.";
            return;
        }

        float stepSize = trialManager.GetStepSize();
        CoroutineState staircaseInfo = trialManager.getCurrentStaircase();
        Color refColor = colorManager.GetRefObjectColor();
        Color testColor = colorManager.GetTestObjectColor();

        string output = CompileTrialInfo(trialConfig, staircaseInfo, stepSize, refColor, testColor);

        devTrialInformation.text = output;
    }




}
