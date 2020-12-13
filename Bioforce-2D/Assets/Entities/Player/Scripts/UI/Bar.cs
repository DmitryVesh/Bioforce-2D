using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    private Slider Slider { get; set; }
    private Image Fill { get; set; }
    [SerializeField] Gradient Gradient = null; // Set in editor

    private void Awake() 
    {
        Slider = GetComponent<Slider>();
        Fill = Slider.fillRect.GetComponent<Image>();
    }

    public void SetCurrentBarValue(float current)
    {
        Slider.value = current;
        Fill.color = Gradient.Evaluate(Slider.normalizedValue);
    }

    public void SetMaxBarValue(float min, float max)
    {
        Slider.minValue = min;
        Slider.maxValue = max;
    }
}
