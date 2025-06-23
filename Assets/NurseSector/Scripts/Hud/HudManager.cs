//Metaverse-Solutions Author: Amber Voskamp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HudManager : MonoBehaviour
{
    #region Variables
    private Transform playerBody;

    //Pause Menu
    private GameObject pauseMenu;
    private Button resumeButton;
    private Button pauseSettingsButton;

    //Data collection Panels
    private GameObject welcomePanel;
    private GameObject userAgreementPanel;
    private GameObject privacyPolicyPanel;

    //Data Collection Buttons
    private Button UAButton;
    private Button PPButton;
    private Button continueButton;
    private Button backButtonUserAgreement;
    private Button backButtonPrivacyPolicy;
    private Color lightBlueColor = new Color(0.329f, 0.498f, 0.663f);

    //Setting Panels
    private GameObject settingsPanel;
    private GameObject generalPanel;
    private GameObject controlPanel;
    private GameObject languagePanel;

    //Settings Buttons
    private Button settingsButton;
    private Button generalButton;
    private Button controlButton;
    private Button languageButton;

    //Sensitivity sliders 
    private Slider cameraSensitivitySlider;
    private Slider mouseSensitivitySlider;

    private float mouseSensitivity = 100f;  // Sensitivity for mouse movement
    private Vector2 mouseLookSmooth;  // Store smoothed mouse input
    private Vector2 currentMouseLook;  // Store the current rotation for both axes
    private float smoothing = 5f;  // Smoothing factor (optional)

    private PlayerMovementScript playerMovementScript;
    private ItemInteraction itemInteractionScript;

    private bool isPaused = false;

    #endregion

    void Start()
    {
        if(!playerBody)
        {
            playerBody = GameObject.FindWithTag("Player").transform;
        }


        InitializePauseScreen();
        InitializeDataCollectionPanels();
        InitializeSettingsPanels();
        InitializePlayerMovementScript();
        InitializeButtonListeners();


        settingsPanel.SetActive(false);
        HideAllPanels(); //TODO: make all are hidden already at start - includes reworking find to search for inactive objects or instantiating as inactive

        //StartCoroutine(ShowWelcomePanelWithDelay());

    }
    private void InitializePlayerMovementScript()
    {
        playerMovementScript = playerBody.GetComponentInChildren<PlayerMovementScript>();
        itemInteractionScript = playerBody.GetComponent<ItemInteraction>();

        if(playerMovementScript){
            Debug.Log("PlayerMovementScript found!");

            // Initialize the sliders with the current values from the PlayerMovementScript
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.value = playerMovementScript.mouseSensitivity;
                mouseSensitivitySlider.onValueChanged.AddListener(SetSensitivity);
            }

            if (cameraSensitivitySlider != null)
            {
                cameraSensitivitySlider.value = playerMovementScript.smoothing;
                cameraSensitivitySlider.onValueChanged.AddListener(SetSmoothing);
            }
        }
        else if(itemInteractionScript)
        {
            //TODO: set sensitivity & smoothing in itemInteractionScript look

            //TODO: add p for pause listener to call new onPause
        }

    }

    private void InitializePauseScreen()
    {
        pauseMenu = GameObject.Find("UICanvas/pauseScreen");
        Debug.Log(pauseMenu != null ? "Found pauseMenu" : "pauseMenu not found");

        resumeButton = GameObject.Find("UICanvas/pauseScreen/Image/btns/resumeBtn")?.GetComponent<Button>();
        Debug.Log(resumeButton != null ? "Found resumeButton" : "resumeButton not found");

        pauseSettingsButton = GameObject.Find("UICanvas/pauseScreen/Image/btns/settingsBtn")?.GetComponent<Button>();
        Debug.Log(pauseSettingsButton != null ? "Found pauseSettingsButton" : "pauseSettingsButton not found");

    }
    private void InitializeSettingsPanels()
    {
        #region SettingsPanels references
        // Settings Button
        settingsButton = GameObject.Find("UICanvas/cornerUI/SettingsButton")?.GetComponent<Button>();
        Debug.Log(settingsButton != null ? "Found SettingsButton" : "SettingsButton not found");

        // Buttons inside SettingsButtons
        generalButton = GameObject.Find("UICanvas/SettingsCanva/SettingsPanel/SettingsButtons/General")?.GetComponent<Button>();
        Debug.Log(generalButton != null ? "Found General Button" : "General Button not found");

        controlButton = GameObject.Find("UICanvas/SettingsCanva/SettingsPanel/SettingsButtons/Controls")?.GetComponent<Button>();
        Debug.Log(controlButton != null ? "Found Controls Button" : "Controls Button not found");

        languageButton = GameObject.Find("UICanvas/SettingsCanva/SettingsPanel/SettingsButtons/Language")?.GetComponent<Button>();
        Debug.Log(languageButton != null ? "Found Language Button" : "Language Button not found");


        // Panels
        settingsPanel = GameObject.Find("UICanvas/SettingsCanva/SettingsPanel");
        Debug.Log(settingsPanel != null ? "Found SettingsPanel" : "SettingsPanel not found");

        generalPanel = settingsPanel?.transform.Find("GeneralPanel")?.gameObject;
        Debug.Log(generalPanel != null ? "Found GeneralPanel" : "GeneralPanel not found");

        controlPanel = settingsPanel?.transform.Find("ControlsPanel")?.gameObject;
        Debug.Log(controlPanel != null ? "Found ControlPanel" : "ControlPanel not found");

        languagePanel = settingsPanel?.transform.Find("LanguagePanel")?.gameObject;
        Debug.Log(languagePanel != null ? "Found LanguagePanel" : "LanguagePanel not found");

        // Sliders
        cameraSensitivitySlider = controlPanel?.transform.Find("CameraSensitivityContainer/CameraSmoothingSlider")?.GetComponent<Slider>();
        Debug.Log(cameraSensitivitySlider != null ? "Found CameraSmoothingSlider" : "CameraSmoothingSlider not found");

        mouseSensitivitySlider = controlPanel?.transform.Find("MouseSensitivityContainer/SensitivitySlider")?.GetComponent<Slider>();
        Debug.Log(mouseSensitivitySlider != null ? "Found SensitivitySlider" : "SensitivitySlider not found");
        #endregion
    }
    private void InitializeDataCollectionPanels()
    {
        #region DatacollectionPanels references
        // Find Welcome Panel and its components using GameObject.Find
        welcomePanel = GameObject.Find("DataCollectionCanvas/WelcomePanel");
        Debug.Log(welcomePanel != null ? "Found Welcome Panel" : "Welcome Panel not found");

        UAButton = GameObject.Find("DataCollectionCanvas/WelcomePanel/PopUp/UAButton")?.GetComponent<Button>();
        Debug.Log(UAButton != null ? "Found UAButton" : "UAButton not found");

        PPButton = GameObject.Find("DataCollectionCanvas/WelcomePanel/PopUp/PPButton")?.GetComponent<Button>();
        Debug.Log(PPButton != null ? "Found PPButton" : "PPButton not found");

        continueButton = GameObject.Find("DataCollectionCanvas/WelcomePanel/PopUp/ContinueButton")?.GetComponent<Button>();
        Debug.Log(continueButton != null ? "Found ContinueButton" : "ContinueButton not found");

        // Find User Agreement Panel and its components using GameObject.Find
        userAgreementPanel = GameObject.Find("DataCollectionCanvas/UserAgreementPanel");
        Debug.Log(userAgreementPanel != null ? "Found User Agreement Panel" : "User Agreement Panel not found");

        backButtonUserAgreement = GameObject.Find("DataCollectionCanvas/UserAgreementPanel/PopUp/UAPBackButton")?.GetComponent<Button>();
        Debug.Log(backButtonUserAgreement != null ? "Found BackButton in User Agreement Panel" : "BackButton in User Agreement Panel not found");

        // Find Privacy Policy Panel and its components using GameObject.Find
        privacyPolicyPanel = GameObject.Find("DataCollectionCanvas/PrivacyPolicyPanel");
        Debug.Log(privacyPolicyPanel != null ? "Found Privacy Policy Panel" : "Privacy Policy Panel not found");

        backButtonPrivacyPolicy = GameObject.Find("DataCollectionCanvas/PrivacyPolicyPanel/PopUp/PPPBackButton")?.GetComponent<Button>();
        Debug.Log(backButtonPrivacyPolicy != null ? "Found BackButton in Privacy Policy Panel" : "BackButton in Privacy Policy Panel not found");
        #endregion
    }
    private void InitializeButtonListeners()
    {
        // Initialize button listeners DataCollectionPopUp
        UAButton.onClick.AddListener(ShowUserAgreementPanel);
        PPButton.onClick.AddListener(ShowPrivacyPolicyPanel);
        continueButton.onClick.AddListener(CloseWelcomePanel);
        backButtonUserAgreement.onClick.AddListener(BackToWelcomeFromUserAgreement);
        backButtonPrivacyPolicy.onClick.AddListener(BackToUserAgreementFromPrivacyPolicy);


        // Initialize button listeners settingsPanels 

        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        generalButton.onClick.AddListener(ShowGeneralPanel);
        controlButton.onClick.AddListener(ShowControlPanel);
        languageButton.onClick.AddListener(ShowLanguagePanel);

        // Initialize button listeners pauseMenu
        resumeButton.onClick.AddListener(TogglePause);
        pauseSettingsButton.onClick.AddListener(ToggleSettingsPanel);

        pauseMenu.SetActive(false);
    }

    #region DatacollectionPanels visibility management
    IEnumerator ShowWelcomePanelWithDelay()
    {
        yield return new WaitForSeconds(1f);
        welcomePanel.SetActive(true);
        TogglePause();
    }

    private void ShowUserAgreementPanel()
    {
        HideAllPanels();
        userAgreementPanel.SetActive(true);
    }

    private void ShowPrivacyPolicyPanel()
    {
        HideAllPanels();
        privacyPolicyPanel.SetActive(true);
    }

    private void BackToWelcomeFromUserAgreement()
    {
        HideAllPanels();
        welcomePanel.SetActive(true);

        // Change the UAButton color to light blue
        ColorBlock uaButtonColors = UAButton.colors;
        uaButtonColors.normalColor = lightBlueColor;
        UAButton.colors = uaButtonColors;
    }

    private void BackToUserAgreementFromPrivacyPolicy()
    {
        HideAllPanels();
        welcomePanel.SetActive(true);

        // Change the PPButton color to light blue
        ColorBlock ppButtonColors = PPButton.colors;
        ppButtonColors.normalColor = lightBlueColor;
        PPButton.colors = ppButtonColors;
    }

    private void CloseWelcomePanel()
    {
        privacyPolicyPanel.SetActive(false);
        userAgreementPanel.SetActive(false);
        welcomePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f; // Resume the game
        isPaused = false;
    }
    #endregion
    void Update()
    {
        // Check for pause input
        //TODO: rework to event listener
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }


    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
        if (playerMovementScript != null)
        {
            playerMovementScript.mouseSensitivity = mouseSensitivity;
        }
        else if(itemInteractionScript)
        {
            //TODO: set sensitivity in itemInteractionScript
        }
        Debug.Log("Mouse Sensitivity set to: " + mouseSensitivity);

    }

    public void SetSmoothing(float value)
    {
        smoothing = value;
        if (playerMovementScript != null)
        {
            playerMovementScript.smoothing = smoothing;
        }
        else if(itemInteractionScript)
        {
            //TODO:set smoothing in itemInteractionScript
        }
        Debug.Log("Smoothing set to: " + smoothing);
    }

    private void ToggleSettingsPanel()
    {
        Debug.Log("ToggleSettingsPanel called");
        settingsPanel.SetActive(!settingsPanel.activeSelf);

        if(pauseMenu.activeSelf)
        {
            pauseMenu.SetActive(false);
        }

        if (settingsPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Pause the game
            isPaused = true;
            ShowGeneralPanel(); // Default to General Panel when opening settings
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f; // Resume the game
            isPaused = false;
            HideAllPanels();
        }
    }

    private void ShowGeneralPanel()
    {
        HideAllPanels();
        generalPanel.SetActive(true);
    }

    private void ShowControlPanel()
    {
        HideAllPanels();
        controlPanel.SetActive(true);
    }

    private void ShowLanguagePanel()
    {
        HideAllPanels();
        languagePanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        generalPanel.SetActive(false);
        controlPanel.SetActive(false);
        languagePanel.SetActive(false);
        welcomePanel.SetActive(false);
        userAgreementPanel.SetActive(false);
        privacyPolicyPanel.SetActive(false);
        pauseMenu.SetActive(false);
        pauseMenu.SetActive(false);
    }

    private void TogglePause()
    {
        if (!isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pauseMenu.SetActive(true);
            Time.timeScale = 0f; // Pause the game
            isPaused = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f; // Resume the game
            isPaused = false;
            settingsPanel.SetActive(false);
            HideAllPanels();
        }

        if(itemInteractionScript != null)
        {
            itemInteractionScript.EnableDisableMovement(!isPaused);
        }
    }
}
