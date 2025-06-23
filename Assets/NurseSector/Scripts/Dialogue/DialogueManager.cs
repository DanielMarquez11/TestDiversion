using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private float talkRange;

    private GameObject player;
    private ItemInteraction itemInteraction;
    private GameObject baseDialoguePanel;

    private bool isTalking = false;

    void Start()
    {
        InitializeDialoguePanels();
        InitializePlayer();
        ShowBaseDialoguePanel(false);
        //talkRange = 2.0f;
    }

    void Update()
    {

    }

    public void OnTalk(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }

        if (!isTalking)
        {
            Debug.Log("Try to talk");
            CheckPlayerDistanceToNPC();
            return;
        }

        if (isTalking)
        {
            ShowBaseDialoguePanel(false);
            isTalking = false;
            return;
        }
    }

    
    private void InitializeDialoguePanels()
    {
        baseDialoguePanel = GameObject.Find("DialogueDisplay/DialogueUI/BaseDialoguePanel");
        Debug.Log(baseDialoguePanel != null ? "Found BaseDialoguePanel" : "BaseDialoguePanel not found");
    }

    private void InitializePlayer()
    {
        var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        //grab the parent of the parent of the mainCamera to find the Player
        player = mainCamera.transform.parent.gameObject.transform.parent.gameObject;
        
        //In case the player game object is handled differently it might still be found with the player tag
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        Debug.Log(player != null ? "Found Player" : "Player not found");
        itemInteraction = player.GetComponent<ItemInteraction>();
    }

    private void ShowBaseDialoguePanel(bool showPanel)
    {
        baseDialoguePanel.SetActive(showPanel);
        itemInteraction.EnableDisableMovement(!showPanel);
    }

    private void CheckPlayerDistanceToNPC()
    {
        Collider[] NPCColliderArray = Physics.OverlapSphere(player.transform.position, talkRange);

        foreach (Collider collider in NPCColliderArray)
        {
            Debug.Log(collider.name + " checked for NPC");
            if (collider.TryGetComponent(out BasicNPCInteraction basicNPCInteraction)) 
            {
                Debug.Log("NPC Found: " + collider.name);
                basicNPCInteraction.Talk();
                ShowBaseDialoguePanel(true);
                isTalking = true;
                return;
            }
        }
    }
}