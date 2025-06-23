//Metaverse-Solutions Author: Amber Voskamp
using System.Collections;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public Camera playerCamera;
    [SerializeField]
    private InteractableManager m_interactableManager;
    [SerializeField]
    private GameObject m_handSocket;
    [SerializeField]
    private Animator m_animator;

    [Header("Movement")]
    [Tooltip("The speed of the movement of the player")]
    public float speed = 6.0f;
    [Tooltip("How fast a player will fall")]
    public float gravity = -9.8f;

    [Header("Mouse")]
    [Tooltip("Sensitivity for mouse movement")]
    public float mouseSensitivity = 100f;
    [Tooltip("Smoothing factor (optional)")]
    public float smoothing = 5f;
    [Tooltip("The max angle to look down")]
    public float maxDownwardAngle = 90f;
    [Tooltip("The max angle to look down when arms are active")]
    public float maxDownwardAngleWithArms = 30f;


    private Vector2 m_currentMouseLook;
    private Vector2 m_mouseLookSmooth;

    private Vector3 m_velocity;
    private CharacterController m_controller;
    private bool m_isPaused = false;
    private bool m_areArmsActive = false;

    private Item m_pickedupItem;
    private TaskManager m_taskManager;
    private bool m_animationPlaying;

    void Start()
    {
        m_controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        #region Move
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        m_controller.Move(move * speed * Time.deltaTime);

        if (m_controller.isGrounded && m_velocity.y < 0)
        {
            m_velocity.y = -2f; // Keeps the player grounded
        }

        m_velocity.y += gravity * Time.deltaTime;
        m_controller.Move(m_velocity * Time.deltaTime);
        #endregion

        #region Look rotation
        if (!m_isPaused)
        {
            // Get raw mouse input
            Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // Apply mouse sensitivity and smoothing (optional)
            mouseInput = Vector2.Scale(mouseInput, new Vector2(mouseSensitivity * smoothing * Time.deltaTime, mouseSensitivity * smoothing * Time.deltaTime));

            // Smooth the input
            m_mouseLookSmooth.x = Mathf.Lerp(m_mouseLookSmooth.x, mouseInput.x, 1f / smoothing);
            m_mouseLookSmooth.y = Mathf.Lerp(m_mouseLookSmooth.y, mouseInput.y, 1f / smoothing);

            // Update the current mouse look
            m_currentMouseLook.x += m_mouseLookSmooth.x;
            m_currentMouseLook.y += m_mouseLookSmooth.y;

            if (m_areArmsActive)
            {
                m_currentMouseLook.y = Mathf.Clamp(m_currentMouseLook.y, -maxDownwardAngleWithArms, 90f);
            }
            else
            {
                m_currentMouseLook.y = Mathf.Clamp(m_currentMouseLook.y, -maxDownwardAngle, 90f);
            }

            transform.localRotation = Quaternion.Euler(0f, m_currentMouseLook.x, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(-m_currentMouseLook.y, 0f, 0f);
        }
        #endregion
    }

    public void OnPickup()
    {
        if (m_pickedupItem == null && (m_taskManager == null || (m_taskManager != null && !m_taskManager.gameObject.activeSelf)))
        {
            var pickup = m_interactableManager.GetSelectedPickup();
            if (pickup != null)
            {
                var taskManager = pickup.gameObject.GetComponent<TaskManager>();
                if (taskManager != null)
                {
                    if (m_taskManager == null)
                    {
                        m_taskManager = taskManager;
                        m_animator.SetTrigger("Show");
                        Grab(true, pickup, m_handSocket.transform);
                        m_interactableManager.RemovePickup(pickup);
                    }
                    return;
                }

                m_pickedupItem = pickup;
                if (m_pickedupItem != null)
                {
                    m_animator.SetTrigger("PickUp");
                    Grab(true, m_pickedupItem, m_handSocket.transform);
                }
            }
        }
    }

    public void OnDrop()
    {
        //Check if there is an item to be pickup
        if (m_pickedupItem != null)
        {
            m_animator.SetTrigger("Drop");
            StartCoroutine(WaitForItemDrop());
            Grab(false, m_pickedupItem, m_interactableManager.transform);
            m_pickedupItem = null;
        }
    }

    public void OnInteract()
    {
        var interactable = m_interactableManager.GetSelectedInteractable();
        if (interactable != null)
        {
            if (interactable != null && interactable.CheckIfItemIsOnTheList(m_pickedupItem))
            {
                //Added this in for now so you can see in the console if you finished an task
                UnityEngine.Debug.Log($"[PlayerMovement script] finish task of interacrable: {interactable.name}");
                interactable.FinishTask();
            }
        }
    }

    public void OnToggleTasks()
    {
        if (m_taskManager != null && m_pickedupItem == null && !m_animationPlaying)
        {
            bool toggle = !m_taskManager.gameObject.activeSelf;
            m_animator.SetTrigger(toggle ? "Show" : "Hide");

            if (toggle)
            {
                m_taskManager.gameObject.SetActive(true);
            }

            Grab(true, m_taskManager.gameObject.GetComponent<Item>(), m_handSocket.transform);

            StartCoroutine(WaitForTaskToggle(!toggle));
        }
    }

    private void Grab(bool grab, Item item, Transform parent)
    {
        item.ToggleGravity(!grab);
        item.transform.SetParent(parent, true);
        if (grab)
        {
            //item.transform.position = parent.position;
            item.SetOffsets();
        }

    }

    public bool hasPickupItem()
    {
        return m_pickedupItem != null;
    }

    IEnumerator WaitForTaskToggle(bool isHidden)
    {
        m_animationPlaying = true;
        var animationLenght = m_animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLenght);
        m_animationPlaying = false;

        if (isHidden)
        {
            m_taskManager.gameObject.SetActive(false);
        }
    }

    IEnumerator WaitForItemDrop()
    {
        m_animationPlaying = true;
        var animationLenght = m_animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLenght);
        m_animationPlaying = false;
    }

    public void DisableMovement()
    {
        m_velocity = Vector3.zero; // Stop any falling or moving velocity
        speed = 0; // Set speed to 0 to prevent further movement
        Cursor.lockState = CursorLockMode.None; // Optionally unlock the cursor during transition
        Cursor.visible = false;
    }

    public void EnableMovement(float newSpeed)
    {
        speed = newSpeed; // Restore the player's movement speed
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor for gameplay
        Cursor.visible = false;
    }

}
