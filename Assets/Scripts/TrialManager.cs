using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting.FullSerializer;
using System.Linq;

public class TrialManager : MonoBehaviour
{
    public ColorManager colorManager;
    public UIManager uiManager;
    public HeadTracking headTrack;
    private List<TrialConfig> trials;
    private int currentTrialIndex;

    // Upper Bound and Lower Bound Staircase
    private List<Color> currentMatchPointsUB;
    private List<Color> currentMatchPointsLB;
    private CoroutineState staircaseUB;
    private CoroutineState staircaseLB;
    private CoroutineState currentStaircase;

    private Dictionary<int, List<List<Color>>> trialResults;

    // Trial Flags
    public bool isTrialRunning { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public bool isAllTrialDone { get; private set; } = false;
    private bool userInputMatch;

    // Coroutine
    private Coroutine currentCoroutine;

    // Constants for Trial Config
    private float INITAL_STEP_SIZE = 0.020f; // Initial step size in the beginning of trial
    private float STEP_SIZE_DECREMENT_FACTOR = 1.5f; // Factor by which the step size is divded by at each reversal
    private float RANGE_INTERVAL = 10.0f; // The multiple of step size to be considered as range
    private float MIN_STEP_SIZE = 0.0019f; // Stops trials when step size below this
    private float TIME_INTERVAL = 0.6f; // Time in seconds for each color change step


    // ====== Configruing Color Constancy Trials ======

    void Start()
    {
        InitializeTrials();
        currentTrialIndex = 0;
        userInputMatch = false;
    }

    void InitializeTrials()
    {
        trials = new List<TrialConfig>
        {
            CreateTrialConfig(1, "Trial 1: Blue", 240, 100, 100, 40),
            CreateTrialConfig(2, "Trial 2: Red", 0, 100, 100, 40),
            CreateTrialConfig(3, "Trial 3: Green", 120, 100, 100, 40),
        };

        // Shuffle the trials list using LINQ
        trials = trials.OrderBy(t => Random.value).ToList();

        trialResults = new Dictionary<int, List<List<Color>>>();
    }

    TrialConfig CreateTrialConfig(int id, string description, int h, int s, int v, int offset)
    {
        // Normalize input values
        float normalizedHue = h / 360.0f;
        float normalizedSaturation = s / 100.0f;
        float normalizedValue = v / 100.0f;

        // Convert reference color from HSV to RGB
        Color refColor = Color.HSVToRGB(normalizedHue, normalizedSaturation, normalizedValue);

        float testSaturation = normalizedSaturation;
        float testValue = normalizedValue;

        // TODO: make the range (offset) a multiple of stepsize, make sure to normalize before applying it

        float testHueUB = (h + offset) / 360.0f;
        if (testHueUB >= 1.0f) testHueUB -= 1.0f; // Ensure the hue value wraps around correctly

        float testHueLB = (h - offset) / 360.0f;
        if (testHueLB < 0) testHueLB += 1.0f; // Ensure the hue value wraps around correctly


        // Convert test color from HSV to RGB
        Color testColorUB = Color.HSVToRGB(testHueUB, testSaturation, testValue);
        Color testColorLB = Color.HSVToRGB(testHueLB, testSaturation, testValue);

        return new TrialConfig(id, description, refColor, testColorUB, testColorLB, INITAL_STEP_SIZE, offset);
    }

    // ======= Initializing Trials =======

    void InitializeTrialWithConfig()
    {
        if (currentTrialIndex < trials.Count)
        {
            TrialConfig config = trials[currentTrialIndex];
            currentStaircase = null;

            // Initialize Staircases
            InitializeUpperBoundStaircase(config);
            InitializeLowerBoundStaircase(config);

            Debug.Log($"Initializing Trial {config.id}, UB Step Size: {staircaseUB.StepSize}, LB Step Size: {staircaseLB.StepSize}, Initial Step Size {config.initialStepSize}");

            isTrialRunning = false;
            userInputMatch = false;

            // Initialize points in the dictionary for the current trial
            trialResults[config.id] = new List<List<Color>> { currentMatchPointsUB, currentMatchPointsLB };

            // Initialize colors for the trial
            colorManager.SetRefObjectColor(Color.white);
            colorManager.SetTestObjectColor(Color.white);
        }
        else
        {
            CompleteTrial();
            isAllTrialDone = true;
        }
    }

    void InitializeUpperBoundStaircase(TrialConfig config)
    {
        staircaseUB = new CoroutineState(config.initialStepSize, (config.initialRefColor, config.initialTestColorUB), "UB");
        currentMatchPointsUB = new List<Color>();
        
    }

    void InitializeLowerBoundStaircase(TrialConfig config)
    {
        staircaseLB = new CoroutineState(config.initialStepSize, (config.initialRefColor, config.initialTestColorLB), "LB");
        currentMatchPointsLB = new List<Color>();

    }


    // ======= Trial Coroutine Methods =======

    private void StartNextStaircase(CoroutineState state)
    {
        currentStaircase = state;

        StopCurrentCoroutine();

        if (staircaseUB.StepSize >= MIN_STEP_SIZE || staircaseLB.StepSize >= MIN_STEP_SIZE)
        {
            colorManager.SetRefObjectColor(currentStaircase.Range.refColor);
            colorManager.SetTestObjectColor(currentStaircase.Range.testColorBound);
            currentCoroutine = StartCoroutine(StaircaseCoroutine(currentStaircase));
        }
        else
        {
            CompleteTrial();
        }
    }

    private IEnumerator StaircaseCoroutine(CoroutineState state)
    {
        Debug.Log($"Running {state.staircaseType} Staircase with step {state.StepSize}, and bound limit at {state.Range.testColorBound}.");

        isTrialRunning = true;

        while (!userInputMatch)
        {
            //Debug.Log($"while loop entered, is paused = {isPaused}");

            yield return new WaitUntil(() => !isPaused);

            if (state.staircaseType == "UB")
            {
                DecrementColorTowardsReference(state);
            }
            else if (state.staircaseType == "LB")
            {
                IncrementColorTowardsReference(state);
            }

            yield return new WaitForSeconds(TIME_INTERVAL);
        }
        
    }

    // UB Staircase Hue Adjustment:
    // Decrement by step size, if test hue is less than ref hue, test hue = ref hue
    private void DecrementColorTowardsReference(CoroutineState state)
    {
        Color testColor = colorManager.GetTestObjectColor();
        Color refColor = state.Range.refColor;

        // Get HSV values for reference and test colors
        Color.RGBToHSV(refColor, out float refHue, out _, out _);
        Color.RGBToHSV(testColor, out float testHue, out _, out _);

        testHue -= state.StepSize;
        //if (testHue < 0) testHue += 1.0f;

        // Decrement of test hue will go below ref hue

        if (testHue <= refHue)
        {
            Debug.Log("UB - last DECREMENT of hue fell below ref hue, looping to range bound");
            colorManager.SetTestObjectColor(state.Range.testColorBound);
        }
        else
        {
            Debug.Log("UB - DECREMENT");
            colorManager.AdjustTestObjectHue(-state.StepSize);
        }

    }

    // LB Staircase Hue Adjustment
    // Increment by step size, if test hue is less than ref hue, test hue = ref hue.
    private void IncrementColorTowardsReference(CoroutineState state)
    {
        Color testColor = colorManager.GetTestObjectColor();
        Color refColor = state.Range.refColor;
        Color boundColor = state.Range.testColorBound;

        // Get HSV values for reference and test colors
        Color.RGBToHSV(refColor, out float refHue, out _, out _);
        Color.RGBToHSV(testColor, out float testHue, out _, out _);
        Color.RGBToHSV(boundColor, out float boundHue, out _, out _);


        float initialTestHue = testHue;
        testHue += state.StepSize;

        // Wrap around if the hue exceeds 1.0
        if (testHue > 1.0f) testHue -= 1.0f;

        //Debug.Log($"initial hue: {initialTestHue}, after increment {testHue}, bound hue: {boundHue}, ref hue: {refHue}");

        if (refHue == 0f) // check if hue is red
        {
            // Case 1: Check if hue after increment wrapped (where testHue is less than initialTestHue even after an increment),
            // and if testHue (after increment) is greater than refHue. 
            if (initialTestHue > testHue && testHue >= refHue)
            {
                Debug.Log("LB - last INCREMENT of hue went above ref hue after wrapping, looping to range bound");
                colorManager.SetTestObjectColor(state.Range.testColorBound);
            }
            // Case 2: When both initialTestHue and testHue is already wrapped (both greater than refHue),
            // check if there is an increment (initial greater than after) 
            else if (initialTestHue < testHue && testHue > refHue)
            {
                Debug.Log("LB - INCREMENT (wrapped hues)");
                colorManager.AdjustTestObjectHue(state.StepSize);
            }
        }
        else // all other cases where hue is not red
        {

            if (testHue >= refHue)
            {
                Debug.Log("LB - last INCREMENT of hue went above ref hue, looping to range bound");
                colorManager.SetTestObjectColor(state.Range.testColorBound);
            }
            // Regular Increment
            else
            {
                Debug.Log("LB - INCREMENT");
                colorManager.AdjustTestObjectHue(state.StepSize);
            }
        }
    }


    void CompleteTrial()
    {
        
        isTrialRunning = false;

        if (currentTrialIndex < trials.Count)
        {
            TrialConfig config = trials[currentTrialIndex];

            Debug.Log($"Trail {config.id} Complete, Trial Index: {currentTrialIndex}");

            // Store the current lists in the results dictionary
            trialResults[config.id][0] = new List<Color>(currentMatchPointsUB);
            trialResults[config.id][1] = new List<Color>(currentMatchPointsLB);

            //Calculate the threshold of Color Constancy using the last 3 reversals
            UtilityMethods.CalculateThresholdForLBUB(currentMatchPointsUB, currentMatchPointsLB);

            //Log completed trial results to DataHandler
            DataHandler.Instance.LogTrialResultData(config.id, currentMatchPointsUB, currentMatchPointsLB);

            uiManager.HidePauseModal();
            uiManager.ShowCompletionModal();
        }
        else
        {
            Debug.Log("All trials completed.");
            StopCurrentCoroutine();

            uiManager.HidePauseModal();
            uiManager.HideCompletionModal();
            uiManager.ShowFinishExperimentModal();

            // TODO: refactor, add data handling method for end of all trials
            // Handle end of trials (e.g., save data, notify user)

            DataHandler.Instance.ExportTrialDataToCSV();

        }

    }

    void StopCurrentCoroutine()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }


    // ======= Trial Operations =======

    public void StartTrial()
    {
        if (!isTrialRunning)
        {
            InitializeTrialWithConfig();

            if (Random.value > 0.5f)
            {
                StartNextStaircase(staircaseUB);
            }
            else
            {
                StartNextStaircase(staircaseLB);
            }

            headTrack.StartLogging();
        }
    }

    public void NextTrial()
    {
        if (!isTrialRunning)
        {
            headTrack.StopLogging();
            IncrementTrialIndex();            
            StartTrial();
        }
    }

    public void ResetTrial()
    {
        if (isTrialRunning)
        {

            StopCurrentCoroutine();
            ClearCurrentTrialData();
            InitializeTrialWithConfig();

        }
    }

    public void EndTrial()
    {
        if (isTrialRunning)
        {
            StopCurrentCoroutine();
            ClearCurrentTrialData();
            IncrementTrialIndex();
            InitializeTrialWithConfig();
        }
    }

    void IncrementTrialIndex()
    {
        Debug.Log($"current: {currentTrialIndex}, count: {trials.Count}");

        if (currentTrialIndex < trials.Count)
        {
            currentTrialIndex++;
        }

    }

    void ClearCurrentTrialData()
    {
        currentMatchPointsUB.Clear();
        currentMatchPointsLB.Clear();
    }


    // ======= Handling User Inputs =======

    // Handles user's input for perceived match, record match color coordinate, updates range, alternates staircase
    public bool PerceivedMatch()
    {
        if (isTrialRunning)
        {
            Debug.Log("Perceived Match Inputted");
            userInputMatch = true;

            TrialConfig currentConfig = trials[currentTrialIndex];

            float prevStepSize = currentStaircase.StepSize;
            Color refColor = currentStaircase.Range.refColor;
            Color boundLimit = currentStaircase.Range.testColorBound;

            float newStepSize = prevStepSize / STEP_SIZE_DECREMENT_FACTOR;
            Color userMatchColor = colorManager.GetTestObjectColor();

            // Log user match data in DataHandler
            DataHandler.Instance.LogInputData(currentConfig.id, currentConfig.description, userMatchColor, boundLimit, prevStepSize, currentStaircase.staircaseType);
            //DataHandler.LogInputData(currentConfig.id, currentConfig.description, userMatchColor, boundLimit, prevStepSize, currentStaircase.staircaseType);

            if (currentStaircase.staircaseType == "UB")
            {
                currentMatchPointsUB.Add(userMatchColor); // log user perceived match in Trial Manager
                
                float range = newStepSize * RANGE_INTERVAL;
                Color newBoundColor = UtilityMethods.getNewColorWithOffset(userMatchColor, range * 0.5f);
                
                // update next UB staircase state
                staircaseUB.StepSize = newStepSize;
                staircaseUB.Range = (refColor, newBoundColor);

                userInputMatch = false;
                PauseTrial();

                // Run Trial with LB Staircase after pause
                StartNextStaircase(staircaseLB);
            }
            else if (currentStaircase.staircaseType == "LB")
            {
                currentMatchPointsLB.Add(userMatchColor); // log user perceived match

                float range = newStepSize * RANGE_INTERVAL;
                Color newBoundColor = UtilityMethods.getNewColorWithOffset(userMatchColor, range * -0.5f);

                // update next UB staircase state
                staircaseLB.StepSize = newStepSize;
                staircaseLB.Range = (refColor, newBoundColor);

                userInputMatch = false;
                PauseTrial();

                // Run Trail with UB Staircase after pause
                StartNextStaircase(staircaseUB);
            }

            return true; // returns a bool to indicate its a valid input
        }
        return false;
    }

    public void PauseTrial()
    {
        if (!isPaused)
        {
            isPaused = true;
            uiManager.ShowPauseModal("Look for match!");
        }
    }

    public void UnpauseTrial()
    {
        if (isPaused)
        {
            isPaused = false;
            uiManager.HidePauseModal();
        }
    }


    // ======= UI Manager Get Methods =======

    public TrialConfig GetCurrentTrialConfig()
    {
        if (currentTrialIndex < trials.Count)
        {

            return trials[currentTrialIndex];
        }
        else
        {
            return null;
        }
    }

    public int GetCurrentTrialIndex()
    {
        return currentTrialIndex + 1;
    }

    public int GetTotalTrials()
    {
        return trials.Count;
    }

    public float GetStepSize()
    {
        if (currentStaircase == null)
        {
            return 0;
        }
        return currentStaircase.StepSize;
    }

    public CoroutineState getCurrentStaircase()
    {
        return currentStaircase;
    }

    public bool IsTrialRunning()
    {
        return isTrialRunning;
    }

}
