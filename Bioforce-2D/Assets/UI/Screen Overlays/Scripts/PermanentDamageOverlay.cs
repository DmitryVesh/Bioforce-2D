using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PermanentDamageOverlay : HitOverlay //Only need to inherit the overlay data
{
    private float MaxHealth { get; set; } //Must be float so don't have to cast MaxHealth or currentHealth, to find fraction
    [SerializeField] private float Threshold = 0.7f;
    private float ColorGradientTime;
    [SerializeField] private float ColorThresholdValues = 0.1f;
    private float Alpha { get; set; }

    protected override void ActivateOverlay(int currentHealth)
    {
        float healthFraction = currentHealth / MaxHealth;
        if (healthFraction > Threshold)
        {
            DeactivateOverlay();
            return;
        }

        if (!CurrentlyOn())
            EnableOverlay();

        Alpha = (1f - healthFraction) + 0.1f;
        Alpha = Alpha > 1f ? 1f : Alpha;

        Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, Alpha);
    }
    protected override IEnumerator FadeOverlayImage()
    {
        while (true)
        {
            Color gradientColor = OverlayColorGradient.Evaluate(ColorGradientTime);
            Color targetColor = new Color(gradientColor.r, gradientColor.g, gradientColor.b, Alpha);

            while (!ColorsClose(Image.color, targetColor))
            {
                Image.color = Color.Lerp(Image.color, targetColor, FadeSpeed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            ColorGradientTime = (ColorGradientTime + 1) % 2;
        }
    }

    private bool ColorsClose(Color color, Color targetColor) =>
        ColorValuesBelowThreshold(color.r, targetColor.r) &&
        ColorValuesBelowThreshold(color.g, targetColor.g) &&
        ColorValuesBelowThreshold(color.b, targetColor.b);
        

    private bool ColorValuesBelowThreshold(float colorValue, float targetColorValue)
    {
        float dif = colorValue - targetColorValue;        
        dif = dif < 0 ? dif * -1 : dif;

        return dif <= ColorThresholdValues;
    }

    protected override void Start()
    {
        base.Start();
        MaxHealth = PlayerManager.MaxHealth;
    }

    

    protected override void SubscribeToActivationEvent()
    {
        base.SubscribeToActivationEvent();
        PlayerManager.OnPlayerRespawn += DeactivateOverlay;
        PlayerManager.OnPlayerRestoreHealth += ActivateOverlay;
    }

    private void OnDestroy()
    {
        PlayerManager.OnPlayerRespawn -= DeactivateOverlay;
        PlayerManager.OnPlayerRestoreHealth -= ActivateOverlay;
    }

    private bool CurrentlyOn() =>
        Image.enabled;

    
}
