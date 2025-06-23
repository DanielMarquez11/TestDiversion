using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class TaskListContainer : MonoBehaviour
{
    public TaskList taskList;

    [System.Serializable]
    public class TaskList
    {
        public Task[] tasks;

        public TaskManager taskManager;

        public GameObject FindNestedObject(GameObject prefab, bool item)
        {
            if (prefab != null)
            {
                // Check for nested delivery location within item
                if (prefab.layer == LayerMask.NameToLayer("DeliveryLocation") &&
                    prefab.CompareTag("NestedItem") && item)
                {
                    Transform nestedItem = prefab.GetComponentsInChildren<Transform>(true)
                        .FirstOrDefault(t => t.gameObject.layer == LayerMask.NameToLayer("InteractableItem"));

                    if (nestedItem != null)
                    {
                        prefab = nestedItem.gameObject;
                    }
                    else
                    {
                        prefab.gameObject.tag = "Untagged";
                    }
                }
                // Check for nested item within delivery location
                else if (prefab.layer == LayerMask.NameToLayer("InteractableItem") &&
                         prefab.CompareTag("NestedDeliveryLocation") && !item)
                {
                    Transform nestedDeliveryLocation = prefab.GetComponentsInChildren<Transform>(true)
                        .FirstOrDefault(t => t.gameObject.layer == LayerMask.NameToLayer("DeliveryLocation"));

                    if (nestedDeliveryLocation != null)
                    {
                        prefab = nestedDeliveryLocation.gameObject;
                    }
                    else
                    {
                        Debug.LogError($"Nested DeliveryLocation not found in delivery location: {prefab.name}");
                    }
                }
                return prefab;
            }

            // If no nested object is found, return the original prefab
            return prefab;
        }

        [System.Serializable]
        public class Task
        {
            public string taskName;
            public bool isUrgent;
            public int weight;
            public int preferredTaskHostNumber;
            public List<TaskStep> steps = new List<TaskStep>();

            [System.Serializable]
            public class TaskStep
            {
                [Tooltip("The task step name.")]
                public string stepName;
                [Tooltip("The item that needs to be delivered to the delivery location.")]
                public GameObject itemPrefab;
                [Tooltip("The location where the item needs to be delivered.")]
                public GameObject deliveryLocationPrefab;
                [Tooltip("The steps before this step that need to be completed before this step becomes available. For Linear Step Completion, set only the previous step to true in all steps. For Open Step Completion, set all steps to false in each step.")]
                public List<bool> requirePreviousSteps = new List<bool>();

                [Header("Experimental Features")]
                [Tooltip("Place the item in a delivery slot on the deliveryLocationPrefab. deliverySlot should be a transform child of the deliveryLocation layered object in the prefab. A deliverySlot can only hold one item.")]
                public bool placeItemInDeliverySlot = false;
                [Tooltip("Lock the item in the delivery slot. The item cannot be removed from the slot. This feature is only available if placeItemInDeliverySlot is enabled.")]
                public bool lockInSlot = false;
                [Tooltip("Consume the item after the task is completed. Destroys the gameobject from the scene.")]
                public bool consumeItem = false;
                [Tooltip("Remove the functionality of the delivery location after the task is completed. Object remains in the scene.")]
                public bool removeDelivery = false;
                [Tooltip("An object that the item for the step can be transformed into on interaction.")]
                public GameObject swapObject;
                [Tooltip("The animation to play on the characters arms when the task is completed. If None, no animation will be played. Note: DoorOpenClose and PointPress require that the player is not be holding an item. If the player is holding an item, the animation will not play (in the future this will play the animation in the off-hand). RubbingHands also requires that the player is not holding an item, but uses both hands.")]
                public playerArmAnimation playerArmAnim;

                public enum playerArmAnimation
                {
                    None,
                    GenericMedicalAction,
                    GiveItem,
                    PlaceItem,
                    HangIVBag,
                    IVNeedle,
                    TakeTemperature,
                    DoorOpenClose,
                    PointPress,
                    RubbingHands
                }
            }
        }

        // --- Task System Classes ---

        public class TaskManager
        {
            public List<Task> tasks;
            public List<TaskStepNode> taskTreeRoot = new List<TaskStepNode>();
            public Dictionary<Task.TaskStep, (bool isAvailable, bool isCompleted)> taskStepAvailability =
                new Dictionary<Task.TaskStep, (bool isAvailable, bool isCompleted)>();
            public Dictionary<string, List<Task.TaskStep>> locationToSteps =
                new Dictionary<string, List<Task.TaskStep>>();
            public Dictionary<string, List<Task.TaskStep>> itemToSteps =
                new Dictionary<string, List<Task.TaskStep>>();

            // Updated dictionary: TaskHost to list of Tasks
            public Dictionary<taskHost, List<Task>> taskHostToTasks = new Dictionary<taskHost, List<Task>>();
            public List<Task.TaskStep> noItemSteps = new List<Task.TaskStep>();

            public List<TaskList.Task> urgentTaskQueue = new List<TaskList.Task>();
            public TaskList.Task activeUrgentTask = null;
            public TaskManager(List<Task> unityTasks)
            {
                this.tasks = unityTasks;
            }

            public void buildTaskTree()
            {

                Debug.Log("---- Building Task Tree ----");

                foreach (var taskHostEntry in taskHostToTasks)
                {
                    taskHost taskHost = taskHostEntry.Key;
                    List<TaskList.Task> taskHostTasks = taskHostEntry.Value;

                    foreach (TaskList.Task task in taskHostTasks)
                    {
                        Debug.Log($"Building task: {task.taskName}");
                        Debug.Log($"Task steps count: {task.steps.Count}");
                        Debug.Log($"Task steps: {string.Join(", ", task.steps.Select(s => s.itemPrefab ? (s.itemPrefab.GetComponent<taskItem>() ? s.itemPrefab.GetComponent<taskItem>().objectName : s.itemPrefab.name) : "None"))}");
                        Debug.Log($"Task steps: {string.Join(", ", task.steps.Select(s => s.deliveryLocationPrefab ? (s.deliveryLocationPrefab.GetComponent<taskHost>() ? $"taskHost {s.deliveryLocationPrefab.GetComponent<taskHost>().taskHostNumber}" : (s.deliveryLocationPrefab.GetComponent<taskItem>() ? s.deliveryLocationPrefab.GetComponent<taskItem>().objectName : s.deliveryLocationPrefab.name)) : "None"))}");

                        // Create nodes for each step
                        List<TaskStepNode> taskStepNodes = task.steps.Select(step => new TaskStepNode(step)).ToList();

                        for (int i = 0; i < taskStepNodes.Count; i++)
                        {
                            TaskStepNode node = taskStepNodes[i];
                            for (int j = node.taskStep.requirePreviousSteps.Count - 1; j >= 0; j--)
                            {
                                if (node.taskStep.requirePreviousSteps[j] == true)
                                {
                                    int parentIndex = i - (node.taskStep.requirePreviousSteps.Count - j);

                                    if (parentIndex >= 0 && parentIndex < taskStepNodes.Count)
                                    {
                                        TaskStepNode parentNode = taskStepNodes[parentIndex];
                                        parentNode.children.Add(node);
                                    }

                                }
                            }
                            // Add step to dictionaries
                            if (node.taskStep.deliveryLocationPrefab != null)
                            {
                                AddStepToDictionary(node.taskStep.deliveryLocationPrefab, node.taskStep, locationToSteps);
                            }
                            if (node.taskStep.itemPrefab != null)
                            {
                                AddStepToDictionary(node.taskStep.itemPrefab, node.taskStep, itemToSteps);
                            }
                            else
                            {
                                noItemSteps.Add(node.taskStep);
                            }

                            // Add root nodes to taskTreeRoot
                            if (node.taskStep.requirePreviousSteps.Count == 0 || node.taskStep.requirePreviousSteps.All(x => x == false))
                            {
                                taskTreeRoot.Add(node);
                            }
                        }
                    }
                }

                Debug.Log("---- Task Tree Built ----");

            }

            // Helper function to add nested prefabs to the dictionaries
            private void AddStepToDictionary(GameObject prefab, Task.TaskStep step, Dictionary<string, List<Task.TaskStep>> dictionary)
            {
                if (prefab != null)
                {
                    string nameToAdd;
                    if (prefab.GetComponent<taskItem>())
                    {
                        nameToAdd = prefab.GetComponent<taskItem>().objectName;
                    }
                    else if (prefab.GetComponent<taskHost>())
                    {
                        nameToAdd = prefab.GetComponent<taskHost>().taskHostNumber.ToString();
                    }
                    else
                    {
                        nameToAdd = prefab.name;
                    }
                    if (!dictionary.ContainsKey(nameToAdd))
                    {
                        dictionary[nameToAdd] = new List<Task.TaskStep>();
                    }
                    dictionary[nameToAdd].Add(step);
                }
            }

            public void initializeAvailability()
            {
                Debug.Log("---- Initializing Availability ----");

                this.taskStepAvailability = new Dictionary<Task.TaskStep, (bool isAvailable, bool isCompleted)>();

                // Initialize all steps as not available and not completed
                foreach (var task in this.tasks)
                {
                    foreach (var step in task.steps)
                    {
                        this.taskStepAvailability[step] = (false, false);
                    }
                }

                // Traverse the task tree and update availability
                foreach (var rootNode in this.taskTreeRoot)
                {
                    TaskList.Task currentTask = this.tasks.FirstOrDefault(t => t.steps.Contains(rootNode.taskStep));
                    if (currentTask != null)
                    {
                        traverse(rootNode, currentTask, 0); // Start with stepIndex 0
                    }
                }

                Debug.Log("---- Availability Initialized ----");

                void traverse(TaskStepNode node, TaskList.Task task, int stepIndex)
                {

                    List<TaskStepNode> currentLevelNodes = new List<TaskStepNode>();
                    currentLevelNodes.Add(node); // Start with the root node

                    while (currentLevelNodes.Count > 0)
                    {
                        List<TaskStepNode> nextLevelNodes = new List<TaskStepNode>();

                        foreach (TaskStepNode currentNode in currentLevelNodes)
                        {
                            // Check if all required previous steps are completed (or if there are no required steps)
                            bool allParentsCompleted = currentNode.taskStep.requirePreviousSteps.Count == 0 ||
                                                       currentNode.taskStep.requirePreviousSteps.All(x => !x) ||
                                                       currentNode.taskStep.requirePreviousSteps.Where(x => x).All(x =>
                                                       {
                                                           Task.TaskStep previousStep = GetPreviousStep(currentNode.taskStep, currentNode.taskStep.requirePreviousSteps.IndexOf(x));
                                                           return this.taskStepAvailability.ContainsKey(previousStep) && this.taskStepAvailability[previousStep].isCompleted;
                                                       });




                            // If all parents are completed, mark the current step as available
                            if (allParentsCompleted)
                            {
                                // Only mark as available if not already completed
                                if (!this.taskStepAvailability[node.taskStep].isCompleted)
                                {
                                    this.taskStepAvailability[node.taskStep] = (true, false);
                                }
                            }

                            // Add children to the next level
                            nextLevelNodes.AddRange(currentNode.children);
                        }

                        currentLevelNodes = nextLevelNodes; // Move to the next level
                    }

                }

            }

            public void LogTaskTree()
            {
                Debug.Log("---- Task Tree Visualization ----");

                for (int i = 0; i < this.tasks.Count; i++)
                {
                    TaskList.Task currentTask = this.tasks[i];
                    Debug.Log($"Task {i}: {currentTask.taskName}");

                    // Create dictionaries to store node indices and parent indices
                    Dictionary<TaskStepNode, int> nodeIndices = new Dictionary<TaskStepNode, int>();
                    Dictionary<int, List<int>> parentIndices = new Dictionary<int, List<int>>();

                    // Traverse the tree and populate the dictionaries
                    foreach (var rootNode in this.taskTreeRoot.Where(node => currentTask.steps.Contains(node.taskStep)))
                    {
                        LogNode(rootNode, 1, nodeIndices, parentIndices);
                    }

                    // Create a list to store the visualization strings for each step
                    List<string> stepVisualizationStrings = new List<string>();

                    // Generate the visualization strings for each step
                    for (int stepIndex = 0; stepIndex < currentTask.steps.Count; stepIndex++) // Iterate through steps by index
                    {
                        //int stepIndex = kvp.Value; // Get the step index
                        string parents = "";

                        // Collect parent indices from ALL nodes that have the current stepIndex as a child
                        foreach (var parentKvp in nodeIndices.Where(x => x.Key.children.Any(c => nodeIndices[c] == stepIndex)))
                        {
                            int parentStepIndex = parentKvp.Value;
                            if (!parentIndices.ContainsKey(stepIndex))
                            {
                                parentIndices[stepIndex] = new List<int>();
                            }
                            parentIndices[stepIndex].Add(parentStepIndex);
                        }


                        if (parentIndices.ContainsKey(stepIndex))
                        {
                            parentIndices[stepIndex].Sort();
                            // Remove duplicate parent indices
                            parentIndices[stepIndex] = parentIndices[stepIndex].Distinct().ToList();
                            parents = string.Join(", ", parentIndices[stepIndex]);
                        }

                        // Find the node for the current stepIndex
                        var currentNode = nodeIndices.FirstOrDefault(x => x.Value == stepIndex).Key;
                        string connections = "";
                        if (currentNode != null)
                        {
                            connections = string.Join(", ", currentNode.children.Select(child => nodeIndices[child]));
                        }

                        if (currentNode != null)
                        {
                            string visualizationString = $"  [{parents}] -> Step {stepIndex} " +
                                                         $"(Available: {taskStepAvailability[currentNode.taskStep].isAvailable}, " +
                                                         $"Completed: {taskStepAvailability[currentNode.taskStep].isCompleted}) -> [{connections}]";
                            stepVisualizationStrings.Add(visualizationString); // Add the string to the list
                        }
                    }

                    // Sort the visualization strings by step index
                    stepVisualizationStrings.Sort((a, b) =>
                    {
                        int indexA = int.Parse(a.Split(new string[] { "Step " }, StringSplitOptions.None)[1].Split(' ')[0].Trim());
                        int indexB = int.Parse(b.Split(new string[] { "Step " }, StringSplitOptions.None)[1].Split(' ')[0].Trim());
                        return indexA.CompareTo(indexB);
                    });

                    // Log the sorted visualization strings
                    foreach (var visualizationString in stepVisualizationStrings)
                    {
                        Debug.Log(visualizationString);
                    }

                }

                Debug.Log("---- End of Task Tree ----");
            }

            void LogNode(TaskStepNode node, int depth, Dictionary<TaskStepNode, int> nodeIndices, Dictionary<int, List<int>> parentIndices, int parentIndex = -1)
            {
                if (node == null)
                    return;

                string indent = new string(' ', depth * 4);

                // Find the task associated with the node
                TaskList.Task task = this.tasks.FirstOrDefault(t => t.steps.Contains(node.taskStep));

                if (task != null)
                {
                    if (!nodeIndices.ContainsKey(node))
                    {
                        int stepIndex = task.steps.IndexOf(node.taskStep);

                        // Store the node index for visualization
                        nodeIndices[node] = stepIndex;

                        // Store the direct parent index (only if a valid parent index is provided)
                        if (parentIndex != -1)
                        {
                            if (!parentIndices.ContainsKey(stepIndex))
                            {
                                parentIndices[stepIndex] = new List<int>();
                            }
                            parentIndices[stepIndex].Add(parentIndex);
                        }

                        // Recursively log child nodes and store parent indices
                        int childIndex = stepIndex + 1; // Calculate the index of the child node
                        foreach (var child in node.children)
                        {
                            LogNode(child, depth + 1, nodeIndices, parentIndices, stepIndex);
                            childIndex++;
                        }
                    }
                }
            }

            public void updateAvailability(TaskList.Task.TaskStep completedStep)
            {
                Debug.Log("---- Updating Availability ----");
                string itemName = completedStep.itemPrefab != null ? completedStep.itemPrefab.name : "None";
                string deliveryLocationName = completedStep.deliveryLocationPrefab != null ? completedStep.deliveryLocationPrefab.name : "None";
                Debug.Log($"Completed Step: Item - {itemName}, Location - {deliveryLocationName}");

                TaskStepNode completedNode = findNode(this.taskTreeRoot, completedStep);

                if (completedNode != null)
                {

                    //mark node as completed
                    this.taskStepAvailability[completedStep] = (this.taskStepAvailability[completedStep].isAvailable, true);

                    // Collect newly available steps
                    List<TaskList.Task.TaskStep> newlyAvailableSteps = new List<TaskList.Task.TaskStep>();

                    void updateChildren(TaskStepNode node)
                    {
                        foreach (TaskStepNode child in node.children)
                        {
                            if (child.taskStep == null)
                            {
                                continue; // Skip this child node
                            }

                            bool allParentsCompleted = true;


                            for (int i = 0; i < child.taskStep.requirePreviousSteps.Count; i++)
                            {
                                if (child.taskStep.requirePreviousSteps[i] == true)
                                {
                                    Task.TaskStep previousStep = GetPreviousStep(child.taskStep, i);
                                    if (previousStep == null || !this.taskStepAvailability.ContainsKey(previousStep) || !this.taskStepAvailability[previousStep].isCompleted)
                                    {
                                        allParentsCompleted = false;
                                        break;
                                    }
                                }
                            }


                            if (allParentsCompleted)
                            {
                                this.taskStepAvailability[child.taskStep] = (true, this.taskStepAvailability[child.taskStep].isCompleted);
                                newlyAvailableSteps.Add(child.taskStep);
                            }
                        }
                    }
                    if (completedNode.children != null)
                    {
                        if (completedNode.children.Count > 0)
                        {
                            updateChildren(completedNode);
                        }
                    }

                    TaskUIManager taskUIManager = GameObject.FindFirstObjectByType<TaskUIManager>();
                    if (taskUIManager != null)
                    {
                        taskUIManager.UpdateTaskUI(completedStep, newlyAvailableSteps);
                    }
                }

                Debug.Log("---- Availability Updated ----");
                LogActiveTaskSteps();
            }

            public TaskList.Task.TaskStep GetPreviousStep(TaskList.Task.TaskStep currentStep, int index)
            {
                TaskList.Task task = this.tasks.FirstOrDefault(t => t.steps.Contains(currentStep));
                if (task != null)
                {
                    int currentStepIndex = task.steps.IndexOf(currentStep);

                    if (index >= 0 && index < task.steps.Count)
                    {
                        return task.steps[index];
                    }
                }
                return null;
            }

            private TaskStepNode findNode(List<TaskStepNode> nodes, Task.TaskStep taskStep)
            {
                foreach (TaskStepNode node in nodes)
                {
                    if (node == null) // Check if the node itself is null
                    {
                        continue; // Skip this node
                    }

                    if (node.taskStep == null) // Check if the taskStep is null
                    {
                        continue; // Skip this node
                    }

                    if (node.taskStep == taskStep)
                    {
                        return node;
                    }

                    TaskStepNode foundNode = findNode(node.children, taskStep);
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
                return null;
            }

            public void LogActiveTaskSteps()
            {
                Debug.Log("---- Active Task Steps ----");

                // Group steps by taskHost and task
                var groupedSteps = this.taskStepAvailability
                    .Where(kv => kv.Value.isAvailable && !kv.Value.isCompleted)
                    .GroupBy(kv => new
                    {
                        taskHost = taskHostToTasks.FirstOrDefault(p => p.Value.Any(t => t.steps.Contains(kv.Key))).Key,
                        Task = this.tasks.FirstOrDefault(t => t.steps.Contains(kv.Key))
                    })
                    .OrderBy(g => g.Key.taskHost.taskHostNumber)
                    .ThenBy(g => g.Key.Task.taskName);

                // Log each taskHost's tasks and steps
                foreach (var group in groupedSteps)
                {
                    var taskHost = group.Key.taskHost;
                    var task = group.Key.Task;

                    Debug.Log($"taskHost: {taskHost.taskHostNumber} - Task: {task.taskName}");

                    foreach (var entry in group)
                    {
                        var step = entry.Key;
                        string itemName = step.itemPrefab ? (step.itemPrefab.GetComponent<taskItem>() ? step.itemPrefab.GetComponent<taskItem>().objectName : step.itemPrefab.name) : "None";
                        string locationName = step.deliveryLocationPrefab ? (step.deliveryLocationPrefab.GetComponent<taskHost>() ? $"taskHost {step.deliveryLocationPrefab.GetComponent<taskHost>().taskHostNumber}" : (step.deliveryLocationPrefab.GetComponent<taskItem>() ? step.deliveryLocationPrefab.GetComponent<taskItem>().objectName : step.deliveryLocationPrefab.name)) : "None";
                        Debug.Log($"  Step: Item - {itemName}, Location - {locationName}");
                    }
                }

                Debug.Log("---- End of Active Task Steps ----");
            }

            [HideInInspector]
            public float urgentTaskTimer; // Initial timer value

            public void UpdateUrgentTasks(float deltaTime)
            {
                if (activeUrgentTask == null && urgentTaskQueue.Count > 0)
                {
                    urgentTaskTimer -= deltaTime;
                    if (urgentTaskTimer <= 0f)
                    {
                        ActivateNextUrgentTask();
                        urgentTaskTimer = UnityEngine.Random.Range(FindFirstObjectByType<SceneTaskManager>().urgentCooldownRange.x,
                                               FindFirstObjectByType<SceneTaskManager>().urgentCooldownRange.y);
                    }
                }
            }

            private void ActivateNextUrgentTask()
            {
                if (urgentTaskQueue.Count == 0)
                {
                    Debug.LogWarning("No urgent tasks available in the list.");
                    return;
                }

                int randomIndex = UnityEngine.Random.Range(0, urgentTaskQueue.Count);
                activeUrgentTask = urgentTaskQueue[randomIndex];
                urgentTaskQueue.RemoveAt(randomIndex);
                Debug.Log("New Urgent Task: " + activeUrgentTask.taskName);

                // Add the activeUrgentTask to the UI
                TaskUIManager taskUIManager = FindFirstObjectByType<TaskUIManager>();
                if (taskUIManager != null)
                {
                    taskUIManager.ActivateUrgentTaskInUI(activeUrgentTask);
                }
            }
        }

        public class TaskStepNode
        {
            public Task.TaskStep taskStep;
            public List<TaskStepNode> children = new List<TaskStepNode>();
            public bool isAvailable;
            public bool isCompleted;

            public TaskStepNode(Task.TaskStep taskStep)
            {
                this.taskStep = taskStep;
            }
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if (taskList.tasks.Length >= 1)
        {
            for (int i = 0; i < taskList.tasks.Length; i++)
            {
                if (taskList.tasks[i].steps.Count >= 1)
                {
                    for (int j = 0; j < taskList.tasks[i].steps.Count; j++)
                    {
                        while (taskList.tasks[i].steps[j].requirePreviousSteps.Count < j)
                        {
                            taskList.tasks[i].steps[j].requirePreviousSteps.Add(false);
                        }
                        while (taskList.tasks[i].steps[j].requirePreviousSteps.Count > j)
                        {
                            taskList.tasks[i].steps[j].requirePreviousSteps.RemoveAt(taskList.tasks[i].steps[j].requirePreviousSteps.Count - 1);
                        }
                    }
                }
                else if (taskList.tasks[i].steps.Count == 0)
                {
                    taskList.tasks[i].steps.Add(null);
                }
            }
        }
    }
}