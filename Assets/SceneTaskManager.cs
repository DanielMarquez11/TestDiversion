using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;


public class SceneTaskManager : MonoBehaviour
{
    [Tooltip("The default taskHost prefab to spawn.")]
    public GameObject taskHostPrefab;

    [Tooltip("The name of the taskHost type. Shown in the taskUI as header of taskHost list box.")]
    [SerializeField]
    private string taskHostTypeName = "Task Host";
    [Tooltip("The taskHost prefab to spawn. Including limits per task type and custom prefab option.")]
    public taskHostToSpawn[] taskHostsToSpawn;
    [Tooltip("The range of time in seconds for the urgent task cooldown.")]
    public Vector2 urgentCooldownRange = new Vector2(20f, 60f);
    public TaskListContainer sceneTaskList;

    [System.Serializable]
    public class itemsFromUI
    {
        public GameObject itemPrefab;
        public Sprite itemIcon;

        public enum ToolType
        {
            Box1, 
            Box2
        }

        public enum Types
        {
            Installation,
            Tool
        }

        public ToolType toolType;
        public Types type;
        [TextArea(1, 3)] public string itemInfo;
    }
    
    public class SpawnPanelData
    {
        public List<GameObject> objectsToSpawn;
        public List<Sprite> iconList;
        public List<int> toolTypeList;
        public int boxToSpawn;
        public List<string> descriptions;
        
        public SpawnPanelData(List<GameObject> objects, List<Sprite> icons, List<int> types, int box, List<string> desc)
        {
            objectsToSpawn = objects;
            iconList = icons;
            toolTypeList = types;
            boxToSpawn = box;
            descriptions = desc;
        }
    }


    [Tooltip("The items that are able to be spawned by the player from the UI item spawn menu. If empty, the menu will not be accessible.")]
    public itemsFromUI[] itemsFromUIList;

    private TaskUIManager taskUIManager;

    private TaskListContainer.TaskList.TaskManager taskManager;

    private scoreUIManager scoreUIManager;

    [System.Serializable]
    public class taskHostToSpawn
    {
        [Header("Task Limits")]
        [Tooltip("The limit of direct tasks that can be assigned to the taskHost.")]
        public int DirectTaskLimit = 1;
        [Tooltip("The limit of multi-step tasks that can be assigned to the taskHost.")]
        public int MultiTaskLimit = 1;
        [Tooltip("The limit of urgent tasks that can be assigned to the taskHost.")]
        public int UrgentTaskLimit = 1;
        [Tooltip("The custom patient prefab to spawn. Leave empty to use the default taskHost prefab.")]

        [Header("Custom TaskHost Info")]
        public GameObject CustomTaskHostPrefab;
        [Tooltip("The TaskHost name.")]
        public string taskHostName;
        [Tooltip("The TaskHost case text.")]
        public string taskHostCaseText;

        [Tooltip("Should the taskHost prefab exist in the scene before build/compile (to allow it to work with baked lighting)?")]
        public bool spawnFromScene;

        [HideInInspector]
        public GameObject sceneTaskHostObjTemp;

    }

    [HideInInspector]
    public List<taskHost> taskHosts = new List<taskHost>();

    void Start()
    {   
        taskUIManager = FindObjectsByType<TaskUIManager>(FindObjectsSortMode.None).FirstOrDefault();
        if (taskUIManager == null)
        {
            Debug.LogError("Failed to find TaskUIManager.");
            return;
        }

        if (taskHostsToSpawn.Length == 0)
        {
            Debug.LogError("No taskHosts to spawn.");
            return;
        }

        scoreUIManager = FindFirstObjectByType<scoreUIManager>();
        if (scoreUIManager == null)
        {
            Debug.LogError("Failed to find scoreUIManager.");
            return;
        }

        // Spawn taskHost
        for (int i = 0; i < taskHostsToSpawn.Length && i < transform.childCount; i++)
        {
            GameObject taskHostToSpawn = taskHostsToSpawn[i].CustomTaskHostPrefab != null
                ? taskHostsToSpawn[i].CustomTaskHostPrefab
                : taskHostPrefab;

            var taskHostParent = transform.GetChild(i);

            foreach (Transform child in transform.GetChild(i).transform)
            {
                if (child.CompareTag("anchorpoint"))
                {
                    taskHostParent = child;
                    break;
                }
            }

            if(taskHostsToSpawn[i].spawnFromScene)
            {
                if(taskHostsToSpawn[i].sceneTaskHostObjTemp != null){
                    taskHosts.Add(taskHostsToSpawn[i].sceneTaskHostObjTemp.GetComponent<taskHost>());
                }
                else{
                    Debug.LogWarning($"taskHost {taskHostToSpawn.name} is set spawnFromScene true and sceneTaskHostObjTemp is null - error: instantiating prefab in scene at runtime (This may affect the taskHost objects appearance due to lighting - return to editor mode and press 'Update Scene Task Hosts' on the Scene Task Manager)");
                    taskHosts.Add(Instantiate(taskHostToSpawn, taskHostParent).GetComponent<taskHost>());
                }
            }
            else{
                taskHosts.Add(Instantiate(taskHostToSpawn, taskHostParent).GetComponent<taskHost>());
            }
            
            taskHosts[i].taskHostNumber = i + 1;

            if(scoreUIManager != null)
            {
                scoreUIManager.patientNames.Add(taskHostsToSpawn[i].taskHostName);
            }

            if(taskHostsToSpawn[i].taskHostName != null)
            {
                taskHosts[i].taskHostName = taskHostsToSpawn[i].taskHostName;
            }

            if(taskHostsToSpawn[i].taskHostCaseText != null)
            {
                taskHosts[i].taskHostCaseText = taskHostsToSpawn[i].taskHostCaseText;
            }

            if(taskHosts[i].GetComponent<Patient>() != null && gameObject.GetComponent<PatientSpawner>())
            {
                taskHosts[i].GetComponent<Patient>().UpdatePatientData();
                gameObject.GetComponent<PatientSpawner>().posePatientParts(taskHosts[i].GetComponent<Patient>(), gameObject.GetComponent<PatientSpawner>().patientsToSpawn[i]);
                gameObject.GetComponent<PatientSpawner>().clothePatient(taskHosts[i].GetComponent<Patient>(), gameObject.GetComponent<PatientSpawner>().patientsToSpawn[i]);

                if(taskHostsToSpawn[i].taskHostName != null)
                {
                    taskHosts[i].gameObject.GetComponent<Patient>().patientName = taskHostsToSpawn[i].taskHostName;
                }

                if(taskHostsToSpawn[i].taskHostCaseText != null)
                {
                    taskHosts[i].gameObject.GetComponent<Patient>().patientCaseText = taskHostsToSpawn[i].taskHostCaseText;
                }
            }
        }

        // Instantiate the TaskListContainer and initialize the task step tree
        if (sceneTaskList == null)
        {
            Debug.LogError("sceneTaskList is not assigned.");
            return;
        }

        var taskListContainerInstance = Instantiate(sceneTaskList);
        if (taskListContainerInstance == null)
        {
            Debug.LogError("Failed to instantiate TaskListContainer.");
            return;
        }

        DistributeTasks(taskListContainerInstance); // Distribute tasks before creating the task step tree

        // Get the TaskManager from the TaskListContainer
        taskManager = taskListContainerInstance.taskList.taskManager;
        if (taskManager == null)
        {
            Debug.LogError("TaskManager is not initialized.");
            return;
        }

        // Build the task tree and initialize availability
        taskManager.buildTaskTree();
        taskManager.initializeAvailability();

        // Log the task tree
        //taskManager.LogTaskTree(); // Call initially to log the tree
        taskManager.LogActiveTaskSteps();

        // Assign the TaskListContainer and TaskManager to ItemInteraction
        var itemInteraction = FindFirstObjectByType<ItemInteraction>();
        if (itemInteraction == null)
        {
            Debug.LogError("Failed to find ItemInteraction.");
            return;
        }

        itemInteraction.TaskListContainerInstance = taskListContainerInstance;
        itemInteraction.taskManager = taskManager; // Assign the TaskManager directly

        taskUIManager.taskListContainer = taskListContainerInstance;
        taskUIManager.InitializeTaskUI();

        if(gameObject.GetComponent<PatientSpawner>() != null)
        {
            taskUIManager.tempHeaderMain.GetComponent<TMP_Text>().text = "Patient Profile:";
            taskHostTypeName = "Patients";
        }
        else
        {
            taskUIManager.tempHeaderMain.GetComponent<TMP_Text>().text = "Profile:";
        }

        taskUIManager.tempHeaderSecondary.GetComponent<TMP_Text>().text = taskHostTypeName + ":";

        // Set initial timer with random value within the range
        taskManager.urgentTaskTimer = Random.Range(urgentCooldownRange.x, urgentCooldownRange.y);
        
        separateItemsToSpawnToLists();
        
    }

    void Update()
    {
        taskManager.UpdateUrgentTasks(Time.deltaTime);
    }

    // Method to distribute tasks to taskHosts
    void DistributeTasks(TaskListContainer taskListContainerInstance)
    {
        var taskList = taskListContainerInstance.taskList;

        // Populate taskHostToTasks in TaskManager
        TaskListContainer.TaskList.TaskManager taskManager = new TaskListContainer.TaskList.TaskManager(taskListContainerInstance.taskList.tasks.ToList());
        taskManager.taskHostToTasks = new Dictionary<taskHost, List<TaskListContainer.TaskList.Task>>();

        // Assign the taskManager to the taskListContainerInstance
        taskListContainerInstance.taskList.taskManager = taskManager;

        // Initialize taskHostToTasks with empty lists for each taskHost
        foreach (taskHost taskHost in taskHosts)
        {
            taskManager.taskHostToTasks[taskHost] = new List<TaskListContainer.TaskList.Task>();
        }

        // Sort tasks by preferred taskHost number and then by weight
        var sortedTasks = taskList.tasks
            .OrderBy(t => t.preferredTaskHostNumber == 0 ? int.MaxValue : t.preferredTaskHostNumber)
            .ThenByDescending(t => t.weight)
            .ToList();

        // Distribute tasks to taskHost based on preferred taskHost number and weight
        foreach (var task in sortedTasks)
        {
            bool taskAssigned = false;

            // Check if the preferred taskHost number is valid
            int preferredTaskHostIndex = task.preferredTaskHostNumber - 1;
            if (preferredTaskHostIndex >= 0 && preferredTaskHostIndex < taskHosts.Count)
            {
                taskHost preferredTaskHost = taskHosts[preferredTaskHostIndex];
                taskHostToSpawn taskHostLimits = taskHostsToSpawn[preferredTaskHostIndex];

                if (CanAssignTask(task, preferredTaskHost, taskHostLimits, taskManager))
                {
                    AssignTaskToTaskHost(task, preferredTaskHost, taskListContainerInstance, taskManager);
                    taskAssigned = true;
                }
            }

            // If task is not assigned to preferred taskHost, use round-robin technique
            if (!taskAssigned)
            {
                for (int i = 0; i < taskHosts.Count; i++)
                {
                    int index = (i + preferredTaskHostIndex + 1) % taskHosts.Count; // Round-robin index
                    taskHost currentTaskHost = taskHosts[index];
                    taskHostToSpawn taskHostLimits = taskHostsToSpawn[index];

                    if (CanAssignTask(task, currentTaskHost, taskHostLimits, taskManager))
                    {
                        AssignTaskToTaskHost(task, currentTaskHost, taskListContainerInstance, taskManager);
                        taskAssigned = true;
                        break;
                    }
                }
            }
        }

        taskListContainerInstance.taskList.taskManager = taskManager;
    }

    // Helper method to check if a task can be assigned to a taskHost
    bool CanAssignTask(TaskListContainer.TaskList.Task task, taskHost taskHost, taskHostToSpawn taskHostLimits, TaskListContainer.TaskList.TaskManager taskManager)
    {
        if (task.steps.Count == 1 && !task.isUrgent &&
            taskManager.taskHostToTasks[taskHost].Count(t => t.steps.Count == 1 && !t.isUrgent) < taskHostLimits.DirectTaskLimit)
        {
            return true;
        }
        else if (task.steps.Count > 1 && !task.isUrgent &&
                 taskManager.taskHostToTasks[taskHost].Count(t => t.steps.Count > 1 && !t.isUrgent) < taskHostLimits.MultiTaskLimit)
        {
            return true;
        }
        else if (task.isUrgent &&
                 taskManager.taskHostToTasks[taskHost].Count(t => t.isUrgent) < taskHostLimits.UrgentTaskLimit)
        {
            return true;
        }
        return false;
    }

    // Helper method to assign a task to a taskHost
    void AssignTaskToTaskHost(TaskListContainer.TaskList.Task task, taskHost taskHost, TaskListContainer taskListContainerInstance, TaskListContainer.TaskList.TaskManager taskManager)
    {
        foreach (var step in task.steps)
        {
            if (step.itemPrefab != null)
            {
                step.itemPrefab = taskListContainerInstance.taskList.FindNestedObject(step.itemPrefab, true);
            }
            if (step.deliveryLocationPrefab != null)
            {
                step.deliveryLocationPrefab = taskListContainerInstance.taskList.FindNestedObject(step.deliveryLocationPrefab, false);
            }
            else if (step.deliveryLocationPrefab == null)
            {
                step.deliveryLocationPrefab = taskHost.gameObject;
            }
        }
        taskManager.taskHostToTasks[taskHost].Add(task);

        if(scoreUIManager != null)
        {
            scoreUIManager.playerTotalTasks++;
        }

        if (task.isUrgent)
        {
            taskManager.urgentTaskQueue.Add(task);
            if(scoreUIManager != null)
            {
                scoreUIManager.playerUrgentTaskStepsTotal += task.steps.Count;
            }
        }
        else{
            if(scoreUIManager != null)
            {
                scoreUIManager.playerMainTaskStepsTotal += task.steps.Count;
            }
        }
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        foreach(taskHostToSpawn taskHost in taskHostsToSpawn)
        {
            taskHost myTaskHost = null;
            if(taskHost.CustomTaskHostPrefab == null)
            {
                myTaskHost = taskHostPrefab.GetComponent<taskHost>();

            }
            else
            {
                myTaskHost = taskHost.CustomTaskHostPrefab.GetComponent<taskHost>();
            }

            if(myTaskHost == null)
            {
                Debug.LogError("TaskHost prefab does not have a taskHost component.");
                return;
            }
        }
    }
    
    private void separateItemsToSpawnToLists()
    {
        int boxCount = System.Enum.GetValues(typeof(itemsFromUI.ToolType)).Length;

        List<List<GameObject>> itemList = new List<List<GameObject>>(boxCount);
        List<List<Sprite>> spriteList = new List<List<Sprite>>(boxCount);
        List<List<int>> toolTypeList = new List<List<int>>(boxCount);
        List<List<string>> descriptionList = new List<List<string>>(boxCount);

        for(int i = 0; i < boxCount; i++)
        {
            itemList.Add(new List<GameObject>());
            spriteList.Add(new List<Sprite>());
            toolTypeList.Add(new List<int>());
            descriptionList.Add(new List<string>());
        }

        foreach(var item in itemsFromUIList)
        {
            if(item.itemPrefab != null)
            {
                itemList[(int)item.toolType].Add(item.itemPrefab);
                if(item.itemIcon != null)
                {
                    spriteList[(int)item.toolType].Add(item.itemIcon);
                }
                else
                {
                    spriteList[(int)item.toolType].Add(null);
                }

                if (item.itemInfo != null && item.type == itemsFromUI.Types.Tool)
                {
                    descriptionList[(int)item.toolType].Add(item.itemInfo);
                }
                else
                {
                    descriptionList[(int)item.toolType].Add(null);
                }
            }
            toolTypeList[(int)item.toolType].Add((int)item.type);
        }

        for(int i = 0; i < itemList.Count; i++)
        {
            // Create panel data for each box type
            SpawnPanelData spawnPanelData = new SpawnPanelData(
                itemList[i], 
                spriteList[i], 
                toolTypeList[i], 
                i,
                descriptionList[i]
            );
            
            CreateSpawnItemsList(spawnPanelData);
        }
    }
    private void CreateSpawnItemsList(SpawnPanelData spawnPanelData)
    {
        List<GameObject> interactableObjects = new List<GameObject>();

        foreach (GameObject obj in spawnPanelData.objectsToSpawn)
        {
            if (obj != null && obj.GetComponent<taskItem>() != null && obj.gameObject.layer == LayerMask.NameToLayer("InteractableItem"))
            {
                interactableObjects.Add(obj);
            }
            else if (obj.GetComponent<taskItem>() == null && obj.gameObject.layer == LayerMask.NameToLayer("InteractableItem"))
            {
                interactableObjects.Add(obj.GetComponentInChildren<taskItem>().gameObject);
            }
        }

        taskUIManager.CreateSpawnPanel(spawnPanelData);
    }
}