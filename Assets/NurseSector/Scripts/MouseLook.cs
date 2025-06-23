using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;  // Sensitivity for mouse movement
    public float smoothing = 5f;  // Smoothing factor (optional)

    public Transform playerBody;  // The player's body, which will rotate horizontally

    private Vector2 currentMouseLook;  // Store the current rotation for both axes
    private Vector2 mouseLookSmooth;  // Store smoothed mouse input
   // private float xRotation = 0f;  // Vertical rotation of the camera

    private float minSensitivity = 10f;
    private float maxSensitivity = 120f;

    public Slider MouseSensitivitySlider; // Reference to the UI Slider
    public GameObject settingsPanel; // Reference to the Settings Panel
    public Button settingsButton;
    public Slider cameraSensitivitySlider;

    public GameObject controlPanel;
    public Button controlButton;

    public bool areArmsActive = false;  // To check if arms are active
    public float maxDownwardAngle = 30f;  // The max angle to look down when arms are active

    private bool isPaused = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // Lock the cursor to the screen
        Cursor.visible = false;

        if (MouseSensitivitySlider != null)
        {
            MouseSensitivitySlider.minValue = minSensitivity; // Set min value for slider
            MouseSensitivitySlider.maxValue = maxSensitivity; // Set max value for slider
            MouseSensitivitySlider.value = mouseSensitivity; // Set initial slider value to current sensitivity
            MouseSensitivitySlider.onValueChanged.AddListener(SetSensitivity); // Add listener for changes
        }



        // Add listener for smoothing slider
        if (cameraSensitivitySlider != null)
        {
            cameraSensitivitySlider.minValue = 1f; // Minimum smoothing value
            cameraSensitivitySlider.maxValue = 10f; // Maximum smoothing value
            cameraSensitivitySlider.value = smoothing; // Set initial slider value to current smoothing
            cameraSensitivitySlider.onValueChanged.AddListener(SetSmoothing); // Add listener for changes
        }

        // Add listener for settings button
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ToggleSettingsPanel);
        }

        settingsPanel.SetActive(false); // Hide the se

        // Add listener for settings button
        if (controlButton != null)
        {
            controlButton.onClick.AddListener(ToggleControlPanel);
        }

        controlPanel.SetActive(false); // Hide the se

    }

    void Update()
    {
        if(!isPaused)
        {
            // Get raw mouse input
            Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // Apply mouse sensitivity and smoothing (optional)
            mouseInput = Vector2.Scale(mouseInput, new Vector2(mouseSensitivity * smoothing * Time.deltaTime, mouseSensitivity * smoothing * Time.deltaTime));

            // Smooth the input
            mouseLookSmooth.x = Mathf.Lerp(mouseLookSmooth.x, mouseInput.x, 1f / smoothing);
            mouseLookSmooth.y = Mathf.Lerp(mouseLookSmooth.y, mouseInput.y, 1f / smoothing);

            // Update the current mouse look
            currentMouseLook.x += mouseLookSmooth.x;
            currentMouseLook.y += mouseLookSmooth.y;

            if (areArmsActive)
            {
                currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -maxDownwardAngle, 90f);  // Clamp downward to the specified max angle when arms are active
            }
            else
            {
                currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -90f, 90f);  // Normal clamp when arms are not active
            }

            playerBody.localRotation = Quaternion.Euler(0f, currentMouseLook.x, 0f);
            transform.localRotation = Quaternion.Euler(-currentMouseLook.y, 0f, 0f);
        }

        // Check for pause input
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
        Debug.Log("Mouse Sensitivity set to: " + mouseSensitivity);
    }

    public void SetSmoothing(float value)
    {
        smoothing = value;
        Debug.Log("Smoothing set to: " + smoothing);
    }

    private void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);

        if (settingsPanel.activeSelf)
        {
            controlPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Pause the game
            isPaused = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f; // Resume the game
            isPaused = false;
        }
    }

    private void ToggleControlPanel()
    {
        controlPanel.SetActive(!controlPanel.activeSelf);

        if (controlPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Pause the game
            isPaused = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f; // Resume the game
            isPaused = false;
        }
    }

    private void TogglePause()
    {
        if (!isPaused)
        {
            // Pause the game
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Pause the game
            isPaused = true;
        }
        else
        {
            // Resume the game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f; // Resume the game
            isPaused = false;
            settingsPanel.SetActive(false);
            controlPanel.SetActive(false);
        }
    }


}