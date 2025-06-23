using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ItemInteraction : MonoBehaviour
{
    [SerializeField]
    private float playerReach = 5f;
    public Transform playerHandTransform;
    public Transform playerHand0Transform;
    private GameObject tooltipCanvas;

    private GameObject pickupTooltip;
    private GameObject dropTooltip;
    private GameObject interactTooltip;


    private GameObject heldItem;
    private GameObject targetedObject;
    List<Collider> enabledColliders = new List<Collider>();

    [HideInInspector]
    public TaskListContainer.TaskList.TaskManager taskManager;
    [HideInInspector]
    public TaskListContainer TaskListContainerInstance;

    [SerializeField]
    private float moveSpeed = 5f;
    
    private bool movementEnabled = true;
    private Vector2 rotation = Vector2.zero;

    private GameObject nestedChild;
    private bool isItem;
    private bool isDeliveryLocation;

    private Animator m_animator;

    private TaskUIManager taskUIManager;

    #region Sensitivity

    public float MouseSensitivity;
    #endregion
    private GameObject lastTargetedObject;
    private OutlineController outlineController;
    public float clamp = 89;

    private scoreUIManager scoreUIManager;

    public Shader itemOutlineShader;

    public bool canOpenTaskMenu = true;

    void Start()
    {
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        m_animator = GetComponentsInChildren<Animator>().FirstOrDefault();

        if (!m_animator)
        {
            Debug.LogError("Animator not found");
        }

        taskUIManager = FindObjectsByType<TaskUIManager>(FindObjectsSortMode.None).FirstOrDefault();
        if (!taskUIManager)
        {
            Debug.LogError("TaskUIManager not found");
        }
        else
        {
            tooltipCanvas = taskUIManager.tooltipCanvas;
            pickupTooltip = taskUIManager.pickupTooltip;
            dropTooltip = taskUIManager.dropTooltip;
            interactTooltip = taskUIManager.interactTooltip;
        }

        scoreUIManager = FindObjectsByType<scoreUIManager>(FindObjectsSortMode.None).FirstOrDefault();
        if (!scoreUIManager)
        {
            Debug.LogError("ScoreUIManager not found");
        }
        
        // Start rotation
        rotation.x = transform.rotation.eulerAngles.x;

    }

    public void Move()
    {
        if (!movementEnabled) return;
        
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        this.gameObject.GetComponent<CharacterController>().Move(move);

        //make player affected by gravity
        this.gameObject.GetComponent<CharacterController>().Move(new Vector3(0, -9.8f * Time.deltaTime, 0));
    }

    public void EnableDisableMovement(bool enable, float movementSpeed = 5f)
    {
        if (enable)
        {
            movementEnabled = true;
            moveSpeed = movementSpeed;
        }
        else
        {
            movementEnabled = false;
        }
    }
    public void Look()
    {
        if (!movementEnabled)
        {
            return;
        }
        rotation.y += Input.GetAxis("Mouse X") * MouseSensitivity;
        rotation.x -= Input.GetAxis("Mouse Y") * MouseSensitivity;
        rotation.x = Mathf.Clamp(rotation.x, clamp * -1f, clamp);

        Camera.main.transform.parent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        transform.localRotation = Quaternion.Euler(0, rotation.y, 0);
    }

    void Update()
    {
        HandleRaycastInteraction();
        Move();
        Look();
    }

    public void OnPickup(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }

        var pickedUp = false;
        if (isItem && heldItem == null)
        {
            if (targetedObject.layer == LayerMask.NameToLayer("InteractableItem"))
            {
                pickedUp = true;
                PickUpItem(targetedObject);
            }
            else if (nestedChild != null && nestedChild.layer == LayerMask.NameToLayer("InteractableItem"))
            {
                PickUpItem(nestedChild);
                pickedUp = true;
            }
        }
        else if (isItem && heldItem != null)
        {
            DropItem();
            if (targetedObject.layer == LayerMask.NameToLayer("InteractableItem"))
            {
                pickedUp = true;
                PickUpItem(targetedObject);
            }
            else if (nestedChild != null && nestedChild.layer == LayerMask.NameToLayer("InteractableItem"))
            {
                PickUpItem(nestedChild);
                pickedUp = true;
            }
        }

        if (pickedUp)
        {
            grabGizmos grabHelper = findGrabGizmos(heldItem);

            if (!grabHelper)
            {
                beginPickupAnimation();
            }
        }
    }

    public void beginPickupAnimation()
    {
        m_animator.SetBool("PickUp", true);
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }
        if (heldItem != null)
        {
            DropItem();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }
        if (isDeliveryLocation)
        {
            if (targetedObject.layer == LayerMask.NameToLayer("DeliveryLocation"))
            {

                if (targetedObject.gameObject.transform.parent && targetedObject.gameObject.transform.parent.GetComponent<taskHost>())
                {
                    InteractWithLocation(targetedObject.gameObject.transform.parent.gameObject);
                }
                else
                {
                    InteractWithLocation(targetedObject);
                }
            }
            else if (nestedChild != null && nestedChild.layer == LayerMask.NameToLayer("DeliveryLocation"))
            {
                InteractWithLocation(nestedChild);
            }
        }
    }
    public void OnToggleTasks(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }
        if(canOpenTaskMenu == false)
        {
            return;
        }
        taskUIManager.showHideTaskUI();
    }

    public void HandleRaycastInteraction()
    {
        int layerMask = (1 << LayerMask.NameToLayer("InteractableItem")) | (1 << LayerMask.NameToLayer("DeliveryLocation")) | (1 << LayerMask.NameToLayer("Environment"));
        RaycastHit hit;
        if (Camera.main == null)
        {
            return;
        }

        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, playerReach, layerMask);

        if (lastTargetedObject != null && lastTargetedObject != hit.collider?.gameObject)
        {
            RemoveOutline(lastTargetedObject);
            lastTargetedObject = null;
        }

        if (hit.collider != null)
        {
            targetedObject = null;
            isItem = false;
            isDeliveryLocation = false;
            nestedChild = null;

            if (hit.collider.gameObject.CompareTag("NestedDeliveryLocation"))
            {
                if (TaskListContainerInstance.taskList.FindNestedObject(hit.collider.gameObject, false).gameObject.transform.parent == hit.collider.gameObject.transform)
                {
                    targetedObject = hit.collider.gameObject;
                    nestedChild = TaskListContainerInstance.taskList.FindNestedObject(targetedObject, false);
                }
            }
            else if (hit.collider.gameObject.CompareTag("NestedItem"))
            {
                if (TaskListContainerInstance.taskList.FindNestedObject(hit.collider.gameObject, true).gameObject.transform.parent == hit.collider.gameObject.transform
                || TaskListContainerInstance.taskList.FindNestedObject(hit.collider.gameObject, true).gameObject.transform.parent.transform.parent == hit.collider.gameObject.transform)
                {
                    targetedObject = hit.collider.gameObject;
                    nestedChild = TaskListContainerInstance.taskList.FindNestedObject(targetedObject, true);
                }
            }
            else if (hit.collider.gameObject.transform.parent)
            {
                if (hit.collider.gameObject.transform.parent.CompareTag("NestedItem"))
                {
                    if (TaskListContainerInstance.taskList.FindNestedObject(hit.collider.gameObject.transform.parent.gameObject, true).gameObject.transform.parent == hit.collider.gameObject.transform.parent)
                    {
                        targetedObject = hit.collider.gameObject.transform.parent.gameObject;
                        nestedChild = hit.collider.gameObject;
                    }
                }
                else if (hit.collider.gameObject.transform.parent.CompareTag("NestedDeliveryLocation"))
                {
                    if (TaskListContainerInstance.taskList.FindNestedObject(hit.collider.gameObject.transform.parent.gameObject, false).gameObject.transform.parent == hit.collider.gameObject.transform.parent)
                    {
                        targetedObject = hit.collider.gameObject.transform.parent.gameObject;
                        nestedChild = hit.collider.gameObject;
                    }
                }
            }

            if (targetedObject != null && nestedChild != null)
            {
                isItem = true;
                isDeliveryLocation = true;
            }
            else
            {
                targetedObject = hit.collider.gameObject;
                isItem = targetedObject.layer == LayerMask.NameToLayer("InteractableItem");
                isDeliveryLocation = targetedObject.layer == LayerMask.NameToLayer("DeliveryLocation");
            }

             if (targetedObject != null)
            {
                lastTargetedObject = targetedObject;
                ApplyOutline(targetedObject);
            }

            HandleToolTips();
        }
        else
        {
            pickupTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
            pickupTooltip.SetActive(false);
            interactTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
            interactTooltip.SetActive(false);
            targetedObject = null;
            isItem = false;
            isDeliveryLocation = false;

            if (heldItem != null && heldItem.GetComponent<taskItem>())
            {
                dropTooltip.GetComponentInChildren<TMP_Text>(true).text = heldItem.GetComponent<taskItem>().objectName;
                dropTooltip.SetActive(true);
            }
            else
            {
                dropTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
                dropTooltip.SetActive(false);
                tooltipCanvas.SetActive(false);
            }
        }

        // if (Input.GetKeyDown(KeyCode.X))
        // {
        //     TestUpdateAvailability();
        // }
    }

     private void ApplyOutline(GameObject obj)
    {
        if (obj.GetComponent<OutlineController>() == null) return;
        Transform outlineChild = obj.GetComponent<OutlineController>().instantiatedObject.transform;
        if (outlineChild != null)
        {
            Renderer renderer = outlineChild.GetComponent<Renderer>();
            if (renderer != null && renderer.materials.Length > 0)
            {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_ShaderOn", 1);
                obj.GetComponent<OutlineController>().HandleThickness(propertyBlock);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
    private void RemoveOutline(GameObject obj)
    {
        if (obj.GetComponent<OutlineController>() == null) return;
        Transform outlineChild = obj.GetComponent<OutlineController>().instantiatedObject.transform;
        if (outlineChild != null)
        {
            Renderer renderer = outlineChild.GetComponent<Renderer>();
            if (renderer != null && renderer.materials.Length > 0)
            {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_ShaderOn", 0);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }

    public void HandleToolTips()
    {
        tooltipCanvas.SetActive(true);
        if (isItem)
        {
            if (heldItem == null)
            {
                pickupTooltip.SetActive(true);
                dropTooltip.SetActive(false);
            }
            else
            {
                pickupTooltip.SetActive(true);
                dropTooltip.SetActive(true);
                dropTooltip.GetComponentInChildren<TMP_Text>(true).text = heldItem.GetComponent<taskItem>().objectName;
            }

            if (isDeliveryLocation)
            {
                if (targetedObject.layer == LayerMask.NameToLayer("InteractableItem") && nestedChild != null)
                {
                    pickupTooltip.GetComponentInChildren<TMP_Text>(true).text = targetedObject.GetComponent<taskItem>().objectName;

                    if (nestedChild.GetComponent<taskHost>())
                    {
                        interactTooltip.GetComponentInChildren<TMP_Text>(true).text = taskHostNameDefault(targetedObject.GetComponent<taskHost>());
                    }
                    else if (nestedChild.GetComponent<taskItem>())
                    {
                        interactTooltip.GetComponentInChildren<TMP_Text>(true).text = nestedChild.GetComponent<taskItem>().objectName;
                    }
                }
                else if (nestedChild != null && nestedChild.layer == LayerMask.NameToLayer("InteractableItem"))
                {
                    pickupTooltip.GetComponentInChildren<TMP_Text>(true).text = nestedChild.GetComponent<taskItem>().objectName;
                    if (targetedObject.GetComponent<taskHost>())
                    {
                        interactTooltip.GetComponentInChildren<TMP_Text>(true).text = taskHostNameDefault(targetedObject.GetComponent<taskHost>());
                    }
                    else if (targetedObject.GetComponent<taskItem>())
                    {
                        interactTooltip.GetComponentInChildren<TMP_Text>(true).text = targetedObject.GetComponent<taskItem>().objectName;
                    }
                }
                interactTooltip.SetActive(true);
            }
            else
            {
                if (targetedObject.GetComponent<taskItem>())
                {
                    pickupTooltip.GetComponentInChildren<TMP_Text>(true).text = targetedObject.GetComponent<taskItem>().objectName;
                    interactTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
                    interactTooltip.SetActive(false);
                }
                else
                {
                    pickupTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
                    interactTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
                    interactTooltip.SetActive(false);
                }

            }
        }
        else if (isDeliveryLocation)
        {
            pickupTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
            pickupTooltip.SetActive(false);
            interactTooltip.SetActive(false);
            if (targetedObject.GetComponent<taskHost>())
            {
                //interactTooltip.GetComponentInChildren<TMP_Text>(true).text = "taskHost " + targetedObject.GetComponent<taskHost>().taskHostNumber;
                interactTooltip.GetComponentInChildren<TMP_Text>(true).text = taskHostNameDefault(targetedObject.GetComponent<taskHost>());
            }
            else if (targetedObject.transform.parent != null && targetedObject.transform.parent.GetComponent<taskHost>())
            {
                if (targetedObject.GetComponent<taskItem>())
                {
                    interactTooltip.GetComponentInChildren<TMP_Text>(true).text = taskHostNameDefault(targetedObject.transform.parent.GetComponent<taskHost>()) + " | " + targetedObject.GetComponent<taskItem>().objectName;
                }
                else
                {
                    interactTooltip.GetComponentInChildren<TMP_Text>(true).text = taskHostNameDefault(targetedObject.transform.parent.GetComponent<taskHost>());
                }
            }
            else if (targetedObject.transform.parent != null && targetedObject.transform.parent.transform.parent != null && targetedObject.transform.parent.transform.parent.GetComponent<taskHost>())
            {
                if(targetedObject.GetComponent<taskItem>())
                {
                    interactTooltip.GetComponentInChildren<TMP_Text>(true).text = taskHostNameDefault(targetedObject.transform.parent.transform.parent.GetComponent<taskHost>()) + " | " + targetedObject.GetComponent<taskItem>().objectName;
                }
                else
                {
                    interactTooltip.GetComponentInChildren<TMP_Text>(true).text = taskHostNameDefault(targetedObject.transform.parent.transform.parent.GetComponent<taskHost>());
                }
            }
            else if (targetedObject.GetComponent<taskItem>())
            {
                interactTooltip.GetComponentInChildren<TMP_Text>(true).text = targetedObject.GetComponent<taskItem>().objectName;
            }

            interactTooltip.SetActive(true);
            if (heldItem != null)
            {
                dropTooltip.SetActive(true);
                dropTooltip.GetComponentInChildren<TMP_Text>(true).text = heldItem.GetComponent<taskItem>().objectName;
            }
        }
        else
        {
            pickupTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
            pickupTooltip.SetActive(false);
            interactTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
            interactTooltip.SetActive(false);
        }
    }

    private string taskHostNameDefault(taskHost taskHost)
    {
        if (taskHost != null)
        {
            if(taskHost.taskHostName != ""){
                return taskHost.taskHostName;

            }
            else{
                if(taskHost.gameObject.GetComponent<taskHost>() != null){
                    return $"taskHost {taskHost.taskHostNumber}";
                }
                else{
                    return $"taskHost {taskHost.taskHostNumber}";
                }
            }
        }
        else{
            return "defaultTaskHost";
        }
    }

    private int taskIndex = 0;
    private int currentStepIndex = 0;

    public void TestUpdateAvailability()
    {
        TaskListContainer.TaskList.Task task = TaskListContainerInstance.taskList.tasks[taskIndex];
        Debug.Log("TestUpdateAvailability: Task Name - " + task.taskName);

        if (currentStepIndex >= task.steps.Count)
        {
            Debug.LogWarning("TestUpdateAvailability: All steps completed for the task, resetting to 0, incrementing TaskIndex.");
            currentStepIndex = 0;
            taskIndex++;
            return;
        }

        TaskListContainer.TaskList.Task.TaskStep stepToComplete = task.steps[currentStepIndex];
        string itemName = stepToComplete.itemPrefab != null ? stepToComplete.itemPrefab.name : "None";
        string locationName = stepToComplete.deliveryLocationPrefab != null ? stepToComplete.deliveryLocationPrefab.name : "None";
        Debug.Log("TestUpdateAvailability: Step to complete - " + currentStepIndex + " Item/Location: " + itemName + " -> " + locationName);

        taskHost taskHost = taskManager.taskHostToTasks.FirstOrDefault(p => p.Value.Any(t => t.steps.Contains(stepToComplete))).Key;

        if (taskHost != null)
        {
            Debug.Log("TestUpdateAvailability: taskHost found for step " + " | taskHost: " + taskHost.taskHostNumber);
            //CompleteTaskStep(stepToComplete, taskHost);
            //taskManager.updateAvailability(stepToComplete);
            currentStepIndex++;
        }
    }

    public void PickUpItem(GameObject itemToPickup, bool skipPlacement = false)
    {
        if(heldItem != null)
        {
            DropItem();
        }
        if (itemToPickup.transform.parent != null && itemToPickup.transform.parent.CompareTag("NestedItem"))
        {
            itemToPickup.transform.parent.tag = "Untagged";
        }

        Rigidbody rb = itemToPickup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        else if (itemToPickup.transform.parent != null && itemToPickup.transform.parent.GetComponent<Rigidbody>() != null)
        {
            rb = itemToPickup.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        grabGizmos grabHelper = findGrabGizmos(itemToPickup);
        PlaceGizmos placeHelper = itemToPickup.GetComponent<PlaceGizmos>();

        if(placeHelper != null)
        {
            placeHelper.unPlace();
        }

        addItemCollidersToList(itemToPickup);
        createFingerTouchCollider(itemToPickup);

        if (grabHelper != null && skipPlacement == false)
        {
            itemToPickup.transform.SetParent(playerHand0Transform);
            grabHelper.beginPickup();
        }
        else
        {
            itemToPickup.transform.position = playerHandTransform.transform.position;
            itemToPickup.transform.SetParent(playerHandTransform);
            itemToPickup.transform.rotation = playerHandTransform.transform.rotation;
            if(skipPlacement == false)
            {
                toggleHeldItemColliders(false);
            }
        }


        heldItem = itemToPickup;

    }

    private void addItemCollidersToList(GameObject itemToPickup)
    {
        Collider[] itemCollider = itemToPickup.GetComponentsInChildren<Collider>();
        enabledColliders.Clear();
        if (itemCollider != null)
        {
            foreach (Collider itemChild in itemCollider)
            {
                if (itemChild != null)
                {
                    if (itemChild.enabled && (itemChild.gameObject.layer == LayerMask.NameToLayer("InteractableItem") || itemChild.gameObject.layer == LayerMask.NameToLayer("DeliveryLocation")))
                    {
                        enabledColliders.Add(itemChild);
                    }
                }
            }
        }
    }

    private void createFingerTouchCollider(GameObject itemToPickup)
    {
        //check the object for a child with the tag fingerTouchCollider
        foreach (Transform child in itemToPickup.transform)
        {
            if (child.CompareTag("FingerTouchCollider"))
            {
                Debug.Log("FingerTouchCollider already exists");
                return;
            }
        }

        //create an empty gameObject as a child of the itemToPickup
        var createFingerTouchCollider = new GameObject("FingerTouchCollider");
        createFingerTouchCollider.transform.SetParent(itemToPickup.transform);
        createFingerTouchCollider.transform.localPosition = Vector3.zero;
        createFingerTouchCollider.transform.localRotation = Quaternion.identity;
        createFingerTouchCollider.transform.localScale = Vector3.one;
        createFingerTouchCollider.tag = "FingerTouchCollider";
        createFingerTouchCollider.layer = LayerMask.NameToLayer("FingerTouchCollider");

        //copy the collider of the itemToPickup
        Collider itemCollider = itemToPickup.GetComponent<Collider>();
        if(itemCollider != null)
        {
            Collider newCollider = createFingerTouchCollider.AddComponent(itemCollider.GetType()) as Collider;
            if (newCollider != null)
            {
                // Copy the data from the itemCollider to the new collider
                if (itemCollider is BoxCollider originalBoxCollider && newCollider is BoxCollider newBoxCollider)
                {
                    newBoxCollider.center = originalBoxCollider.center;
                    newBoxCollider.size = originalBoxCollider.size;
                }
                else if (itemCollider is SphereCollider originalSphereCollider && newCollider is SphereCollider newSphereCollider)
                {
                    newSphereCollider.center = originalSphereCollider.center;
                    newSphereCollider.radius = originalSphereCollider.radius;
                }
                else if (itemCollider is CapsuleCollider originalCapsuleCollider && newCollider is CapsuleCollider newCapsuleCollider)
                {
                    newCapsuleCollider.center = originalCapsuleCollider.center;
                    newCapsuleCollider.radius = originalCapsuleCollider.radius;
                    newCapsuleCollider.height = originalCapsuleCollider.height;
                    newCapsuleCollider.direction = originalCapsuleCollider.direction;
                }
                else if (itemCollider is MeshCollider originalMeshCollider && newCollider is MeshCollider newMeshCollider)
                {
                    newMeshCollider.sharedMesh = originalMeshCollider.sharedMesh;
                    newMeshCollider.convex = originalMeshCollider.convex;
                }
                else
                {
                    Debug.LogWarning("Unsupported collider type: " + itemCollider.GetType());
                }
            }
        }
        else
        {
            Debug.LogWarning("No collider found on item to pickup.");
        }
    }

    public void toggleHeldItemColliders(bool enable)
    {
        if(heldItem == null)
        {
            return;
        }
        if(enabledColliders.Count > 0)
        {
            foreach (Collider reenableCollider in enabledColliders)
            {
                reenableCollider.enabled = enable;
            }
        }
    }

    grabGizmos findGrabGizmos(GameObject itemToPickup)
    {
        grabGizmos grabHelper = null;

        foreach (Transform child in itemToPickup.transform)
        {
            if (child.GetComponent<grabGizmos>() != null)
            {
                grabHelper = child.GetComponent<grabGizmos>();
                break;
            }
        }

        return grabHelper;
    }

    void OnDrawGizmosSelected()
    {
        //draw a line outwards in the directions that the grabVector and the upVector should be facing
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerHand0Transform.position, -playerHand0Transform.forward);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(playerHand0Transform.position, playerHand0Transform.up);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerHand0Transform.position, playerHand0Transform.right);
    }

    void DropItem(bool destroyAfterDrop = false)
    {
        if (!heldItem)
        {
            return;
        }

        m_animator.SetBool("PickUp", false);

        dropTooltip.GetComponentInChildren<TMP_Text>(true).text = "";
        dropTooltip.SetActive(false);

        if (destroyAfterDrop)
        {
            Destroy(heldItem);
            heldItem = null;
            return;
        }

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if(heldItem.GetComponent<Collider>() != null)
        {
            heldItem.GetComponent<Collider>().enabled = true;
        }
        toggleHeldItemColliders(true);
        enabledColliders.Clear();
        
        heldItem.transform.SetParent(null);

        grabGizmos grabHelper = findGrabGizmos(heldItem);

        if(grabHelper != null)
        {
            grabHelper.drop();
        }
        heldItem = null;
    }

    void InteractWithLocation(GameObject locationToInteract)
    {
        Debug.Log("---- Starting Interaction ----");
        Debug.Log($"Targeted Object: {targetedObject.name}");
        Debug.Log($"Location Object: {locationToInteract.name}");

        TaskListContainer.TaskList.Task.TaskStep potentialStep = GetCurrentStep(locationToInteract, heldItem);

        var door = locationToInteract.GetComponent<interactDoor>();
        var talk = locationToInteract.GetComponent<talkNpc>();

        if (potentialStep == null)
        {
            if(door != null && door.freeOpen)
            {
                door.interactWithDoor();
                return;
            }

            if(talk != null && talk.freeTalk)
            {
                talk.Talk();
                return;
            }

            Debug.LogWarning("No potential step found.");
            taskUIManager.ShowInteractionPanel(false);
            return;
        }

        if(door != null)
        {
            door.interactWithDoor();
        }

        if(talk != null)
        {
            talk.freeTalk = true;
            talk.Talk();
        }
        
        taskHost taskHost = taskManager.taskHostToTasks.FirstOrDefault(p => p.Value.Any(t => t.steps.Contains(potentialStep))).Key; ;

        string itemName = potentialStep.itemPrefab != null ? potentialStep.itemPrefab.name : "None";
        string locationName = potentialStep.deliveryLocationPrefab != null ? potentialStep.deliveryLocationPrefab.name : "None";
        if (taskHost != null)
        {
            Debug.Log($"Completing step: Item - {itemName}, Location - {locationName} for taskHost {taskHost.name + taskHost.taskHostNumber}");
            CompleteTaskStep(potentialStep, taskHost, locationToInteract);
        }
    }

    private TaskListContainer.TaskList.Task.TaskStep GetCurrentStep(GameObject locationObject, GameObject itemObject)
    {
        var updatedItem = "";
        Debug.Log("---- Finding Current Step ----");

        if(taskManager == null)
        {
            return null;
        }

        List<TaskListContainer.TaskList.Task.TaskStep> potentialSteps = new List<TaskListContainer.TaskList.Task.TaskStep>();

        string locationName = "";
        if (locationObject.GetComponent<taskHost>())
        {
            taskHost taskHost = locationObject.GetComponent<taskHost>();
            locationName = taskHost.taskHostNumber.ToString();
        }
        else if (locationObject.GetComponent<taskItem>())
        {
            locationName = locationObject.GetComponent<taskItem>().objectName;
        }

        if (taskManager.locationToSteps.ContainsKey(locationName))
        {
            potentialSteps.AddRange(taskManager.locationToSteps[locationName]);
        }

        List<TaskListContainer.TaskList.Task.TaskStep> itemSteps = new List<TaskListContainer.TaskList.Task.TaskStep>();

        if (itemObject != null && itemObject.GetComponent<taskItem>())
        {
            updatedItem = itemObject.GetComponent<taskItem>().objectName;
            if (taskManager.itemToSteps.ContainsKey(updatedItem))
            {
                itemSteps = taskManager.itemToSteps[updatedItem];
            }
        }
        else if (itemObject == null && taskManager.noItemSteps.Count > 0)
        {
            itemSteps.AddRange(taskManager.noItemSteps);
        }

        potentialSteps = potentialSteps.Intersect(itemSteps).ToList();

        if (taskManager.activeUrgentTask != null && potentialSteps.Count > 1)
        {
            var urgentStep = potentialSteps.FirstOrDefault(step => taskManager.activeUrgentTask.steps.Contains(step) &&
                                                                  taskManager.taskStepAvailability[step].isAvailable &&
                                                                  !taskManager.taskStepAvailability[step].isCompleted);
            if (urgentStep != null)
            {
                return urgentStep;
            }
        }

        foreach (var step in potentialSteps)
        {
            if (taskManager.taskStepAvailability[step].isAvailable && !taskManager.taskStepAvailability[step].isCompleted)
            {
                bool itemMatches = step.itemPrefab == null || (itemObject != null && updatedItem == step.itemPrefab.GetComponent<taskItem>().objectName);
                bool locationMatches = false;
                if (locationObject.GetComponent<taskHost>())
                {
                    locationMatches = locationName == locationObject.GetComponent<taskHost>().taskHostNumber.ToString();
                }
                else if (locationObject.GetComponent<taskItem>())
                {
                    locationMatches = locationName == step.deliveryLocationPrefab.GetComponent<taskItem>().objectName;
                }

                // Check if the step is part of an inactive urgent task
                TaskListContainer.TaskList.Task taskForStep = taskManager.tasks.FirstOrDefault(t => t.steps.Contains(step));
                if (taskForStep != null && taskForStep.isUrgent && taskManager.activeUrgentTask != taskForStep)
                {
                    Debug.LogWarning($"Step is part of an inactive urgent task: {taskForStep.taskName}");
                    continue; // Skip this step
                }

                if (itemMatches && locationMatches)
                {
                    return step;
                }
            }
        }
        Debug.Log("No matching step found: " + (updatedItem != "" ? updatedItem : itemObject?.name) + " -> " + (locationName != "" ? locationName : locationObject.name));
        return null;
    }

    private void CompleteTaskStep(TaskListContainer.TaskList.Task.TaskStep step, taskHost taskHost, GameObject locationObject)
    {
        TaskListContainer.TaskList.Task task = TaskListContainerInstance.taskList.tasks
            .FirstOrDefault(t => t.steps.Contains(step));

        if (task != null && taskHost != null)
        {
            int stepIndex = task.steps.IndexOf(step);
            Debug.Log($"Completing task step {stepIndex} of task {task.taskName} for taskHost {taskHost.taskHostNumber}");

            taskManager.updateAvailability(step);

            if (task.isUrgent)
            {
                if(taskHost.gameObject.GetComponent<Patient>() != null){
                    taskHost.gameObject.GetComponent<Patient>().CompleteUrgentTaskStep();
                }
                
                if(scoreUIManager != null){
                    scoreUIManager.completeUrgentTaskStep();
                }
            }
            else{
                if(scoreUIManager != null){
                    scoreUIManager.completeMainTaskStep();
                }
            }

            if (taskUIManager != null)
            {
                taskUIManager.UseItemForStep(heldItem);
            }
            toggleHeldItemColliders(true);

            if(step.swapObject != null && heldItem != null)
            {
                Destroy(heldItem);
                var swapObj = Instantiate(step.swapObject);
                PickUpItem(swapObj, step.placeItemInDeliverySlot);
            }

            if (step.placeItemInDeliverySlot && heldItem != null)
            {
                placeInSlot(locationObject, step.lockInSlot);
            }

            if (step.consumeItem && heldItem != null)
            {
                DropItem(step.consumeItem);
            }
            
            switch(step.playerArmAnim){
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.GenericMedicalAction:
                    m_animator.SetTrigger("MedicalInteraction");
                    break;
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.GiveItem:
                    m_animator.SetTrigger("HandOverGive");
                    break;
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.PlaceItem:
                    m_animator.SetTrigger("PlaceItem");
                    break;
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.HangIVBag:
                    m_animator.SetTrigger("HangIV");
                    break;
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.IVNeedle:
                    m_animator.SetTrigger("InsertIV");
                    break;
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.TakeTemperature:
                    m_animator.SetTrigger("TakeTemperature");
                    break;
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.DoorOpenClose:
                    if(heldItem == null){
                        m_animator.SetTrigger("OpenClose");
                    }
                    else{
                        Debug.LogWarning("Cannot DoorOpenClose while holding an item. No animation will play."); //TODO: provide an alternative to play in off-hand if holding an item
                    }
                    break;
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.PointPress:

                    if(heldItem == null){
                        m_animator.SetTrigger("PointPress");
                    }
                    else{
                        Debug.LogWarning("Cannot PointPress while holding an item. No animation will play."); //TODO: provide an alternative to play in off-hand if holding an item
                    }
                    break;
                case TaskListContainer.TaskList.Task.TaskStep.playerArmAnimation.RubbingHands:
                    if(heldItem == null){
                        m_animator.SetTrigger("RubHands");
                    }
                    else{
                        Debug.LogWarning("Cannot RubHands while holding an item. No animation will play."); //TODO: feedback to player, force drop item, or restrict interaction
                    }
                    break;
            }

            if (step.removeDelivery && locationObject != null)
            {
                locationObject.layer = LayerMask.NameToLayer("Default");
                if (locationObject.transform.parent != null && locationObject.transform.parent.gameObject.CompareTag("NestedDeliveryLocation"))
                {
                    locationObject.transform.parent.gameObject.tag = "Untagged";
                }
            }

            taskUIManager.ShowInteractionPanel(true);
        }
    }

    private void placeInSlot(GameObject locationObject, bool lockInSlot = false)
    {
        Transform[] childTransforms = locationObject.GetComponentsInChildren<Transform>();
        var placedItem = false;
        foreach (Transform child in childTransforms)
        {
            if (child == locationObject.transform)
            {
                continue;
            }

            if (child.CompareTag("DeliverySlot") && child.childCount == 0)
            {
                m_animator.SetBool("PickUp", false);
                Rigidbody rb = heldItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.WakeUp();
                }

                grabGizmos grabHelper = findGrabGizmos(heldItem);
                if(grabHelper != null)
                {
                    grabHelper.drop();
                }

                placedItem = true;
                

                if(heldItem.GetComponent<PlaceGizmos>() != null && child.GetComponent<deliverySlot>() != null)
                {
                    heldItem.GetComponent<PlaceGizmos>().beginPlace(child.gameObject, lockInSlot);
                }
                else{
                    heldItem.transform.position = child.position;
                    heldItem.transform.rotation = child.rotation;
                }

                if(!lockInSlot)
                {
                    locationObject.tag = "NestedItem";
                }
                else{
                    heldItem.layer = LayerMask.NameToLayer("Default");
                }
                
                heldItem.transform.localScale = new Vector3(heldItem.transform.localScale.x * child.localScale.x, heldItem.transform.localScale.y * child.localScale.y, heldItem.transform.localScale.z * child.localScale.z);
                heldItem.transform.SetParent(child);
                heldItem = null;
                break;
            }
        }
        if (!placedItem)
        {
            Debug.LogWarning("No DeliverySlots available on location.");
            DropItem();
        }
    }
}