using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class TaskUIManager : MonoBehaviour
{
    private int taskHostNumber = 1;
    private string taskHostCaseText = "";
    public TMP_Text taskHostHeaderTemp;
    public TMP_Text taskHostCaseTemp;

    [HideInInspector]
    public TaskListContainer taskListContainer; // Reference to the TaskListContainer
    public GameObject taskHeaderGroupPrefab; // Prefab for the task header group
    public GameObject subtaskHeaderPrefab; // Prefab for the subtask header
    public Transform contentTransform; // Reference to the Scroll View's Content transform
    public Transform contentTransformTaskHostsList; // Reference to the Scroll View's Content transform

    public GameObject interactionSuccessPanel;
    public GameObject interactionFailPanel;

    private TaskListContainer.TaskList.TaskManager taskManager;
    private List<TaskListContainer.TaskList.Task> taskHostTasks;
    private Dictionary<TaskListContainer.TaskList.Task, GameObject> taskHeaderObjects = new Dictionary<TaskListContainer.TaskList.Task, GameObject>();

    [SerializeField]
    private ScrollRect scrollRectTasks;

    [SerializeField]
    private GameObject uiPanel;
    public Image fadeImage;

    [SerializeField]
    private GameObject urgentTaskNotification;

    public GameObject taskHostNameButtonPrefab;

    private Dictionary<int, List<GameObject>> taskHostTaskHeaders = new Dictionary<int, List<GameObject>>();

    public GameObject tooltipCanvas;
    public GameObject pickupTooltip;
    public GameObject dropTooltip;
    public GameObject interactTooltip;
    public List<GameObject> dialogueUi = new List<GameObject>();

    private scoreUIManager scoreUIManager;

    public GameObject tempHeaderMain;
    public GameObject tempHeaderSecondary;

    public GameObject placeItemMenuPanel;
    public List<GameObject> placeItemTileGridContent = new List<GameObject>();
    public GameObject[] itemToPlaceTilePrefab;

    public GameObject SpawnMenuButton;
    public GameObject SpawnMenuButtonSecondary;

    private List<List<GameObject>> spawnObjects = new List<List<GameObject>>();
    private List<GameObject> items = new List<GameObject>();
    private List<GameObject> usedItems = new List<GameObject>(); 
    private ItemInteraction playerItemInteraction;
    [SerializeField, Range(1, 5)] private int maxItemsAmount = 3;
    
    
    void Start()
    {
        //InitializeTaskUI();
        uiPanel.SetActive(false);
        scoreUIManager = FindObjectsByType<scoreUIManager>(FindObjectsSortMode.None).FirstOrDefault();
        playerItemInteraction = GameObject.FindGameObjectWithTag("Player").GetComponent<ItemInteraction>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            ChangeTaskHostNumber(1);
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            ChangeTaskHostNumber(-1);
        }
    }
    
    public void CreateSpawnPanel(SceneTaskManager.SpawnPanelData spawnData)
    {
        if (spawnData == null || spawnData.objectsToSpawn == null)
        {
            return;
        }
        
        spawnObjects.Add(spawnData.objectsToSpawn);
        
        int boxToSpawn = spawnData.boxToSpawn;
        if (boxToSpawn >= placeItemTileGridContent.Count)
        {
            boxToSpawn = placeItemTileGridContent.Count - 1;
        }
        
        for (int i = 0; i < spawnData.objectsToSpawn.Count; i++)
        {
            int toolTypeIndex = (i < spawnData.toolTypeList.Count) ? spawnData.toolTypeList[i] : 0;
            if (toolTypeIndex >= itemToPlaceTilePrefab.Length)
            {
                toolTypeIndex = 0;
            }
            
            GameObject newItem = Instantiate(itemToPlaceTilePrefab[toolTypeIndex], placeItemTileGridContent[boxToSpawn].transform);
            
            Button buttonObject = newItem.GetComponent<Button>() ?? newItem.GetComponentInChildren<Button>();
            if (buttonObject != null)
            {
                buttonObject.onClick.AddListener(() => SpawnButton(buttonObject));
            }
            
            GameObject iconObject = newItem.transform.GetChild(0).transform.GetChild(0).gameObject;
            if (i < spawnData.iconList.Count && spawnData.iconList[i] != null)
            {
                Image iconImage = iconObject.GetComponentInChildren<Image>();
                iconImage.sprite = spawnData.iconList[i];
            }
            else
            {
                iconObject.SetActive(false);
            }
            
            if (i < spawnData.descriptions.Count && spawnData.descriptions[i] != null)
            { 
                TMP_Text descriptionText = newItem.transform.GetComponentsInChildren<TMP_Text>().Last();
                descriptionText.text = spawnData.descriptions[i];
            }
            
            newItem.transform.SetParent(placeItemTileGridContent[boxToSpawn].transform, false);
            TMP_Text headerText = newItem.GetComponentInChildren<TMP_Text>();
            headerText.text = spawnData.objectsToSpawn[i].GetComponent<taskItem>().objectName;
        }
        
        SpawnMenuButton.SetActive(spawnObjects.Count > 0);
    }

    private void SpawnButton(Button spawnButton)
    {
        GameObject objectButton = spawnButton.gameObject;
        GameObject tileObject;
        
        if (placeItemTileGridContent.Contains(objectButton.transform.parent.gameObject))
        {
            tileObject = objectButton;
        }
        else
        {
            tileObject = objectButton.transform.parent.transform.parent.gameObject;
        }
        
        int boxNumber;
        boxNumber = placeItemTileGridContent.IndexOf(tileObject.transform.parent.gameObject);
        GameObject spawnObject = Instantiate(spawnObjects[boxNumber][tileObject.transform.GetSiblingIndex()], new Vector3(0f, 10f, 0f), Quaternion.identity);

        if (playerItemInteraction != null)
        {
            playerItemInteraction.PickUpItem(spawnObject);
            showHideTaskUI();
        }

        items.Add(spawnObject);

        while (items.Count > maxItemsAmount)
        {
            GameObject toRemoveObject = items[0];
            items.RemoveAt(0);
            Destroy(toRemoveObject);
        }
    }
    public void UseItemForStep(GameObject usedItem)
    {
        if (items.Contains(usedItem))
        {
            usedItems.Add(usedItem);
            items.Remove(usedItem);
        }
    }
    public void OnClickMenuButton()
    {
        SpawnMenuButtonSecondary.SetActive(false);
        placeItemMenuPanel.SetActive(true);
    }

    public void OnClickMenuButtonSecondary(bool fullExit = false)
    {
        if (fullExit)
        {
            SpawnMenuButtonSecondary.SetActive(false);
            placeItemMenuPanel.SetActive(false);
        }
        else
        {
            SpawnMenuButtonSecondary.SetActive(true);
            placeItemMenuPanel.SetActive(false);
        }
    }

    private void ChangeTaskHostNumber(int change)
    {
        if (change == 0)
        {
            return;
        }
        int newTaskHostNumber = taskHostNumber + change;
        int maxTaskHostNumber = taskHostTaskHeaders.Keys.Max();
        int minTaskHostNumber = taskHostTaskHeaders.Keys.Min();

        if (newTaskHostNumber >= minTaskHostNumber && newTaskHostNumber <= maxTaskHostNumber)
        {
            taskHostNumber = newTaskHostNumber;
            RefreshTaskUI();
        }
    }

    private void RefreshTaskUI()
    {
        // Hide all task headers
        foreach (var taskHeaders in taskHostTaskHeaders.Values)
        {
            foreach (var taskHeader in taskHeaders)
            {
                taskHeader.SetActive(false);
            }
        }

        //taskHostHeaderTemp.text = "taskHost " + taskHostNumber;
        //Get all taskHosts in the scene, find by number, if they have text as taskHostCase text, set it as text, otherwise set to empty
        var taskHost = FindObjectsByType<taskHost>(FindObjectsSortMode.None).FirstOrDefault(p => p.taskHostNumber == taskHostNumber);
        if (taskHost != null)
        {
            taskHostCaseText = taskHost.taskHostCaseText;
            if(taskHost.taskHostName != "")
            {
                taskHostHeaderTemp.text = taskHost.taskHostName;
            }
            else
            {
                if(taskHost.gameObject.GetComponent<Patient>() != null)
                {
                    taskHostHeaderTemp.text = "Patient " + taskHostNumber;
                }
                else
                {
                    taskHostHeaderTemp.text = "taskHost " + taskHostNumber;
                }
            }
        }
        else
        {
            taskHostCaseText = "";
            taskHostHeaderTemp.text = "taskHost " + taskHostNumber;
        }
        taskHostCaseTemp.text = taskHostCaseText;

        // Show task headers for the current taskHost number
        if (taskHostTaskHeaders.ContainsKey(taskHostNumber))
        {
            foreach (var taskHeader in taskHostTaskHeaders[taskHostNumber])
            {
                taskHeader.SetActive(true);
            }
        }
    }
    public void showHideTaskUI()
    {
        if (taskHostTaskHeaders.Keys.Count == 0)
        {
            return;
        }
        if (placeItemMenuPanel.activeInHierarchy)
        {
            OnClickMenuButtonSecondary(true);
        }
        else
        {
            uiPanel.SetActive(!uiPanel.activeSelf);
        }
        if (uiPanel.activeInHierarchy || placeItemMenuPanel.activeInHierarchy)
        {
            //show mouse
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            //hide mouse
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerItemInteraction != null)
        {
            playerItemInteraction.EnableDisableMovement(!uiPanel.activeSelf);
        }
    }

    public void scrollUI(InputAction.CallbackContext context)
    {
        if (uiPanel.activeSelf)
        {

            float scrollValue = context.ReadValue<Vector2>().y;
            scrollRectTasks.verticalNormalizedPosition += scrollValue * scrollRectTasks.scrollSensitivity * Time.deltaTime;

        }
    }

    public void InitializeTaskUI()
    {

        if (taskListContainer == null)
        {
            Debug.LogError("Task List Container not assigned in TaskUIManager.");
            return;
        }

        taskManager = taskListContainer.taskList.taskManager;

        foreach (var taskHost in taskManager.taskHostToTasks.Keys)
        {
            int taskHostNum = taskHost.taskHostNumber;
            var taskHostNameButton = Instantiate(taskHostNameButtonPrefab, contentTransformTaskHostsList);
            if(taskHost.taskHostName != "")
            {
                taskHostNameButton.GetComponentInChildren<TMP_Text>().text = taskHost.taskHostName;
            }
            else
            {
                if(taskHost.gameObject.GetComponent<Patient>() != null)
                {
                    taskHostNameButton.GetComponentInChildren<TMP_Text>().text = "Patient " + taskHostNum;
                }
                else
                {
                    taskHostNameButton.GetComponentInChildren<TMP_Text>().text = "taskHost " + taskHostNum;
                }
            }
            //taskHostNameButton.GetComponentInChildren<TMP_Text>().text = taskHost.taskHostName;
            taskHostNameButton.GetComponent<Button>().onClick.AddListener(() => OnTaskHostButtonPress(taskHostNameButton));
            taskHostTasks = taskManager.taskHostToTasks[taskHost];

            if (taskHostTasks == null)
            {
                Debug.LogError("No tasks found for taskHost number: " + taskHostNum);
                continue;
            }

            List<GameObject> taskHeaders = new List<GameObject>();

            foreach (var task in taskHostTasks)
            {
                if (task.isUrgent)
                {
                    continue;
                }
                // Create task header
                GameObject taskHeaderGroup = Instantiate(taskHeaderGroupPrefab, contentTransform);
                taskHeaderObjects[task] = taskHeaderGroup; // Store reference to the header GROUP
                taskHeaders.Add(taskHeaderGroup);
                GameObject taskHeader = taskHeaderGroup.transform.Find("taskHeader").gameObject;

                // Set task name in header
                Toggle toggleComponent = taskHeader.GetComponentInChildren<Toggle>();
                TMP_Text taskNameText = taskHeader.GetComponentInChildren<TMP_Text>();
                taskNameText.text = task.taskName;

                // Create subtask headers for initially available steps
                foreach (var step in task.steps)
                {
                    if (taskManager.taskStepAvailability[step].isAvailable)
                    {
                        CreateSubtaskHeader(task, step);
                    }
                }
            }

            taskHostTaskHeaders[taskHostNum] = taskHeaders;
        }

        RefreshTaskUI();
    }

    public void OnTaskHostButtonPress(GameObject btn)
    {
        if (btn.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.gameObject != btn.gameObject).enabled)
        {
            btn.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.gameObject != btn.gameObject).enabled = false;
        }
        int index = btn.transform.GetSiblingIndex() + 1;
        int change = index - taskHostNumber;
        ChangeTaskHostNumber(change);
    }

    public void UpdateTaskUI(TaskListContainer.TaskList.Task.TaskStep completedStep, List<TaskListContainer.TaskList.Task.TaskStep> newlyAvailableSteps)
    {
        TaskListContainer.TaskList.Task taskForStep = taskListContainer.taskList.tasks.FirstOrDefault(t => t.steps.Contains(completedStep));

        Debug.Log("TaskForStep: " + taskForStep.taskName);

        //remove header for the step
        Transform subtaskGroup = taskHeaderObjects[taskForStep].transform.Find("subtaskHeaderGroup");
        foreach (Transform child in subtaskGroup)
        {
            string subtaskText = child.GetComponentInChildren<TMP_Text>().text;
            if (SubtaskHeaderTextMatchesStep(subtaskText, completedStep))
            {
                Debug.Log("removing subtaskHeader: " + subtaskText);
                DestroyImmediate(child.gameObject);
                break;
            }
        }


        //add newly available subtask headers
        foreach (var step in newlyAvailableSteps)
        {
            CreateSubtaskHeader(taskForStep, step);
        }

        Debug.Log("SubtaskGroup Child Count: " + subtaskGroup.childCount);

        if (subtaskGroup.childCount == 0)
        {
            MarkTaskAsComplete(taskForStep);
        }
        else
        {
            foreach (Transform child in subtaskGroup)
            {
                string subtaskText = child.GetComponentInChildren<TMP_Text>().text;
            }
        }

    }

    // Helper method to check if subtask header text matches a step
    private bool SubtaskHeaderTextMatchesStep(string subtaskText, TaskListContainer.TaskList.Task.TaskStep step)
    {
        if(step.stepName != "")
        {
            return subtaskText == step.stepName;
        }
        string itemName = step.itemPrefab ? step.itemPrefab.GetComponent<taskItem>().objectName : "None";
        string locationName = "None";
        if (step.deliveryLocationPrefab)
        {
            var taskHostComponent = step.deliveryLocationPrefab.GetComponent<taskHost>();
            if (taskHostComponent != null)
            {
                if(taskHostComponent.taskHostName != "")
                {
                    locationName = taskHostComponent.taskHostName;
                }
                else{
                    locationName = $"taskHost {taskHostComponent.taskHostNumber}";
                }
            }

            var patientComponent = step.deliveryLocationPrefab.GetComponent<Patient>();
            if (patientComponent != null && taskHostComponent != null)
            {
                int patientNum = taskHostComponent.taskHostNumber;
                locationName = $"Patient {patientNum}";
            }
            else
            {
                var taskItemComponent = step.deliveryLocationPrefab.GetComponent<taskItem>();
                if (taskItemComponent != null)
                {
                    locationName = taskItemComponent.objectName;
                }
            }
        }
        string expectedText = $"{itemName} -> {locationName}";
        return subtaskText == expectedText;
    }

    private void MarkTaskAsComplete(TaskListContainer.TaskList.Task completedTask)
    {
        //if it is currentUrgentTask mark that as null
        if (taskManager.activeUrgentTask != null && taskManager.activeUrgentTask == completedTask)
        {
            taskManager.activeUrgentTask = null;
            taskHost taskHost = taskManager.taskHostToTasks.FirstOrDefault(p => p.Value.Contains(completedTask)).Key;
            urgentTaskNotification.SetActive(false);
            var taskHostBtn = contentTransformTaskHostsList.GetChild(taskHost.taskHostNumber - 1);
            if (taskHostBtn.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.gameObject != taskHostBtn.gameObject).enabled)
            {
                taskHostBtn.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.gameObject != taskHostBtn.gameObject).enabled = false;
            }
        }

        GameObject taskHeader = taskHeaderObjects[completedTask].transform.Find("taskHeader").gameObject;
        Toggle toggleComponent = taskHeader.GetComponentInChildren<Toggle>();
        TMP_Text taskNameText = taskHeader.GetComponentInChildren<TMP_Text>();

        // Mark checkbox and change text color
        toggleComponent.isOn = true;
        taskNameText.color = Color.gray;
        taskHeaderObjects[completedTask].transform.SetAsLastSibling();

        if(scoreUIManager != null)
        {
            scoreUIManager.playerTasksCompleted++;
        }
    }

    private void CreateSubtaskHeader(TaskListContainer.TaskList.Task task, TaskListContainer.TaskList.Task.TaskStep step)
    {
        GameObject subtaskHeader = Instantiate(subtaskHeaderPrefab, taskHeaderObjects[task].transform.Find("subtaskHeaderGroup"));
        TMP_Text subtaskText = subtaskHeader.GetComponentInChildren<TMP_Text>();
        if(step.stepName != "")
        {
            subtaskText.text = step.stepName;
            return;
        }

        string itemName = step.itemPrefab ? step.itemPrefab.GetComponent<taskItem>().objectName : "None";
        string locationName = "None";
        if (step.deliveryLocationPrefab)
        {
            var taskHostComponent = step.deliveryLocationPrefab.GetComponent<taskHost>();
            if (taskHostComponent != null)
            {
                if(taskHostComponent.taskHostName != "")
                {
                    locationName = taskHostComponent.taskHostName;
                }
                else{
                    locationName = $"taskHost {taskHostComponent.taskHostNumber}";
                }
            }

            var patientComponent = step.deliveryLocationPrefab.GetComponent<Patient>();
            if (patientComponent != null && taskHostComponent != null)
            {
                int patientNum = taskHostComponent.taskHostNumber;
                locationName = $"Patient {patientNum}";
            }
            else
            {
                var taskItemComponent = step.deliveryLocationPrefab.GetComponent<taskItem>();
                if (taskItemComponent != null)
                {
                    locationName = taskItemComponent.objectName;
                }
            }
        }
        subtaskText.text = $"{itemName} -> {locationName}";
    }

    public void ActivateUrgentTaskInUI(TaskListContainer.TaskList.Task urgentTask)
    {
        if (urgentTask == null)
        {
            return;
        }

        // Create the task header group
        GameObject taskHeaderGroup = Instantiate(taskHeaderGroupPrefab, contentTransform);
        taskHeaderObjects[urgentTask] = taskHeaderGroup;
        GameObject taskHeader = taskHeaderGroup.transform.Find("taskHeader").gameObject;

        // Set task name in header
        Toggle toggleComponent = taskHeader.GetComponentInChildren<Toggle>();
        TMP_Text taskNameText = taskHeader.GetComponentInChildren<TMP_Text>();
        taskNameText.text = urgentTask.taskName;
        taskNameText.color = Color.red;

        taskHeaderGroup.transform.SetAsFirstSibling();

        taskHost taskHost = taskManager.taskHostToTasks.FirstOrDefault(p => p.Value.Contains(urgentTask)).Key;

        if(taskHost.gameObject.GetComponent<Patient>())
        {
            taskHost.gameObject.GetComponent<Patient>().BeginUrgentTask();
        }

        int taskHostNum = taskHost.taskHostNumber;
        string taskHostName = taskHost.taskHostName;

        if (!taskHostTaskHeaders.ContainsKey(taskHostNum))
        {
            taskHostTaskHeaders[taskHostNum] = new List<GameObject>();
        }
        taskHostTaskHeaders[taskHostNum].Add(taskHeaderGroup);

        urgentTaskNotification.SetActive(true);
        if(taskHostName != "")
        {
            urgentTaskNotification.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = taskHostName;
        }
        else
        {
            if(taskHost.GetComponent<Patient>() != null)
            {
                urgentTaskNotification.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = $"Patient {taskHostNum}";
            }
            else
            {
                urgentTaskNotification.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = $"taskHost {taskHostNum}";
            }
        }
        //urgentTaskNotification.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = $"taskHost {taskHostNum}";

        Transform taskHostButtonTransform = contentTransformTaskHostsList.GetChild(taskHostNum - 1);
        if (taskHostButtonTransform != null)
        {
            if (!taskHostButtonTransform.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.gameObject != taskHostButtonTransform.gameObject).enabled)
            {
                taskHostButtonTransform.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.gameObject != taskHostButtonTransform.gameObject).enabled = true;
            }
        }

        taskHeaderObjects[urgentTask].SetActive(true);
        foreach (var step in urgentTask.steps)
        {
            if (taskManager.taskStepAvailability[step].isAvailable)
            {
                CreateSubtaskHeader(urgentTask, step);
            }
        }

        RefreshTaskUI();

    }
    public void ShowInteractionPanel(bool success)
    {
        interactionSuccessPanel.SetActive(false);
        interactionFailPanel.SetActive(false);
        StopAllCoroutines();
        GameObject panel = success ? interactionSuccessPanel : interactionFailPanel;
        panel.SetActive(true);
        if(this.gameObject.activeInHierarchy)
        {
            StartCoroutine(HideInteractionPanel(panel));
        }
    }

    private IEnumerator HideInteractionPanel(GameObject panel)
    {
        yield return new WaitForSeconds(2);
        panel.SetActive(false);
    }
}