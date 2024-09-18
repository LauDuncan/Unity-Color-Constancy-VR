using UnityEngine;

public class TestInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("KEYBOARD INPUT TESTER: A key is pressed.");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("KEYBOARD INPUT TESTER: B key is pressed.");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("KEYBOARD INPUT TESTER: S key is pressed.");
        }
    }
}
