using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TaskManager;

public class TaskUI : MonoBehaviour
{
    public TMP_Text taskDescriptionText; // Reference to the text component
    public Toggle taskToggle;

    private Task currentTask;
    // Set the task details
    /* public void SetTask(Task task)
     {
         taskDescriptionText.text = (task.isCompleted ? "<s>" : "") + task.description + (task.isCompleted ? "</s>" : "");
     }*/
     /*public void SetTask(Task task)
     {
         currentTask = task;
         taskDescriptionText.text = task.description;
         // Log if the text was successfully set
         taskToggle.isOn = task.isCompleted;


     }*/

    public void SetTask(TaskItem task)
    {
        if (taskDescriptionText != null)
        {
            taskDescriptionText.gameObject.SetActive(true);  // Make sure it's enabled
            taskDescriptionText.text = task.desc;
        }

        if (taskToggle != null)
        {
            taskToggle.gameObject.SetActive(true);  // Ensure Toggle is enabled
            taskToggle.isOn = task.isCompleted;
            taskToggle.interactable = false;  // Optional, if you don't want the user to toggle it manually
        }
    }

    // Handle when the toggle (checkbox) is clicked
    private void OnTaskToggled(bool isOn)
    {
        if (currentTask != null)
        {
            currentTask.isCompleted = isOn;
            taskDescriptionText.text = currentTask.isCompleted ? "<s>" + currentTask.description + "</s>" : currentTask.description;
        }
    }
}
