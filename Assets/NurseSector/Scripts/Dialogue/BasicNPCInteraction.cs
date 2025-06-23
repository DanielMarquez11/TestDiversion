using UnityEngine;
using TMPro;

public class BasicNPCInteraction : MonoBehaviour
{
    [SerializeField]
    private string dialogueText;

    private TextMeshProUGUI textOutput;

    private void Start()
    {
        textOutput = GameObject.Find("DialogueDisplay/DialogueUI/BaseDialoguePanel/BottomTextBox/Text (TMP)")?.GetComponent<TextMeshProUGUI>();
        Debug.Log(textOutput != null ? "Found Textbox" : "Textbox not found");
    }

    public void Talk()
    {
        textOutput.text = dialogueText;
        Debug.Log("Text set to textbox");
    }
}