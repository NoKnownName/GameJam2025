using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AbilityClick : MonoBehaviour
{
    [SerializeField] private Image cooldownImage;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private int abilitySlot = 1;

    private float remaining;


    private void OnEnable()  { InputManager.OnAbilityExecuted += HandleAbilityExecuted; }
    private void OnDisable() { InputManager.OnAbilityExecuted -= HandleAbilityExecuted; }

    public void OnClick()
    {
        if (remaining > 0f)
        {
            Debug.Log($"[AbilityClick] Cooldown {remaining:F2}s – ignoriere.");
            return;
        }

        if (InputManager.I == null)
        {
            Debug.LogError("[AbilityClick] Kein InputManager in Szene.");
            return;
        }

        Debug.Log($"[AbilityClick] ARM ability {abilitySlot}");
        InputManager.I.ArmAbility(abilitySlot);
    }

    private void HandleAbilityExecuted(int slot)
    {
        if (slot != abilitySlot) return;
        remaining = cooldown;
        if (cooldownImage) cooldownImage.fillAmount = 1f;
        Debug.Log($"[AbilityClick] Ability {slot} EXECUTED → Cooldown {cooldown}s");
    }

    private void Update()
    {
        if (remaining <= 0f) return;
        remaining -= Time.deltaTime;
        if (remaining < 0f) remaining = 0f;
        if (cooldownImage) cooldownImage.fillAmount = remaining / Mathf.Max(0.0001f, cooldown);
    }
}
