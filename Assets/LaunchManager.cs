using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Slider
using UnityEngine.Events;

public class LaunchManager : MonoBehaviour
{
    [Header("References")]
    public PaperPlaneController planeController;
    public Slider powerSlider; // Drag your Slider UI element here
    public UnityEvent OnLaunch;

    [Header("Power Bar Settings")]
    [Tooltip("How fast the slider moves from 0 to 1")]
    public float barSpeed = 1.5f;
    [Range(0, 1)]
    [Tooltip("The start of the perfect zone (0 to 1)")]
    public float perfectZoneStart = 0.85f;
    [Range(0, 1)]
    [Tooltip("The end of the perfect zone (0 to 1)")]
    public float perfectZoneEnd = 0.95f;

    private bool isGameActive = true;

    void Update()
    {
        // Don't do anything if the game has ended
        if (!isGameActive) return;

        // Make the slider value move back and forth between 0 and 1
        // Mathf.PingPong is perfect for this!
        powerSlider.value = Mathf.PingPong(Time.time * barSpeed, 1);

        // Check for a single tap (or mouse click for testing)
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            StopGameAndLaunch();
        }
    }

    private void StopGameAndLaunch()
    {
        isGameActive = false; // Stop the update loop from running

        float finalValue = powerSlider.value;
        float launchPower;

        // Check if the player hit the perfect zone
        if (finalValue >= perfectZoneStart && finalValue <= perfectZoneEnd)
        {
            Debug.Log("PERFECT LAUNCH!");
            launchPower = 1.0f; // 100% power
        }
        else
        {
            Debug.Log("Good enough launch.");
            // Power is based on how close they were to the end
            launchPower = finalValue;
        }

        // Tell the plane to launch with the calculated power
        planeController.Launch(launchPower);

        // Trigger the event to hide the UI
        OnLaunch.Invoke();
    }
}