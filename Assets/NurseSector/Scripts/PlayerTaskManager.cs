//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;


//public class PlayerTaskManager : MonoBehaviour
//{
//    public TaskManager taskManager; // Reference to the TaskManager
//    public GameObject medicineBottle; // Reference to the medicine bottle object
//    public GameObject Cup;
//    public GameObject Thermometer;

//    public GameObject patient1; // Reference to Patient 1
//    public GameObject patient2; // Reference to Patient 2
//    public GameObject patient3;

//    private bool hasMedicine = false; // Whether the player has picked up the medicine
//    private bool hasCup = false;
//    private bool hasThermometer = false;

//    private bool isNearPatient1 = false;
//    private bool isNearPatient2 = false;// Whether the player is near Patient 2
//    private bool isNearPatient3 = false;

//    public GameObject interactionPromptG;
//    private Text promptText; // Reference to the Text component on the G prompt

//    [HideInInspector] public PickableObject pickableobject;



//    void Start()
//    {
//        pickableobject = GameObject.FindWithTag("Pickable").GetComponent<PickableObject>();
//        if (pickableobject == null)
//        {
//            Debug.LogError("pickableobject not found!");
//        }

//        if (interactionPromptG != null)
//        {
//            promptText = interactionPromptG.GetComponent<Text>(); // Get the Text component on the G prompt
//            if (promptText == null)
//            {
//                Debug.LogError("No Text component found on interactionPromptG!");
//            }
//        }
//        interactionPromptG?.SetActive(false);
//    }



//    void Update()
//    {

//        // Show the G prompt only if the player has medicine and is near Patient 1
//        if (isNearPatient1 && hasMedicine && !taskManager.IsTaskCompleted("Give medication to patient 1"))
//        {
//            ShowGPrompt("Press G to give medication");
//            pickableobject.interactionPromptQ?.SetActive(false);
//        }
//        // Show the G prompt only if the player has the cup and is near Patient 2
//        else if (isNearPatient2 && hasCup && !taskManager.IsTaskCompleted("Bring cup to patient 2"))
//        {
//            ShowGPrompt("Press G to give cup ");
//            pickableobject.interactionPromptQ?.SetActive(false); // Hide Q prompt
//        }
//        else if (isNearPatient3 && hasThermometer && !taskManager.IsTaskCompleted("Check patient3's temperature"))
//        {
//            ShowGPrompt("Press G to take temperature");
//            pickableobject.interactionPromptQ?.SetActive(false); // Hide Q prompt
//        }
//        else
//        {
//            interactionPromptG?.SetActive(false); // Hide the prompt if conditions are not met

//        }

//        // Check if the player gives medicine to patient 1
//        if (hasMedicine && isNearPatient1 && Input.GetKeyDown(KeyCode.G))
//        {
//            GiveMedicationToPatient1();
//        }

//        // Check if the player gives the cup to patient 2
//        if (hasCup && isNearPatient2 && Input.GetKeyDown(KeyCode.G))
//        {
//            GiveCupToPatient2();
//        }

//        // Check if the player takes temperature of patient 3
//        if (hasThermometer && isNearPatient3 && Input.GetKeyDown(KeyCode.G))
//        {
//            TakeTemperaturePatient3();
//        }

//    }

//    void OnTriggerEnter(Collider other)
//    {

//        if (other.gameObject == patient1)
//        {
//            isNearPatient1 = true;
//            Debug.Log("Player is near Patient 1");

//            // Check if the player has medicine to show the G prompt
//            if (hasMedicine && !taskManager.IsTaskCompleted("Give medication to patient 1"))
//            {
//                ShowGPrompt("Press G to give medication to Patient 1");
//                pickableobject.interactionPromptQ?.SetActive(false); // Hide Q prompt
//            }
//        }
//        else if (other.gameObject == patient2)
//        {
//            isNearPatient2 = true;
//            Debug.Log("Player is near Patient 2");

//            // Check if the player has the cup to show the G prompt
//            if (hasCup && !taskManager.IsTaskCompleted("Bring cup to patient 2"))
//            {
//                ShowGPrompt("Press G to give cup to Patient 2");
//                pickableobject.interactionPromptQ?.SetActive(false); // Hide Q prompt
//            }
//        }
//        else if (other.gameObject == patient3)
//        {
//            isNearPatient3 = true;
//            Debug.Log("Player is near Patient 3");

//            // Check if the player has the thermometer to show the G prompt
//            if (hasThermometer && !taskManager.IsTaskCompleted("Check patient3's temperature"))
//            {
//                ShowGPrompt("Press G to take temperature of Patient 3");
//                pickableobject.interactionPromptQ?.SetActive(false); // Hide Q prompt
//            }
//        }

//    }

//    // Method called when exiting a trigger zone
//    void OnTriggerExit(Collider other)
//    {
//        if (other.gameObject == patient1)
//        {
//            isNearPatient1 = false;
//            Debug.Log("Player has left the area of Patient 1.");
//        }
//        else if (other.gameObject == patient2)
//        {
//            isNearPatient2 = false;
//            interactionPromptG?.SetActive(false);

//            Debug.Log("Player has left the area of Patient 2.");
//        }
//        else if (other.gameObject == patient3)
//        {
//            isNearPatient3 = false;
//            interactionPromptG?.SetActive(false); // Hide G prompt
//            Debug.Log("Player has left the area of Patient 3.");
//        }

//    }

//    void GiveMedicationToPatient1()
//    {
//        // Check if the task is available in the task manager
//        TaskManager.TaskItem task = taskManager.GetTask("Give medication to patient 1");
//        if (!task.isCompleted)
//        {
//            taskManager.CompleteTask(task);
//            hasMedicine = false; // Reset medicine status
//                                 // isTaskCompleted = true;
//            interactionPromptG?.SetActive(false);
//            pickableobject.interactionPromptQ?.SetActive(true);
//            Debug.Log("Task 1 completed: Gave medication to patient 1.");
//        }
//    }

//    void GiveCupToPatient2()
//    {
//        // Check if the task is available in the task manager
//        TaskManager.TaskItem task = taskManager.GetTask("Bring cup to patient 2");
//        if (!task.isCompleted)
//        {
//            taskManager.CompleteTask(task);
//            hasCup = false; // Reset cup status
//            interactionPromptG?.SetActive(false);
//            pickableobject.interactionPromptQ?.SetActive(true);
//            Debug.Log("Task 2 completed: Gave cup to patient 2.");
//        }
//    }

//    void TakeTemperaturePatient3()
//    {
//        TaskManager.TaskItem task = taskManager.GetTask("Check patient3's temperature");
//        if (!task.isCompleted)
//        {
//            taskManager.CompleteTask(task);
//            hasThermometer = false;
//            pickableobject.interactionPromptQ?.SetActive(true);
//            Debug.Log("Task 3 completed: Took temperature of patient 3.");
//        }

//    }

//    // Method to show the G prompt with a specific message
//    void ShowGPrompt(string message)
//    {
//        if (promptText != null)
//        {
//            promptText.text = message; // Update the text component with the message
//        }
//        interactionPromptG?.SetActive(true); // Show the G prompt
//    }


//    // This method can be called by the Player script when the player picks up the medicine
//    public void PickUpMedicine()
//    {
//        hasMedicine = true;
//        Debug.Log("Picked up the medicine.");

//        if (isNearPatient1 && !taskManager.IsTaskCompleted("Give medication to patient 1"))
//        {
//            // interactionPromptG?.SetActive(true); // Show the G prompt if near Patient 1
//            ShowGPrompt("Press G to give medication to Patient 1");
//        }
//    }

//    public void PickUpCup()
//    {
//        hasCup = true;
//        Debug.Log("Picked up the cup.");

//        if (isNearPatient2 && !taskManager.IsTaskCompleted("Bring cup to patient 2"))
//        {
//            ShowGPrompt("Press G to give cup to Patient 2"); // Show the G prompt if near Patient 2
//        }
//    }

//    public void PickUpThermometer()
//    {
//        hasThermometer = true;
//        Debug.Log("Picked up thermometer");

//        if (isNearPatient3 && !taskManager.IsTaskCompleted("Check patient3's temperature"))
//        {
//            ShowGPrompt("Press G take temperature Patient 3"); // Show the G prompt if near Patient 2
//        }

//    }
//    // Method to check if player is near an object (simple distance check)
//    bool IsNear(GameObject target)
//    {
//        return Vector3.Distance(transform.position, target.transform.position) < 2.0f; // Adjust the range as needed
//    }
//}
