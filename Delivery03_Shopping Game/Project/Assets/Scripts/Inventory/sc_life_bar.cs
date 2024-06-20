using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class LifeBar : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject damageObject;
    [SerializeField] private RawImage damageImage;
    [SerializeField] private AudioClip damageSound;

    private Slider slider;
    private float transitionDuration = 0.75f;
    private float transitionDurationOffset = 2f; // Offset to wait for the transition to finish 0.75*2 = 1.5s + 0.5s offset
    private bool canClick = true;

    private void OnEnable()
    {
        PlayerInventory.OnItemConsumedEvent += RestoreLife;
    }

    void Start()
    {
        slider = GetComponent<Slider>();
        damageImage.color = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0f);
        damageObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canClick && eventData.pointerPress == gameObject)
        {
            StartCoroutine(HandleClick());
        }
    }

    IEnumerator HandleClick()
    {
        canClick = false; // Disable click temporarily to avoid spamming
        SoundManager.Instance.PlaySound(damageSound);
        StartCoroutine(InterpolateSliderValue(0f));
        StartCoroutine(FadeDamageImage());
        yield return new WaitForSeconds(transitionDurationOffset); // Wait for transition to finish
        canClick = true;
    }

    public void RestoreLife(float healthRestore)
    {
        if (slider != null)
        {
            StartCoroutine(InterpolateSliderValue(healthRestore));
        }
    }

    IEnumerator InterpolateSliderValue(float targetValue)
    {
        float elapsedTime = 0f;
        float startValue = slider.value;

        while (elapsedTime < transitionDuration)
        {
            slider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        slider.value = targetValue;
    }

    IEnumerator FadeDamageImage()
    {
        damageObject.SetActive(true);
        float elapsedTime = 0f;
        Color startColor = damageImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsedTime < transitionDuration)
        {
            damageImage.color = Color.Lerp(startColor, endColor, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        damageImage.color = endColor; 

        yield return new WaitForSeconds(0.5f);

        elapsedTime = 0f;
        startColor = damageImage.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < transitionDuration)
        {
            damageImage.color = Color.Lerp(startColor, endColor, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        damageImage.color = endColor; 
        damageObject.SetActive(false);
    }
}
