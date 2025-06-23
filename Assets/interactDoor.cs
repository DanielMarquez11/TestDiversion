using UnityEngine;

public class interactDoor : MonoBehaviour
{
    private Animator m_animator;
    [SerializeField]
    private DoorMotionType doorMotionType;

    [Tooltip("If false, the door will only open if it is assigned in an available task step.")]
    public bool freeOpen = false;
    private enum DoorMotionType
    {
        None,
        HingeLeft,
        HingeRight,
        SlideLeft,
        SlideRight,
        HingeLeft90Deg,
        FlipSwitchDown,
        HingeLeftYAxis
    }

    [Range(-1, 9999)]
    public int lockCode = -1;

    private LockManager lockManager;
    private LevelTransition levelTransition; // Reference to LevelTransition if this is a transition door


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_animator = GetComponent<Animator>();

        lockManager = FindFirstObjectByType<LockManager>();

        levelTransition = GetComponent<LevelTransition>(); // Check if this door is also a transition door

        if (doorMotionType != DoorMotionType.None && m_animator != null)
        {
            m_animator.SetInteger("doorType", (int)doorMotionType);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void interactWithDoor()
    {
        if (lockCode >= 0 && lockManager != null)
        {
            lockManager.SetLockCode(lockCode, this);
        }
        else{
            openDoor();
        }
    }

    public void openDoor()
    {
        if(m_animator != null)
        {
            m_animator.SetBool("isOpen", !m_animator.GetBool("isOpen"));

            if(lockCode >= 0 && lockManager != null)
            {
                lockCode = -1;
                freeOpen = true;
            }
        }


        if (levelTransition != null)
        {
            // If this is a transition door, trigger level transition instead of animation
            levelTransition.StartLevelTransition();
            if(levelTransition.isLocked) return; // Prevent locking player if door is locked
            levelTransition.DisablePlayerMovement();
            return;
        }
    }
}
