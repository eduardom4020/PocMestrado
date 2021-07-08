using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CameraDisabler : MonoBehaviour
{
    private static GameObject ARCamera;
    private static GameObject EditorCamera;

    static CameraDisabler()
    {
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
        EditorCamera ??= GameObject.Find("EditorCamera");
        ARCamera ??= GameObject.Find("AR Camera");

        if (EditorCamera != null && EditorCamera.activeSelf)
        {
            if (ARCamera != null && ARCamera.activeSelf)
                ARCamera.SetActive(false);
        }
        else
        {
            if (ARCamera != null && !ARCamera.activeSelf)
                ARCamera.SetActive(true);
        }
        
    }
}
