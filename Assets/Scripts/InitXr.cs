using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;
 
public class InitXr : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(StartXR());
    }
 
    IEnumerator StartXR()
    {
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
            yield return null;
        }
    }
}