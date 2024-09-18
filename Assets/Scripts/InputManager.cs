using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public TrialManager TrialManager;
    public GameObject menuCanvas;
    public GameObject devCanvas;

    public InputActionReference matchChangeInput;
    //public InputActionReference changeInput;
    public InputActionReference menuToggleButton;

    public AudioSource audioSource;
    public AudioClip perceivedMatchClip;
    public AudioClip perceivedChangeClip;

    // Start is called before the first frame update
    void Start()
    {
        matchChangeInput.action.Enable();
        matchChangeInput.action.performed += handleSingleButton;

        //changeInput.action.Enable();
        //changeInput.action.performed += inputChange;

        menuToggleButton.action.Enable();
        menuToggleButton.action.performed += ToggleMenu;
    }

    //private void inputMatch(InputAction.CallbackContext obj)
    //{
    //    //Debug.Log("RIGHT trigger pressed");
    //    if (TrialManager.isTrialRunning)
    //    {
    //        bool validInput = TrialManager.PerceivedMatch();
    //        if (validInput) PlayAudioClip(perceivedMatchClip);
    //    }
    //}

    //private void inputChange(InputAction.CallbackContext obj)
    //{
    //    //Debug.Log("LEFT trigger pressed");
    //    if (TrialManager.isTrialRunning)
    //    {
    //        bool validInput = TrialManager.PerceivedChange();
    //        if (validInput) PlayAudioClip(perceivedChangeClip);
    //    }
    //}

    private void handleSingleButton(InputAction.CallbackContext obj)
    {
        // comment below out if pause duration is user defined
        if (TrialManager.isPaused)
        {
            TrialManager.UnpauseTrial();
            return;
        }


        if (TrialManager.isTrialRunning)
        {
            bool validInput = TrialManager.PerceivedMatch();
            if (validInput) PlayAudioClip(perceivedChangeClip);
        }
    }


    private void ToggleMenu(InputAction.CallbackContext obj)
    {
        menuCanvas.SetActive(!menuCanvas.activeSelf);
    }

    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("KEYBOARD INPUT: A key is pressed.");
            if (TrialManager.isPaused)
            {
                TrialManager.UnpauseTrial();
                return;
            }


            if (TrialManager.isTrialRunning)
            {
                bool validInput = TrialManager.PerceivedMatch();
                if (validInput) PlayAudioClip(perceivedChangeClip);
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("KEYBOARD INPUT: S key is pressed.");
            TrialManager.StartTrial();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("KEYBOARD INPUT: N key is pressed.");
            ExperimentController.Instance.LoadNextScene();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("KEYBOARD INPUT: R key is pressed.");
            ExperimentController.Instance.ReloadScene();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("KEYBOARD INPUT: D key is pressed.");
            devCanvas.SetActive(!devCanvas.activeSelf);
        }
    }


}
