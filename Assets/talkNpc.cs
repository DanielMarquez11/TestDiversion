using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class talkNpc : MonoBehaviour
{
    private List<GameObject> dialogueUi = new List<GameObject>();

    [SerializeField]
    private List<string> dialogueText = new List<string>();

    private int dialogueIndex = 0;

    private bool isTalking = false;
    private TaskUIManager taskUIManager;

    [Tooltip("If false, the player can talk to the npc only if the npc is assigned in an available and active task step (Defaults to true after initial dialogue interaction). If true, the player can talk to the npc at any time.")]
    public bool freeTalk = false;

    private enum DialogueBoxStyleFormat
    {
        ScreenBottomCenter,
        ScreenMiddleCenter
    }

    [SerializeField]
    private DialogueBoxStyleFormat dialogueBoxStyleFormat;

    void Start()
    {
        taskUIManager = FindFirstObjectByType<TaskUIManager>();

        if (taskUIManager != null)
        {
            dialogueUi = taskUIManager.dialogueUi;
        }
    }

    public void Talk()
    {
        if (!dialogueUi[((int)dialogueBoxStyleFormat)])
        {
            return;
        }

        if (dialogueIndex > dialogueText.Count)
        {
            return;
        }

        if (!isTalking)
        {
            isTalking = true;
            ShowDialogue();
        }
        else
        {
            ContinueDialogue();
        }
    }

    private void ShowDialogue()
    {
        dialogueUi[((int)dialogueBoxStyleFormat)].GetComponentInChildren<TMP_Text>(true).text = dialogueText[dialogueIndex];

        if(!dialogueUi[((int)dialogueBoxStyleFormat)].activeSelf)
        {
            dialogueUi[((int)dialogueBoxStyleFormat)].SetActive(true);
        }

        StartCoroutine("hideOpenDialogueUi");
        if (freeTalk)
        {
            dialogueIndex++;
        }
    }

    private void ContinueDialogue()
    {
        StopCoroutine("hideOpenDialogueUi");
        if (dialogueIndex < dialogueText.Count || !freeTalk)
        {
            ShowDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueUi[((int)dialogueBoxStyleFormat)].GetComponentInChildren<TMP_Text>().text = "";
        dialogueUi[((int)dialogueBoxStyleFormat)].SetActive(false);
        isTalking = false;
        dialogueIndex = 0;
    }

    IEnumerator hideOpenDialogueUi()
    {
        yield return new WaitForSeconds(3);
        EndDialogue();
    }
}