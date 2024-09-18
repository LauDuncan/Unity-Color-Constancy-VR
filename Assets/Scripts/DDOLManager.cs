//// 2024-07-16 AI-Tag 
//// This was created with assistance from Muse, a Unity Artificial Intelligence product

//using UnityEngine;
//using System.Collections.Generic;

//public class DDOLManager : MonoBehaviour
//{
//    [Header("Assign Root GameObjects through Inspector")]
//    public static GameObject[] rootObjectsToPreserve;

//    [Header("Specify Root GameObject Names to Find at Runtime")]
//    public static string[] rootObjectNamesToFind;

//    // Dictionary to hold unique instances of GameObjects
//    private static Dictionary<string, GameObject> preservedObjects = new Dictionary<string, GameObject>();

//    void Awake()
//    {
//        DontDestroyOnLoad(this.gameObject);

//        PreserveObjects(rootObjectsToPreserve);
//        FindAndPreserveObjectsByName(rootObjectNamesToFind);
//    }

//    private void PreserveObjects(GameObject[] objects)
//    {
//        foreach (var obj in objects)
//        {
//            if (obj != null)
//            {
//                PreserveSingleInstance(obj);
//            }
//        }
//    }

//    private void FindAndPreserveObjectsByName(string[] names)
//    {
//        foreach (var name in names)
//        {
//            GameObject obj = GameObject.Find(name);
//            if (obj != null)
//            {
//                PreserveSingleInstance(obj);
//            }
//            else
//            {
//                Debug.LogWarning($"Root GameObject with name {name} not found.");
//            }
//        }
//    }

//    void PreserveSingleInstance(GameObject obj)
//    {
//        if (preservedObjects.ContainsKey(obj.name))
//        {
//            if (preservedObjects[obj.name] != obj)
//            {
//                Destroy(obj); // Destroy the duplicate instance
//            }
//        }
//        else
//        {
//            preservedObjects[obj.name] = obj;
//            DontDestroyOnLoad(obj);
//        }
//    }
//}
