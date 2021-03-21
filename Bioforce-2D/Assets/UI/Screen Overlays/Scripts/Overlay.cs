using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class Overlay : MonoBehaviour
{
    [SerializeField] private Sprite[] OverlaySprites;
    [SerializeField] protected Gradient OverlayColorGradient;
    [SerializeField] protected float FadeSpeed = 3f;

    [SerializeField] protected PlayerManager PlayerManager; //Holds the event to subscribe to, to activate

    [SerializeField] protected Image Image;
    [SerializeField] private float TimeBeforeClearing = 0.2f;

    protected Coroutine FadingCoroutine { get; set; }

    protected virtual void Start()
    {
        Image.enabled = false;
        SubscribeToActivationEvent();
    }

    protected abstract void SubscribeToActivationEvent();
    
    protected void RandomiseImage()
    {
        Image.sprite = OverlaySprites[UnityEngine.Random.Range(0, OverlaySprites.Length)];
        Image.rectTransform.rotation = Quaternion.Euler(0, RandomRotation(), RandomRotation());

        Image.color = OverlayColorGradient.Evaluate(UnityEngine.Random.Range(0f, 1f));
    }
    
    protected virtual void Activate()
    {
        if (!(FadingCoroutine is null))
            StopCoroutine(FadingCoroutine);
        
        EnableOverlay();
    }

    protected virtual void EnableOverlay()
    {
        RandomiseImage();
        Image.enabled = true;

        FadingCoroutine = StartCoroutine(FadeOverlayImage());
    }

    protected virtual IEnumerator FadeOverlayImage()
    {
        yield return new WaitForSeconds(TimeBeforeClearing);
        
        while (true)
        {
            Image.color = Color.Lerp(Image.color, Color.clear, FadeSpeed * Time.fixedDeltaTime);
            
            float thresholdAlpha = 0.03f;
            if (Image.color.a < thresholdAlpha)
                break;

            yield return new WaitForFixedUpdate();
        }

        Image.enabled = false;
    }
    private float RandomRotation() =>
        PhysicsHelper.RandomBool() ? 0f : 180f;
}
