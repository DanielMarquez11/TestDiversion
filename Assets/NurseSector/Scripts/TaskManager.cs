using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class TaskManager : MonoBehaviour
{
    [Serializable]
    public struct TaskItem
    {
        public string desc;
        [HideInInspector]
        public bool isCompleted;
        [HideInInspector]
        public Toggle toggle;
        public Interactable interactable;
    }

    public GameObject taskPrefab; // Reference to the TaskPrefab
    public Transform taskListContainer; // Container where the tasks will be instantiated
    [SerializeField]
    private List<TaskItem> taskItems = new List<TaskItem>(); // List to hold all tasks

    void Start()
    {
        UpdateTaskListUI(); // Update the UI at the start
    }

    // Method to add a task
    public void AddTask(Task newTask)
    {
        UpdateTaskListUI(); 
    }

    void UpdateTaskListUI()
    {
        if (taskListContainer == null)
        {
            Debug.LogError("TaskListContainer is not assigned!");
            return;
        }

        if (taskPrefab == null)
        {
            Debug.LogError("TaskPrefab is not assigned!");
            return;
        }

        // Clear the previous task list
        foreach (Transform child in taskListContainer)
        {
            Destroy(child.gameObject);
        }

        // Create a new UI element for each task
        for (int i = 0; i < taskItems.Count; i++)
        {
            var task = taskItems[i];
            if (task.interactable == null)
            {
                UnityEngine.Debug.LogWarning($"[TaskManager] `{task.desc}` has no interactable assigned ");
            }

            GameObject taskGO = Instantiate(taskPrefab, taskListContainer);
            TaskUI taskUI = taskGO.GetComponent<TaskUI>();
            task.toggle = taskUI.taskToggle;
            taskItems[i] = task;
            taskGO.SetActive(true);

            if (taskUI == null)
            {
                Debug.LogError("TaskPrefab does not have a TaskUI component!");
                return;
            }

            taskUI.SetTask(task);
        }
    }

    // Method to mark a task as completed
    public void CompleteTask(TaskItem task)
    {
        task.isCompleted = true;
        //Update Toggle 
        task.toggle.isOn = true;

    }

    public TaskItem GetTaskByInteractable(Interactable interactable)
    {
        for (int i = 0; i < taskItems.Count; i++)
        {
            var task = taskItems[i];
            if (task.interactable == interactable)
            {
                return task;
            }
        }

        UnityEngine.Debug.LogWarning($"[TaskManager] Task requested that doesn't exist");
        return new TaskItem();
    }

}
