using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    public string levelToLoad; // Scene name to load

    private ItemInteraction itemInteraction; // Reference to ItemInteraction
    private TaskUIManager taskUI; 

    public bool isLocked = false;

    private void Start()
    {
        itemInteraction = FindFirstObjectByType<ItemInteraction>();
        taskUI = FindFirstObjectByType<TaskUIManager>();
        Debug.Log("ItenInteraction script found");
    }

    public void StartLevelTransition()
    {
        if(isLocked) return; // Prevent transition if locked
        StartCoroutine(TransitionToLevel());
    }

    private IEnumerator TransitionToLevel()
    {
        DisablePlayerMovement(); // Disable player movement

        if (taskUI.fadeImage != null)
        {
            yield return Fade(true); // Fade to black
        }

        yield return LoadScene(levelToLoad);

        if (taskUI.fadeImage != null)
        {
            yield return Fade(false); // Fade from black
        }

        EnablePlayerMovement(5f); // Re-enable player movement
    }

    public void DisablePlayerMovement()
    {
        if (itemInteraction != null)
        {
            itemInteraction.EnableDisableMovement(false);
        }
    }

    public void EnablePlayerMovement(float speed)
    {
        if (itemInteraction != null)
        {
            itemInteraction.EnableDisableMovement(true, speed);
        }
    }

    private IEnumerator Fade(bool toBlack)
    {
        float targetAlpha = toBlack ? 1f : 0f;
        Color color = taskUI.fadeImage.color;

        while (!Mathf.Approximately(color.a, targetAlpha))
        {
            color.a = Mathf.MoveTowards(color.a, targetAlpha, Time.deltaTime);
            taskUI.fadeImage.color = color;
            yield return null;
        }
    }

    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}