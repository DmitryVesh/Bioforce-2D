using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNum : MonoBehaviour
{

    [SerializeField] private float FadeSpeed = 30; //Rate at which the number will fade away
    [SerializeField] private float InitialIncreaseScale = 0.01f; //Used to increase the numbers when less than half time to live is left
    [SerializeField] private float AfterIncreaseScale = 0.04f; //Used to increase the numbers when more than half time to live is left
                                                               //After, is bigger than Initial, in order to 'blow' up the numbers when fading away
    [SerializeField] private float TimeToLive = 1;
    private float CurrentTimeToLive { get; set; }

    private bool Available { get; set; } = false;

    private TextMeshProUGUI NumberText { get; set; }
    private bool CanFade { get; set; } = false;

    private Vector3 MoveVector { get; set; } //Used to make the damage nums go to the left or to the right
    private float MoveVectorDamping { get; set; } //Used to decrease the moveVector, so numbers decrease in speed.

    public bool IsAvailable() =>
        Available;
    public void Activate(Vector3 position, int damage, bool isGoingRight)
    {
        gameObject.SetActive(true);
        gameObject.transform.position = position;
        CurrentTimeToLive = TimeToLive;
        NumberText.SetText(damage.ToString());

        SetNumberTextAlpha(255);

        Available = false;
        float randomXValue = Random.Range(0.1f, 0.5f);
        float randomYValue = Random.Range(0.1f, 0.5f);
        if (isGoingRight)
            MoveVector = new Vector3(randomXValue, randomYValue) * 10;
        else
            MoveVector = new Vector3(-randomXValue, randomYValue) * 10;
        CanFade = false;
    }

    private void SetNumberTextAlpha(byte alpha)
    {
        Color32 originalColor = NumberText.color;
        originalColor.a = alpha;
        NumberText.color = originalColor;
    }

    private void Awake()
    {
        NumberText = GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }
    private void FixedUpdate()
    {
        if (CanFade)
        {
            FadeAway();
            MoveUp();
            ChangeSize();
        }
        else if (!Available)
        {
            CurrentTimeToLive -= Time.fixedDeltaTime;
            MoveUp();
            ChangeSize();
            if (CurrentTimeToLive < 0)
                CanFade = true;
        }
        
    }

    private void ChangeSize()
    {
        if (CurrentTimeToLive > TimeToLive / 2)
            transform.localScale += Vector3.one * InitialIncreaseScale * Time.fixedDeltaTime;
        else
            transform.localScale += Vector3.one * AfterIncreaseScale * Time.fixedDeltaTime;
    }
    private void MoveUp()
    {
        transform.position += MoveVector * Time.fixedDeltaTime;
        MoveVector -= MoveVector * MoveVectorDamping * Time.fixedDeltaTime;
    }

    private void FadeAway()
    {
        Color32 currentColor = NumberText.color;
        byte setAlphaTo = (byte)(currentColor.a - (FadeSpeed * Time.fixedDeltaTime));
        SetNumberTextAlpha(setAlphaTo);
        if (currentColor.a <= 50)
        {
            CanFade = false;
            Reset();
        }
    }
    private void Reset()
    {
        transform.localScale = Vector3.one;
        Available = true;
        gameObject.SetActive(false);
    }
}
