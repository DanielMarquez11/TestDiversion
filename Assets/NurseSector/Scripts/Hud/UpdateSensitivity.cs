using System;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSensitivity : MonoBehaviour
{
    [SerializeField] private Slider MouseSlider;
    [SerializeField] private ItemInteraction _itemInteraction;

    private void Start()
    {
        if (_itemInteraction == null)
        {
            _itemInteraction = GameObject.FindWithTag("Player").GetComponent<ItemInteraction>();
        }

        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity",3.0f);
        MouseSlider.value = savedSensitivity;
        _itemInteraction.MouseSensitivity = savedSensitivity;
        
        MouseSlider.onValueChanged.AddListener(UpdateLookSpeed);
    }

    public void UpdateLookSpeed(float value)
    {
        _itemInteraction.MouseSensitivity = value;
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}