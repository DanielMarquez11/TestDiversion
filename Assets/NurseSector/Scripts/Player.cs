//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;


//public class Player : MonoBehaviour
//{

//    [HideInInspector] public Text tabHintText;
//    [HideInInspector] public Image clipboardSlotImage;
//    private GameObject clipboard;
//    private static bool isAnyObjectPickedUp = false;
//    private bool isClipboardPickedUp = false;
//    private bool isPlayerInRange = false;
//    private bool isPickedUp = false;

//    public GameObject interactionPromptE;
//    public GameObject interactionPromptQ;
//    public GameObject arms;
  
//    private MouseLook mouseLook;
//    [HideInInspector] public PickableObject pickableobject;
//    private PlayerTaskManager playerTaskManager;


//    // Start is called before the first frame update
//    void Start()
//    {
//        pickableobject = GameObject.FindWithTag("Pickable").GetComponent<PickableObject>();
//        playerTaskManager = GetComponent<PlayerTaskManager>();
//        clipboard = GameObject.FindWithTag("Clipboard");
    

//        HideArmsAtStart();
//        InitializeUIComponents();
//        HideInteractionPrompts();
//    }

//    private void HideArmsAtStart()
//    {
//        if (arms != null)
//        {
//            arms.SetActive(false);
//        }
//        else
//        {
//            Debug.LogError("Arms object not found at the start!");
//        }

//        mouseLook = Camera.main.GetComponent<MouseLook>();
//        if (mouseLook == null)
//        {
//            Debug.LogError("MouseLook script not found on the Main Camera!");
//        }
//    }

//    private void InitializeUIComponents()
//    {
//        GameObject canvas = GameObject.Find("Canvas");

//        clipboardSlotImage = canvas.transform.Find("ClipboardSlot")?.GetComponent<Image>();
//        tabHintText = canvas.transform.Find("TabHint")?.GetComponent<Text>();

//        clipboardSlotImage?.gameObject.SetActive(false);
//        tabHintText?.gameObject.SetActive(false);
//    }

//    private void HideInteractionPrompts()
//    {
//        interactionPromptE?.SetActive(false);
//        interactionPromptQ?.SetActive(false);
       
//    }


//    void OnTriggerEnter(Collider other)
//    {
//        // Check if the object is pickable
//        if ((other.CompareTag("Pickable") || other.CompareTag("Clipboard")) && !isPickedUp && !isAnyObjectPickedUp)
//        {
//            pickableobject = other.GetComponent<PickableObject>();
//            if (pickableobject != null && !pickableobject.isClipboardVisible)
//            {
//                isPlayerInRange = true;
//                interactionPromptE.SetActive(true);
//                Debug.Log("E prompt should be showing");
//            }
//        }

//        //Check if the medicine bottle is picked up to allow player to interact with Patient1
//        if (other.CompareTag("Pickable"))
//        {
//            PickableObject pickableObject = other.GetComponent<PickableObject>();
//            if (pickableObject != null && pickableObject.itemType == "Medicine")
//            {
//                Debug.Log("Medicine detected, calling PickUpMedicine()");
//                playerTaskManager.PickUpMedicine(); // This should set hasMedicine to true
//            }

//            else if (pickableObject != null && pickableObject.itemType == "Cup")
//            {
//                Debug.Log("Cup detected, calling PickUpCup()");
//                playerTaskManager.PickUpCup(); // This should set hasCup to true
//            }
//            else if (pickableObject != null && pickableObject.itemType == "Thermometer")
//            {
//                Debug.Log("Thermometer detected, calling PickUpThermometer()");
//                playerTaskManager.PickUpThermometer(); // This should set hasCup to true
//            }
//        }


//    }

//    void OnTriggerExit(Collider other)
//    {
//        // Check if exiting a pickable object
//        if (other.CompareTag("Pickable") || other.CompareTag("Clipboard"))
//        {
//            isPlayerInRange = false;
//            interactionPromptE.SetActive(false);
//           // currentPickableObject = null; // Clear the reference
//        }
//    }



//    // Update is called once per frame
//    void Update()
//    {
//        HandlePickUpAndDropInput();
//        HandleClipboardToggleInput();
//    }


//    void HandlePickUpAndDropInput()
//    {
//        if (isPlayerInRange && !isPickedUp && Input.GetKeyDown(KeyCode.E))
//        {
//            pickableobject.PickUpObject();
//            isPickedUp = true;
//            isAnyObjectPickedUp = true;
//        }
//        else if (isPickedUp && !isClipboardPickedUp && Input.GetKeyDown(KeyCode.Q))
//        {
//            pickableobject.DropObject();
//            isPickedUp = false;
//            isAnyObjectPickedUp = false;
//        }

//        RestrictArmsLookAround();
//    }

//    void HandleClipboardToggleInput()
//    {

//        if (isPickedUp && !pickableobject.isClipboardVisible)
//        {
//           // Debug.Log("Cannot show/hide clipboard while holding another object.");
//            return; // Exit the method if holding any non-clipboard object
//        }
//        if (clipboardSlotImage != null && clipboardSlotImage.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Tab))
//       {
//          pickableobject.ToggleClipboardAnimations();
//          Debug.Log("PickableObject script referenced correctly.calling ToggleClipboardAnimations method");

//            interactionPromptE?.SetActive(false);

//            isPickedUp = false;
//            isAnyObjectPickedUp = false;

//            RestrictArmsLookAround();

//        }
        
//    }

 
//    public void EnableArms()
//    {
//        if (arms != null)
//        {
//            arms.SetActive(true);
//            Animator armsAnimator = arms.GetComponent<Animator>();
//            if (armsAnimator != null)
//            {
//                armsAnimator.Update(0); // Force update to sync the Animator state
//            }
//        }
//    }

//   public void DisableArms()
//    {
//        if (arms != null)
//        {
//            arms.SetActive(false);

//        }
//    }

//    public void RestrictArmsLookAround()
//    {
//        if (mouseLook != null)
//        {
//            mouseLook.areArmsActive = true;
//        }
//    }

  
//}
