using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ExperimentController
{
    // ======= Set Participant ID =======
    
    
    private int participantId = 19;


    // ==================================



    private static ExperimentController _instance;
    public static ExperimentController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ExperimentController();
            }
            return _instance;
        }
    }

    private static List<string> allScenes;
    private static Queue<string> sceneQueue;


    private ExperimentController()
    {
        // Initialize scenes and other necessary data
        allScenes = new List<string>
        {
            "7000K-Normal",             // 1
            "7000K-High",               // 2
            "7000K-Low",                // 3
            "10000K-Normal",            // 4
            "10000K-High",              // 5
            "10000K-Low",               // 6
            "4000K-Normal",             // 7
            "4000K-High",               // 8
            "4000K-Low",                // 9
        };
        DataHandler.Instance.ParticipantID = participantId;
        InitializeSceneQueue();
    }

    private void InitializeSceneQueue()
    {
        sceneQueue = new Queue<string>();

        // Determine the starting index based on participant ID
        int startIndex = (participantId - 1) % allScenes.Count;

        // Populate the queue with the scenes in the counterbalanced order
        for (int i = 0; i < allScenes.Count; i++)
        {
            int sceneIndex = (startIndex + i) % allScenes.Count;
            sceneQueue.Enqueue(allScenes[sceneIndex]);
        }
    }

    public void LoadNextScene()
    {
        TrialManager trialManager = Object.FindObjectOfType<TrialManager>();
        if (trialManager != null && trialManager.isAllTrialDone)
        //if (trialManager != null)
            {
            if (sceneQueue.Count > 0)
            {
                string nextScene = sceneQueue.Dequeue();
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                Debug.Log("All scenes have been completed.");
            }
        }
        else
        {
            Debug.Log("Cannot proceed to next scene, trials not complete.");
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public string GetSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    // Ensure only one instance exists, even after loading new scenes
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoadRuntimeMethod()
    {
        if (_instance != null)
        {
            _instance = new ExperimentController();
        }
    }


    // Add other necessary methods to manage the experiment
}
