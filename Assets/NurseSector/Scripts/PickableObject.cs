//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class PickableObject : MonoBehaviour
//{
//    public string itemType; // e.g., "Medicine", "Flyer", "Cup"
//    private Text tabHintText;
//    private Image clipboardSlotImage;
//    [HideInInspector] public static bool isAnyObjectPickedUp = false;
//    private bool isClipboardPickedUp = false;
//    [HideInInspector] public bool isClipboardVisible = false;
//    private Transform playerHand;
//    private bool isFirstAnimationActive = true;
//    private Vector3 originalScale;
//    [HideInInspector] public bool isPickedUp = false;
//    private Vector3 clipboardPositionOffset = new Vector3(-0.187f, 0.2095f, -0.0139f);
//    private Vector3 clipboardRotationOffset = new Vector3(-60.613f, 0f, 0f);

//    public Vector3 positionOffset;
//    public Vector3 rotationOffset;

//    public GameObject interactionPromptE;
//    public GameObject interactionPromptQ;
//    public GameObject arms;
//    private GameObject clipboard;

//    private Animator clipboardAnimator;
//    private MouseLook mouseLook;
//    private Player playerScript;

//    private float crossfadeDuration = 0.2f; // Duration for crossfade

//    //private bool isInRestrictedArea = false;

//    void Start()
//    {

//        playerScript = GameObject.FindWithTag("Player").GetComponent<Player>();
//        if (playerScript == null)
//        {
//            Debug.LogError("Player script not found!");
//        }
 
//        FindPlayerHand();
//        InitializeClipboardAnimator();
       
//    }


//    private void FindPlayerHand()
//    {
//        playerHand = GameObject.FindWithTag("Player").transform.Find("HandPosition");
//        if (playerHand == null)
//        {
//            Debug.LogError("HandPosition not found! Check hierarchy and path.");
//        }
//        originalScale = transform.localScale;
//    }

//    private void InitializeClipboardAnimator()
//    {
//        clipboard = GameObject.Find("Clipboard");
//        if (clipboard == null)
//        {
//            Debug.LogError("Clipboard object not found in the scene!");
//        }
//        clipboardAnimator = clipboard.GetComponent<Animator>();
//        if (clipboardAnimator == null)
//        {
//            Debug.LogError("No Animator component found on the clipboard!");
//        }
//    }


//    public void PickUpObject()
//    {
//        isPickedUp = true;
//        isAnyObjectPickedUp = true;

//        if (gameObject.CompareTag("Clipboard"))
//        {
//            isClipboardPickedUp = true;
//            isClipboardVisible = true;
//            playerScript.clipboardSlotImage?.gameObject.SetActive(true);
//            playerScript.tabHintText?.gameObject.SetActive(true);

//            Debug.Log("clipboardSlotImage should get enabled ");

//            //StartCoroutine(DisableClipboardWithDelay());
//           // HideClipboard();


//            SetOffsetsForClipboardShowClipboard();


//            AttachObjectToHand(clipboard.transform);
           
//        }
//        else
//        {
//            isAnyObjectPickedUp = true;
//            interactionPromptQ?.SetActive(true);

//            AttachObjectToHand(transform);
//        }

//        playerScript.EnableArms();

//        if (arms != null)
//        {
//            Animator animator = arms.GetComponent<Animator>();
//            if (animator != null)
//            {
//                animator.Play("PickUp", 0, 0); // Crossfade to "PickUp" animation

//                if (isClipboardPickedUp)
//                 {
//                     StartCoroutine(HideArmsAfterAnimation(animator));
//                 }
//            }
//            playerScript.RestrictArmsLookAround();
          
//        }

//    }


//    void AttachObjectToHand(Transform pickableObject)
//    {
//        if (pickableObject != null && playerHand != null)
//        {
//            bool isClipboard = pickableObject.CompareTag("Clipboard");



//            // Use clipboard-specific offsets if it's the clipboard, otherwise use general offsets
//            Vector3 posOffset = isClipboard ? clipboardPositionOffset : positionOffset;
//            Vector3 rotOffset = isClipboard ? clipboardRotationOffset : rotationOffset;

//            // Attach the object to the player's hand with the correct offsets
//            pickableObject.SetParent(playerHand);
//            pickableObject.localPosition = posOffset;
//            pickableObject.localRotation = Quaternion.Euler(rotOffset);

//            Rigidbody rb = pickableObject.GetComponent<Rigidbody>();
//            if (rb != null)
//            {
//                rb.isKinematic = true;
//                rb.useGravity = false;
//            }

//            //Debug.Log($"{pickableObject.name} attached to hand position and rotation.");
//        }
//        else
//        {
//            Debug.LogError("PickableObject or PlayerHand is null! Cannot snap object to hand.");
//        }

//        interactionPromptE?.SetActive(false);
//    }

//   public void DropObject()
//    {

//        // Check if the current object is the clipboard
//        if (isClipboardPickedUp)
//        {
//            Debug.Log("Cannot drop the clipboard!");
//            return; // Exit the method without dropping
//        }
//        isPickedUp = false;
//        isAnyObjectPickedUp = false;


//        Animator animator = arms.GetComponent<Animator>();
//        if (animator != null)
//        {
//            animator.CrossFade("Drop", crossfadeDuration); // Crossfade to "Drop" animation
//            StartCoroutine(DisableArmsAfterAnimation(animator));
//        }
//        DetachObjectFromHand();

//        if (isClipboardPickedUp)
//        {
//            HideClipboard(); // Directly call HideClipboard instead of using a coroutine
//            isClipboardPickedUp = false;
//            isClipboardVisible = false;
//        }

//        interactionPromptQ?.SetActive(false);
//    }

//    void DetachObjectFromHand()
//    {
//        transform.SetParent(null);
//        transform.localScale = originalScale;

//        Rigidbody rb = GetComponent<Rigidbody>();
//        if (rb != null)
//        {
//            rb.isKinematic = false;
//            rb.useGravity = true;
//        }
//    }

//    IEnumerator HideArmsAfterAnimation(Animator animator)
//    {
//        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

//        if (!isClipboardPickedUp || !isClipboardVisible)
//        {
//            playerScript.DisableArms();
//            playerScript.RestrictArmsLookAround();
//        }
//    }

//    IEnumerator DisableArmsAfterAnimation(Animator animator)
//    {
//        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
//        playerScript.DisableArms();
//        playerScript.RestrictArmsLookAround();
//    }



//    public void ToggleClipboardAnimations()
//    {
//        // Check the current state of the clipboard to determine what to do
//        bool isShowing = clipboardAnimator.GetCurrentAnimatorStateInfo(0).IsName("ShowClipboard");

//        if (isShowing)
//        {
//            // Clipboard is currently shown, so hide it
//            HideClipboard();
//            isFirstAnimationActive = false; // Now the clipboard is hidden
//        }
//        else
//        {
//            // Clipboard is currently hidden, so show it
//            ShowClipboard();
//            isFirstAnimationActive = true; // Now the clipboard is shown
//        }
//    }

 


//    void ShowClipboard()
//    {
//        if (clipboard != null && clipboardAnimator != null)
//        {
           
//            playerScript.EnableArms(); // Enable arms before showing clipboard
//            clipboard.SetActive(true); // Ensure clipboard is active
//            playerScript.interactionPromptE?.SetActive(false);


//            if (!clipboardAnimator.GetCurrentAnimatorStateInfo(0).IsName("ShowClipboard"))
//            {
//                clipboardAnimator.ResetTrigger("Hide");
//                clipboardAnimator.SetTrigger("Show");
//                Debug.Log("ShowClipboard animation triggered.");

//                StartCoroutine(WaitForShowClipboardAnimation());

//            }
         
//        }
//    }

//    void SetOffsetsForClipboardShowClipboard()
//    {
//        clipboardPositionOffset = new Vector3(-0.187f, 0.2095f, -0.0139f);
//        clipboardRotationOffset = new Vector3(-60.613f, 0f, 0f);
//      //  Debug.Log("Clipboard Position and Rotation Offsets updated to final values.");
//    }

//    IEnumerator WaitForShowClipboardAnimation()
//    {
//        // Wait until the ShowClipboard animation is almost done
//        while (clipboardAnimator.GetCurrentAnimatorStateInfo(0).IsName("ShowClipboard") &&
//               clipboardAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
//        {
//            yield return null;
//        }

//        // Set the final offsets once the animation is complete
//        SetOffsetsForClipboardShowClipboard();

//        // Attach the clipboard to the hand using the updated offsets
//        AttachObjectToHand(clipboard.transform);

//    }


//    void HideClipboard()
//    {
//        if (clipboard != null && clipboardAnimator != null)
//        {
//            playerScript.interactionPromptE?.SetActive(false);

//            // Check if it's already playing the animation to avoid conflicts
//            if (!clipboardAnimator.GetCurrentAnimatorStateInfo(0).IsName("HideClipboard"))
//            {
//                clipboardAnimator.ResetTrigger("Show");
//                clipboardAnimator.SetTrigger("Hide");
//                Debug.Log("HideClipboard animation triggered.");

//                StartCoroutine(WaitForAnimationToEnd());
//            }
//            else
//            {
//                Debug.LogWarning("HideClipboard animation is already playing.");
//            }
//        }
//        else
//        {
//            Debug.LogError("Clipboard or clipboardAnimator is null!");
//        }
//    }




//    IEnumerator WaitForAnimationToEnd()
//    {
//        float hideDuration = clipboardAnimator.GetCurrentAnimatorStateInfo(0).length;
//        Debug.Log($"HideClipboard animation duration: {hideDuration}");

//        if (hideDuration > 0)
//        {
//            yield return new WaitForSeconds(hideDuration);
//        }
//        else
//        {
//           // Debug.LogWarning("HideClipboard animation duration is zero or invalid.");
//        }

//        Debug.Log("Disabling clipboard and arms after HideClipboard animation.");
//        clipboard.SetActive(false);
//        playerScript.DisableArms(); // Disable arms after hiding the clipboard

//    }

//}