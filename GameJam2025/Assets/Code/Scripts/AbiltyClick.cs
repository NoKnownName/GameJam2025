using UnityEngine;
using UnityEngine.UI;

public class AbilityClick : MonoBehaviour
{
    [SerializeField] private Image cooldownImage;
    [SerializeField] private float cooldown;

    private float remaining;

    public void OnClick()
    {
        if (remaining > 0f) return;
        remaining = cooldown;
        cooldownImage.fillAmount = 1f;
    }

    private void Update()
    {
        if (remaining <= 0f) return;

        remaining -= Time.deltaTime;
        if (remaining < 0f) remaining = 0f;

        cooldownImage.fillAmount = remaining / cooldown;
    }
}
