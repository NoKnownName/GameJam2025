using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager I;
    public static PlayerInput PlayerInput;

    public static Vector2 movement;
    public static bool jumpWasPressed;
    public static bool jumpIsHeld;
    public static bool jumpWasReleased;
    public static bool runIsHeld;

    private Camera myCamera;
    private float cooldown;
    private GameObject clickedObject;

    private bool awaitingTarget;
    private int armedAbilitySlot = -1;

    public bool Ability1Active = true;
    public bool Ability2Active = true;
    public bool Ability3Active = true;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;

    public static event Action<int> OnAbilityExecuted;

    private void Awake()
    {
        I = this;

        PlayerInput = GetComponent<PlayerInput>();
        moveAction = PlayerInput.actions["Move"];
        jumpAction = PlayerInput.actions["Jump"];
        runAction = PlayerInput.actions["Run"];

        myCamera = Camera.main;
    }

    private void Update()
    {
        movement = moveAction.ReadValue<Vector2>();
        jumpWasPressed = jumpAction.WasPressedThisFrame();
        jumpIsHeld = jumpAction.IsPressed();
        jumpWasReleased = jumpAction.WasReleasedThisFrame();
        runIsHeld = runAction.IsPressed();

        if (clickedObject != null)
        {
            if (cooldown > 0f)
            {
                cooldown -= Time.deltaTime;
                clickedObject.SetActive(false);
            }
            else
            {
                clickedObject.SetActive(true);
                clickedObject = null;
                cooldown = 0f;
            }
        }
    }

    public void ArmAbility(int abilitySlot)
    {
        awaitingTarget = true;
        armedAbilitySlot = abilitySlot;
        //Cursor-Feedback
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (!awaitingTarget) return;

        var ray = myCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hit2D = Physics2D.GetRayIntersection(ray);

        if (!hit2D.collider)
        {
            awaitingTarget = false;
            armedAbilitySlot = -1;
            return;
        }

        bool executed = HandleArmedAbilityOnTarget(armedAbilitySlot, hit2D.collider.gameObject);

        if (executed)
            OnAbilityExecuted?.Invoke(armedAbilitySlot);

        awaitingTarget = false;
        armedAbilitySlot = -1;
    }

    private bool HandleArmedAbilityOnTarget(int slot, GameObject target)
    {
        switch (slot)
        {
            case 1:
                clickedObject = target;
                cooldown = 3f;
                return true;

            case 2:
                // TODO: Ability 2 Logik auf 'target'
                return false;

            case 3:
                // TODO: Ability 3 Logik auf 'target'
                return false;

            default:
                return false;
        }
    }
}
