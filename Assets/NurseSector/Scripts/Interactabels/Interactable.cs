//Metaverse-Solutions Author: Amber Voskamp
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField, Tooltip("This list contains the items that can be used on this interactable")]
    private List<Item> m_itemList = new List<Item>();

    private InteractableManager m_interactableManager;
    private TaskManager m_taskManager;
    //Get taskitem here and a way to send this to the taskmanager and check off an task

    // Start is called before the first frame update
    void Start()
    {
        InteractableManager[] interactableManagers = FindObjectsByType<InteractableManager>(FindObjectsSortMode.None);

        //InteractableManager[] interactableManagers = FindObjectsOfType(typeof(InteractableManager)) as InteractableManager[];

        //Check if there is an interactableManager or if there are multiple there should only be one in the whole scene
        if (interactableManagers.Length != 1)
        {
            UnityEngine.Debug.LogError($"[Object] interactableManagers couldn't be found only one should exist");
            return;
        }

        m_interactableManager = interactableManagers[0];
        m_interactableManager.AddInteractable(this);

        m_taskManager = m_interactableManager.getTaskManager;
    }

    public bool CheckIfItemIsOnTheList(Item item)
    {
        return m_itemList.Contains(item);
    }

    public void FinishTask()
    {
        var taskItem = m_taskManager.GetTaskByInteractable(this);
        m_taskManager.CompleteTask(taskItem);
    }
}
