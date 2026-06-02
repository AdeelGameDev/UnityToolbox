using UnityEngine;

public class PerformanceManager : MonoBehaviour
{
    private void Awake()
    {
        // Disable VSync to allow manual frame rate control
        QualitySettings.vSyncCount = 0;

        // Lock the game to 60 FPS
        Application.targetFrameRate = 60;

        // Prevent the device from going to sleep
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
