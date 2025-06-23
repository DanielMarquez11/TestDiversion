using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class scoreUIManager : MonoBehaviour
{

    public string deliveredCompletionText;
    public GameObject deliveredCompletionHeader;

    public int playerTasksCompleted;
    public int playerTotalTasks;

    public int playerMainTaskStepsCompleted = 0;
    public int playerMainTaskStepsTotal = 0;

    public int playerUrgentTaskStepsCompleted = 0;
    public int playerUrgentTaskStepsTotal = 0;

    // public int numPatients = 0;
    public GameObject[] patientCards;

    public List<string> patientNames = new List<string>();

    public GameObject playerTasksCompletedText;
    public GameObject playerTotalTasksText;
    public GameObject playerMainTaskStepsCompletedText;
    public GameObject playerMainTaskStepsTotalText;
    public GameObject playerMainTaskStepsSlider;
    public GameObject playerUrgentTaskStepsCompletedText;
    public GameObject playerUrgentTaskStepsTotalText;
    public GameObject playerUrgentTaskStepsSlider;

    public GameObject playerOverallPerformanceTextObj;
    public GameObject playerOverallPerformanceStarsBox;

    public string[] playerOverallPerfomanceText = new string[3] { "Poor","Good","Great!" };

    [Range(1, 1200)]
    public int gameTimerSeconds = 600;

    [SerializeField]
    private GameObject playerScoreCard;

    [Header("Leaderboard references:")]
    [SerializeField]
    private bool showLeaderboard;
    [SerializeField]
    private GameObject showLeaderBoardBtn;
    [SerializeField]
    private GameObject leaderboardUI;
    [SerializeField]
    private GameObject leaderboardEntryPrefab;
    [SerializeField]
    private GameObject leaderboardInputPopup;

    [SerializeField]
    private GameObject leaderboardEntryTextBox;
    [SerializeField]
    private GameObject leaderEntriesPanel;

    private float playerScoreTime;
    private bool playerSavedScore = false;
    [SerializeField]
    private bool lockSceneDoorsUntilEnd; //lock scene doors until end of game

    private LevelTransition[] sceneDoorsLocked;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerScoreCard.SetActive(false);
        StartCoroutine(StartTimer());

        if(lockSceneDoorsUntilEnd == true)
        {
            sceneDoorsLocked = FindObjectsByType<LevelTransition>(FindObjectsSortMode.None);
            foreach (LevelTransition sceneDoor in sceneDoorsLocked)
            {
                sceneDoor.isLocked = true; //lock scene doors until end of game
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void completeMainTaskStep()
    {
        playerMainTaskStepsCompleted++;
        checkStepCompletion();
    }

    public void completeUrgentTaskStep()
    {
        playerUrgentTaskStepsCompleted++;
        checkStepCompletion();
    }

    private void checkStepCompletion()
    {
        if(playerMainTaskStepsCompleted == playerMainTaskStepsTotal
            && playerUrgentTaskStepsCompleted == playerUrgentTaskStepsTotal
            && playerTasksCompleted == playerTotalTasks)
        {
            StopAllCoroutines();
            showScoreCard();
            unlockSceneDoors();
        }
    }

    private void unlockSceneDoors()
    {
        if(lockSceneDoorsUntilEnd == true && sceneDoorsLocked != null)
        {
            foreach (LevelTransition sceneDoor in sceneDoorsLocked)
            {
                sceneDoor.isLocked = false; //unlock scene doors
            }
        }
    }

    public void showScoreCard()
    {
        if(deliveredCompletionText != null && deliveredCompletionText != "" && deliveredCompletionHeader != null && deliveredCompletionHeader.GetComponent<TMPro.TextMeshProUGUI>() != null)
        {
            deliveredCompletionHeader.GetComponent<TMPro.TextMeshProUGUI>().text = deliveredCompletionText + ":";
        }

        playerScoreTime = (int)Time.timeSinceLevelLoad; //time left in seconds

        //set text for tasks completed
        playerTasksCompletedText.GetComponent<TMPro.TextMeshProUGUI>().text = playerTasksCompleted.ToString();
        playerTotalTasksText.GetComponent<TMPro.TextMeshProUGUI>().text = "/" + playerTotalTasks.ToString() + " Completed";

        //set text for main taskSteps
        playerMainTaskStepsCompletedText.GetComponent<TMPro.TextMeshProUGUI>().text = playerMainTaskStepsCompleted.ToString();
        playerMainTaskStepsTotalText.GetComponent<TMPro.TextMeshProUGUI>().text = "/" + playerMainTaskStepsTotal.ToString();
        playerMainTaskStepsSlider.GetComponent<UnityEngine.UI.Slider>().maxValue = playerMainTaskStepsTotal;
        playerMainTaskStepsSlider.GetComponent<UnityEngine.UI.Slider>().value = playerMainTaskStepsCompleted;
        

        //set text for urgent taskSteps
        if(playerUrgentTaskStepsTotal == 0)
        {
            playerUrgentTaskStepsTotalText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            playerUrgentTaskStepsCompletedText.GetComponent<TMPro.TextMeshProUGUI>().text = playerUrgentTaskStepsCompleted.ToString();
            playerUrgentTaskStepsTotalText.GetComponent<TMPro.TextMeshProUGUI>().text = "/" + playerUrgentTaskStepsTotal.ToString();
            playerUrgentTaskStepsSlider.GetComponent<UnityEngine.UI.Slider>().maxValue = playerUrgentTaskStepsTotal;
            playerUrgentTaskStepsSlider.GetComponent<UnityEngine.UI.Slider>().value = playerUrgentTaskStepsCompleted;
        }
        
        
        //loop through all patients and get their names and set the card to inactive if is above the number of patients
        for (int i = 0; i < patientCards.Length; i++)
        {
            if (i < patientNames.Count)
            {
                patientCards[i].transform.GetChild(0).GetComponentInChildren<TMPro.TextMeshProUGUI>().text = patientNames[i];
            }
            else
            {
                patientCards[i].SetActive(false);
            }
        }

        if(playerOverallPerformanceStarsBox != null && playerOverallPerformanceTextObj != null)
        {
            if(playerMainTaskStepsCompleted == playerMainTaskStepsTotal && playerUrgentTaskStepsCompleted == playerUrgentTaskStepsTotal && gameTimerSeconds - playerScoreTime > 0)
            {
                playerOverallPerformanceTextObj.GetComponent<TMPro.TextMeshProUGUI>().text = playerOverallPerfomanceText[2];
                //instantiate 3 stars in box
                for(int i = 0; i < playerOverallPerformanceStarsBox.transform.childCount; i++)
                {
                    playerOverallPerformanceStarsBox.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
            else
            {
                if(playerMainTaskStepsCompleted + playerUrgentTaskStepsCompleted >= (playerMainTaskStepsTotal + playerUrgentTaskStepsTotal)/2)
                {
                    playerOverallPerformanceTextObj.GetComponent<TMPro.TextMeshProUGUI>().text = playerOverallPerfomanceText[1];
                    //instantiate 2 stars in box
                    for(int i = 0; i < playerOverallPerformanceStarsBox.transform.childCount-1; i++)
                    {
                        playerOverallPerformanceStarsBox.transform.GetChild(i).gameObject.SetActive(true);
                    }
                }
                else
                {
                    playerOverallPerformanceTextObj.GetComponent<TMPro.TextMeshProUGUI>().text = playerOverallPerfomanceText[0];
                    //instantiate 1 star in box
                    playerOverallPerformanceStarsBox.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            
        }

        if(showLeaderboard && showLeaderBoardBtn != null)
        {
            showLeaderBoardBtn.SetActive(true);
        }

        var itemInteraction = FindFirstObjectByType<ItemInteraction>();
        if(itemInteraction != null)
        {
            itemInteraction.EnableDisableMovement(false);
            itemInteraction.canOpenTaskMenu = false; //disable task menu
        }

        var taskUIManager = FindFirstObjectByType<TaskUIManager>();
        if(taskUIManager != null)
        {
            taskUIManager.gameObject.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerScoreCard.SetActive(true);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(gameTimerSeconds);
        showScoreCard();
    }

    public void showLeaderboardUI()
    {
        Debug.Log("ShowLeaderBoard");
        playerScoreCard.SetActive(false); //turn off scorecard if active
        leaderboardUI.SetActive(true); //turn on leaderboard ui

        if(playerScoreTime != gameTimerSeconds && playerSavedScore  == false)
        {
            leaderboardInputPopup.SetActive(true);
        }
        else{
            EmptyLeaderboardPanel();
            populateLeaderBoard();
        }
    }

    public void exitScoreUI()
    {
        leaderboardUI.SetActive(false); //turn off leaderboard ui
        leaderboardInputPopup.SetActive(false); //turn off input popup
        playerScoreCard.SetActive(false); //turn off scorecard

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        var itemInteraction = FindFirstObjectByType<ItemInteraction>();
        if(itemInteraction != null)
        {
            itemInteraction.EnableDisableMovement(true); //enable player movement
            itemInteraction.canOpenTaskMenu = true; //enable task menu
        }

        var taskUIManager = FindFirstObjectByType<TaskUIManager>(UnityEngine.FindObjectsInactive.Include);
        if(taskUIManager != null)
        {
            taskUIManager.gameObject.SetActive(true);
        }
    }

    public void populateLeaderBoard()
    {
        var entries = LoadLeaderboard();

        for(int i = 0; i < entries.entries.Count; i++)
        {
            ScoreEntry entry = entries.entries[i];
            //create new row object
            GameObject newRow = Instantiate(leaderboardEntryPrefab, leaderEntriesPanel.transform);
            //name to text
            newRow.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = (i + 1).ToString();
            //name to text
            newRow.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = entry.playerName;
            //seconds to text (format?)
            // newRow.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = entry.scoreTime.ToString();
            if(entry.scoreTime > 60)
            {
                string minutes = Mathf.Floor(entry.scoreTime / 60).ToString();
                string seconds = (entry.scoreTime % 60).ToString("00"); // Format seconds to always show two digits
                newRow.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = minutes +":"+ seconds;
            }
            else
            {
                newRow.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = entry.scoreTime.ToString() + "s";
            }
        }
    }

    public void backFromLeaderboardUI()
    {
        leaderboardUI.SetActive(false); //turn off leaderboard ui
        leaderboardInputPopup.SetActive(false); //turn off input popup
        playerScoreCard.SetActive(true); //turn on scorecard if active
    }

    [System.Serializable]
    public class ScoreEntry{
        public string playerName;
        public float scoreTime;

        public ScoreEntry(string name, float time)
        {
            playerName = name;
            scoreTime = time;
        }

    }

    [System.Serializable]
    public class LeaderboardData
    {
        public List<ScoreEntry> entries = new List<ScoreEntry>();
    }

    public void leaderBoardAddEntryBtn()
    {
        if(playerSavedScore == true)
        {
            leaderboardInputPopup.SetActive(false); //turn off input popup
            return;
        }

        var name = leaderboardEntryTextBox.GetComponent<TMPro.TMP_InputField>().text;
        var seconds = playerScoreTime; //time left in seconds
        AddScore(name, seconds); //add score to leaderboard
        playerSavedScore = true; //set player saved score to true
        EmptyLeaderboardPanel(); //clear leaderboard panel before adding new entries
        populateLeaderBoard(); //populate leaderboard with new entries
        leaderboardInputPopup.SetActive(false); //turn off input popup
    }

    private void EmptyLeaderboardPanel()
    {
        // Clear the leaderboard panel before adding new entries
        foreach (Transform child in leaderEntriesPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private const string LeaderboardKey = "HighScores"; // The key used in PlayerPrefs
    private const int MaxEntries = 10; // Maximum number of scores to keep

    // Loads the leaderboard from PlayerPrefs
    public static LeaderboardData LoadLeaderboard()
    {
        string leaderboardKeyString = LeaderboardKey + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; // The key used in PlayerPrefs
        if (PlayerPrefs.HasKey(leaderboardKeyString))
        {
            string json = PlayerPrefs.GetString(leaderboardKeyString);
            // Attempt to deserialize, return empty if it fails
            try
            {
                 LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
                 // Ensure the list isn't null after loading
                 if (data.entries == null) {
                     data.entries = new List<ScoreEntry>();
                 }
                 return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading leaderboard: {ex.Message}. Returning empty leaderboard.");
                return new LeaderboardData(); // Return empty data on error
            }
        }
        else
        {
            return new LeaderboardData(); // Return empty data if key doesn't exist
        }
    }

    // Saves the leaderboard to PlayerPrefs
    private static void SaveLeaderboard(LeaderboardData data)
    {
        // Sort the entries by time (ascending - lower time is better)
        data.entries = data.entries.OrderBy(entry => entry.scoreTime).ToList();

        // Trim the list if it exceeds the maximum number of entries
        // if (data.entries.Count > MaxEntries)
        // {
        //     data.entries = data.entries.GetRange(0, MaxEntries);
        // }

        string leaderboardKeyString = LeaderboardKey + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; // The key used in PlayerPrefs

        // Serialize to JSON and save
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(leaderboardKeyString, json);
        PlayerPrefs.Save(); // IMPORTANT: Ensure data is written to disk immediately
        Debug.Log("Leaderboard saved!");
    }

    // Adds a new score entry to the leaderboard
    public static void AddScore(string playerName, float scoreTime)
    {
        // Load the current leaderboard
        LeaderboardData leaderboard = LoadLeaderboard();

        // Create the new entry
        ScoreEntry newEntry = new ScoreEntry(playerName, scoreTime);

        // Add the new entry
        leaderboard.entries.Add(newEntry);

        // Save the updated leaderboard (SaveLeaderboard handles sorting and trimming)
        SaveLeaderboard(leaderboard);
    }

    // Optional: Clears the leaderboard
    public static void ClearLeaderboard()
    {
        string leaderboardKeyString = LeaderboardKey + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; // The key used in PlayerPrefs
        PlayerPrefs.DeleteKey(leaderboardKeyString);
        PlayerPrefs.Save();
        Debug.Log("Leaderboard cleared!");
    }

     // Optional: Get the current list of entries (read-only)
    public static List<ScoreEntry> GetEntries()
    {
        return LoadLeaderboard().entries;
    }

}
