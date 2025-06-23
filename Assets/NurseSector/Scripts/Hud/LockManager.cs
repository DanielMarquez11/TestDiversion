using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LockManager : MonoBehaviour
{
    [Header("Lock Settings")]
    private string correctPassword = "1234"; // Correct password
    [SerializeField] private TextMeshProUGUI displayText;      // Display text for entered digits
    [SerializeField] private int maxPasswordLength = 4;       // Maximum length of the password

    private Image displayImage;
    private string enteredPassword = "";                      // Store the player's entered password


    private Color32 customGreen = new Color32(213, 246, 196, 255); // 
    private Color32 customRed = new Color32(251, 191, 194, 255);

    private interactDoor targetDoor;
    private ItemInteraction itemInteraction;

    void Start()
    {
        // Dynamically get the Image component from the DisplayPanel (parent or sibling)
        displayImage = displayText.GetComponentInParent<Image>();
        if (displayImage == null)
        {
            Debug.LogError("Display image not found! Make sure there is an Image component in the parent of the displayText.");
        }

        // Hide the lock UI at the start
        transform.GetChild(0).gameObject.SetActive(false);
        itemInteraction = FindFirstObjectByType<ItemInteraction>();
    }

    /// <summary>
    /// Adds a digit to the entered password when a button is clicked.
    /// </summary>
    public void AddDigit(string digit)
    {
        if (enteredPassword.Length < maxPasswordLength)
        {
            enteredPassword += digit;
            UpdateDisplay();

            // Only check the password after the last (4th) digit is entered
            if (enteredPassword.Length == maxPasswordLength)
            {
                if (enteredPassword == correctPassword)
                {
                    displayImage.color = customGreen; // Change to green for correct password
                    Debug.Log("Correct Password! Unlocking...");
                    Unlock();
                }
                else
                {
                    displayImage.color = customRed; // Change to red for incorrect password
                    Debug.Log("Incorrect Password. Try Again.");
                }
            }
        }
    }

    /// <summary>
    /// Clears the entered password.
    /// </summary>
    public void ClearPassword()
    {
        enteredPassword = "";
        displayImage.color = Color.white;
        UpdateDisplay();
    }

   

    /// <summary>
    /// Updates the display text with the entered password.
    /// </summary>
    private void UpdateDisplay()
    {
        displayText.text = enteredPassword.Length > 0 ? enteredPassword : "Enter Code";
    }

    /// <summary>
    /// Unlocks the mechanism (replace with your unlock logic).
    /// </summary>
    private void Unlock()
    {
        Debug.Log("Unlocked successfully!");
        transform.GetChild(0).gameObject.SetActive(false); // Hide the lock UI
        if(itemInteraction != null)
        {
            itemInteraction.EnableDisableMovement(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        // Implement your unlock functionality here (e.g., open a door, trigger an event, etc.)

        targetDoor.openDoor();
    }

    public void SetLockCode(int code, interactDoor door)
    {
        correctPassword = code.ToString("0000");
        targetDoor = door;
        ClearPassword();

        transform.GetChild(0).gameObject.SetActive(true);
        if(itemInteraction != null)
        {
            itemInteraction.EnableDisableMovement(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}