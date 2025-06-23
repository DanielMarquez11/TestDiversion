using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Patient : MonoBehaviour
{
    public int PatientNumber;
    public bool isDead = false;

    private GameObject _player;
    private GameObject canvasBar;
    private TMP_Text patientNameTxt;
    private Slider healthSlider;
    private Image fillImage;
    private Image handleImage;

    private float healthBefore;
    public Sprite[] healthBarStates = new Sprite[4];

    public string patientName;
    public string patientCaseText;

    [Header("Modular Clothing Assignment")]
    public GameObject[] modularPants;

    public GameObject[] modularShirts;
    public GameObject[] modularShoes;
    public GameObject[] modularOutfits;

    private float _health = 100;
    private int taskAmount;
    private ItemInteraction _itemInteraction;
    private float healAmount;
    private float incrementHealthChange;

    public float health
    {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, 100);
            CheckHealth();
        }
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");

        if (_player) _itemInteraction = _player.GetComponent<ItemInteraction>();

        canvasBar = GetComponentInChildren<Canvas>()?.gameObject;
        if (canvasBar)
        {
            patientNameTxt = canvasBar.GetComponentInChildren<TMP_Text>();
            healthSlider = canvasBar.GetComponentInChildren<Slider>();

            if (healthSlider)
            {
                fillImage = healthSlider.fillRect?.GetComponent<Image>();
                handleImage = healthSlider.handleRect?.GetComponent<Image>();
            }

            patientName = string.IsNullOrEmpty(patientName) ? $"Patient {PatientNumber}" : patientName;
            if (patientNameTxt) patientNameTxt.text = patientName;
        }

        CheckHealth();
    }

    private void Update()
    {
        UIFollowPlayer();
    }

    public void UpdatePatientData()
    {
        if(gameObject.GetComponent<taskHost>() != null)
        {
            PatientNumber = gameObject.GetComponent<taskHost>().taskHostNumber;
            patientName = gameObject.GetComponent<taskHost>().taskHostName;
            patientCaseText = gameObject.GetComponent<taskHost>().taskHostCaseText;

            patientName = string.IsNullOrEmpty(patientName) ? $"Patient {PatientNumber}" : patientName;
            if (patientNameTxt) patientNameTxt.text = patientName;
        }
    }

    private void CheckHealth()
    {
        if (isDead || canvasBar == null || healthSlider == null) return;

        healthSlider.value = _health;

        if (_health <= 0) return;
        

        int spriteIndex;
        Color newColor;

        if (_health > 75)
        {
            newColor = Color.green;
            spriteIndex = 0;
        }
        else if (_health > 50)
        {
            newColor = Color.yellow;
            spriteIndex = 1;
        }
        else if (_health > 25)
        {
            newColor = Color.red;
            spriteIndex = 2;
        }
        else
        {
            newColor = Color.red;
            spriteIndex = 3;
        }

        if (fillImage && fillImage.color != newColor) fillImage.color = newColor;
        if (handleImage && handleImage.sprite != healthBarStates[spriteIndex])
            handleImage.sprite = healthBarStates[spriteIndex];
    }

    public void UIFollowPlayer()
    {
        if (_player && canvasBar)
        {
            canvasBar.transform.rotation = Quaternion.Euler(0, _player.transform.rotation.eulerAngles.y, 0);
        }
    }

    public void BeginUrgentTask()
    {
        if (_itemInteraction?.taskManager?.activeUrgentTask == null) return;

        taskAmount = _itemInteraction.taskManager.activeUrgentTask.steps.Count;
        incrementHealthChange = 100 / taskAmount;
        healthBefore = _health;
        StartCoroutine(SmoothHealthChange(0));
        
    }

    public void CompleteUrgentTaskStep()
    {
        float activeTasks = Mathf.Max(taskAmount, 1);
        float stepHealAmount = healthBefore / activeTasks;

        StartCoroutine(SmoothHealthChange(Mathf.Min(_health + stepHealAmount, healthBefore)));
    }

    private IEnumerator SmoothHealthChange(float newHealth, float duration = 1f)
    {
        float elapsedTime = 0f;
        float startingHealth = _health;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpedHealth = Mathf.Lerp(startingHealth, newHealth, elapsedTime / duration);
            
            health = Mathf.Round(lerpedHealth * 100f) / 100f;
        
            if (healthSlider) healthSlider.value = health;
            yield return null;
        }
        health = newHealth;
        if (healthSlider) healthSlider.value = health;
    }
}