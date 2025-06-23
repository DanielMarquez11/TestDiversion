//Metaverse-Solutions Author: Amber Voskamp
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    /// <summary>
    /// This script will handle sending the right interactable object to be interactableObject to the player
    /// </summary>

    public float pickupDistance;
    [SerializeField]
    private TaskManager m_taskManager;
    [SerializeField]
    private HudManager m_hudManager;

    [SerializeField]
    private PlayerMovementScript m_playerMovement;
    private List<Item> m_pickupsObject = new List<Item>();
    private List<Interactable> m_interactableObject = new List<Interactable>();
    private Item m_selectedPickup;
    private Interactable m_selectedInteractable;
    private Vector2 m_middleOfTheScreen;

    public TaskManager getTaskManager { get { return m_taskManager; } }

    private void Start()
    {
        m_middleOfTheScreen = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    private void Update()
    {
        bool hasItemBeenPickup = m_playerMovement.hasPickupItem();

        if (!hasItemBeenPickup)
        {
            #region Get selected pickup
            for (int i = 0; i < m_pickupsObject.Count; i++)
            {
                var pickupObject = m_pickupsObject[i];

                var distance = math.distance(m_pickupsObject[i].transform.position, m_playerMovement.playerCamera.transform.position);

                if (distance < pickupDistance) //Pickup is within range to pickup
                {
                    Vector3 viewPos = m_playerMovement.playerCamera.WorldToViewportPoint(pickupObject.transform.position);
                    bool inView = viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;
                    if (inView) //Pickup is within the view of the player
                    {
                        if (m_selectedPickup == null)
                        {
                            m_selectedPickup = pickupObject;
                        }
                        else if (m_selectedPickup == pickupObject)
                        {
                            continue;
                        }
                        else
                        {
                            var screenSpaceDistSelectedPickup = GetScreenSpaceDistance(m_selectedPickup.transform.position);
                            var screenSpaceDistPickup = GetScreenSpaceDistance(pickupObject.transform.position);

                            if (screenSpaceDistPickup < screenSpaceDistSelectedPickup)
                            {
                                m_selectedPickup = pickupObject;
                            }
                        }

                        m_pickupsObject[i] = pickupObject;
                    }
                    else if (m_selectedPickup == pickupObject)
                    {
                        m_selectedPickup = null; ;
                    }
                }
                else if (m_selectedPickup == pickupObject)
                {
                    m_selectedPickup = null; ;
                }
            }
            #endregion

            UpdatePickupMessage(m_selectedPickup != null);
        }
        else
        {
            // Disable the item pickup message when the player picks up an item added byM.M
            // m_hudManager.pickupMessage.SetItemPickupMessage(false, "");
            // added byM.M

            #region Get interactable
            for (int i = 0; i < m_interactableObject.Count; i++)
            {
                var interactableObject = m_interactableObject[i];

                var distance = math.distance(m_interactableObject[i].transform.position, m_playerMovement.playerCamera.transform.position);

                if (distance < pickupDistance) //Pickup is within range to pickup
                {
                    Vector3 viewPos = m_playerMovement.playerCamera.WorldToViewportPoint(interactableObject.transform.position);
                    bool inView = viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;
                    if (inView) //Pickup is within the view of the player
                    {

                        if (m_selectedInteractable == null)
                        {
                            m_selectedInteractable = interactableObject;
                        }
                        else if (m_selectedInteractable == interactableObject)
                        {
                            continue;
                        }
                        else
                        {
                            var screenSpaceDistSelectedPickup = GetScreenSpaceDistance(m_selectedInteractable.transform.position);
                            var screenSpaceDistPickup = GetScreenSpaceDistance(interactableObject.transform.position);

                            if (screenSpaceDistPickup < screenSpaceDistSelectedPickup)
                            {
                                m_selectedInteractable = interactableObject;
                            }
                        }

                        m_interactableObject[i] = interactableObject;
                    }
                    else if (m_selectedInteractable == interactableObject)
                    {
                        m_selectedInteractable = null; ;
                    }
                }
                else if (m_selectedInteractable == interactableObject)
                {
                    m_selectedInteractable = null; ;
                }
            }
            #endregion

            UpdateInteractableMessage(m_selectedInteractable != null);
        }
    }

    public void AddPickup(Item pickupObject)
    {
        if (!m_pickupsObject.Contains(pickupObject))
        {
            m_pickupsObject.Add(pickupObject);
        }
    }

    public void RemovePickup(Item pickupObject)
    {
        if (m_pickupsObject.Contains(pickupObject))
        {
            m_pickupsObject.Remove(pickupObject);
        }
    }

    public void AddInteractable(in Interactable interactable)
    {
        m_interactableObject.Add(interactable);
    }

    /// <summary>
    /// Get the distance of a gameobject in screen space from the middle of the screen
    /// </summary>
    /// <param name="position">The position of the gameobject</param>
    /// <returns></returns>
    private float GetScreenSpaceDistance(Vector3 position)
    {
        Vector2 screenSpacePosition = m_playerMovement.playerCamera.WorldToScreenPoint(position);
        return math.distance(screenSpacePosition, m_middleOfTheScreen);
    }

    private void UpdatePickupMessage(bool hasSelectedPickup)
    {
        string name = hasSelectedPickup ? m_selectedPickup.name : "";
        //m_hudManager.pickupMessage.SetItemPickupMessage(hasSelectedPickup, name);
    }

    private void UpdateInteractableMessage(bool hasSelectedInteractable)
    {
        string name = hasSelectedInteractable ? m_selectedInteractable.name : "";
        //m_hudManager.pickupMessage.SetInteractablePickupMessage(hasSelectedInteractable, name);
    }


    public Item GetSelectedPickup()
    {
        return m_selectedPickup;
    }

    public Interactable GetSelectedInteractable()
    {
        return m_selectedInteractable;
    }

}
