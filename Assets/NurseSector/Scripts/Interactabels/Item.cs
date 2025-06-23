//Metaverse-Solutions Author: Amber Voskamp
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Item : MonoBehaviour
{
    /// <summary>
    /// This script is to have this gameobject be pickupable by the player. 
    /// First of we need to set a trigger area so we can show a message that this object can be pickup
    /// </summary>
    public Vector3 pickupPositionOffset;
    public Vector3 pickupRotationOffset;

    [SerializeField]
    private Rigidbody m_rigidbody;

    private InteractableManager m_interactableManager;


    // Start is called before the first frame update
    void Start()
    {
        InteractableManager[] interactableManagers = FindObjectsByType<InteractableManager>(FindObjectsSortMode.None);
        //   InteractableManager[] interactableManagers = FindObjectsOfType(typeof(InteractableManager)) as InteractableManager[];


        //Check if there is an interactableManager or if there are multiple there should only be one in the whole scene
        if (interactableManagers.Length != 1)
        {
            UnityEngine.Debug.LogError($"[Object] interactableManagers couldn't be found only one should exist");
            return;
        }
        
        m_interactableManager = interactableManagers[0];
        m_interactableManager.AddPickup(this);
    }
    public void SetOffsets()
    {
        this.gameObject.transform.SetLocalPositionAndRotation(pickupPositionOffset, Quaternion.Euler(pickupRotationOffset));
    }

    public void ToggleGravity(bool toggle)
    {
        m_rigidbody.useGravity = toggle;
        m_rigidbody.isKinematic = !toggle;
    }

}
