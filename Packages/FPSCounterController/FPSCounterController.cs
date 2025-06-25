using UnityEngine;
using UnityEngine.UI;

public class FPSCounterController : MonoBehaviour
{
    public bool showFPSCounter = true;
    public bool setFrameRate = true;
    public Text fpsCounterText;

    private float deltaTime = 0.0f;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;

        if (setFrameRate)
        {
            Application.targetFrameRate = 60;
        }
    }

    private void Update()
    {
        if (showFPSCounter)
            UpdateFPSCounter();
        else if (fpsCounterText != null)
            fpsCounterText.text = "";
    }

    private void UpdateFPSCounter()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        if (fpsCounterText != null)
            fpsCounterText.text = $"FPS: {Mathf.CeilToInt(fps)}";
    }
}
