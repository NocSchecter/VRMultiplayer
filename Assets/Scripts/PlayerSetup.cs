using UnityEngine.Networking;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
using UnityEngine.SpatialTracking;
using TMPro;

public class PlayerSetup : NetworkBehaviour
{
    public Camera vrCmamera;
    public AudioListener audioListener;

    public TrackedPoseDriver trackedPoseDriver;

    public TextMeshProUGUI playerName;

    private void Start() 
    {
        DisableVRComponents();
    }

    private void DisableVRComponents()
    {
        if (!isLocalPlayer)
        {
            vrCmamera.enabled = false;
            audioListener.enabled = false;
            trackedPoseDriver.enabled = false;
        }
    }
}
