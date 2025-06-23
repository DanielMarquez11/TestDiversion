using TMPro;
using UnityEngine;

public class PickupMessage : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_itemPickupMessage;
    

    [SerializeField]
    private TMP_Text m_interactablePickupMessage;

    void Start()
    {
        m_itemPickupMessage.gameObject.SetActive(false);
        m_interactablePickupMessage.gameObject.SetActive(false);
    }

    public void SetItemPickupMessage(bool enabled, string selectedName)
    {
         m_itemPickupMessage.gameObject.SetActive(enabled);
        m_itemPickupMessage.text = enabled ? selectedName : "";
    }

    public void SetInteractablePickupMessage(bool enabled, string selectedName)
    {
        m_interactablePickupMessage.gameObject.SetActive(enabled);
        m_interactablePickupMessage.text = enabled ? selectedName : "";
    }
}
