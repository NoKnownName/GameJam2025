using System;
using System.Collections;
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
    [SerializeField] private float ability2Duration = 3f;

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
        var hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        if (!hit2D.collider)
        {
            Debug.Log("[InputManager] No valid hit, still awaiting target…");
            return;
        }

        var targetGO = hit2D.rigidbody ? hit2D.rigidbody.gameObject
                                       : hit2D.collider.transform.root.gameObject;

        if (targetGO.name == "PLAYER" || targetGO.GetComponentInParent<PlayerMovement>() != null)
        {
            Debug.Log("[InputManager] Click on PLAYER ignored. Click a platform.");
        }

        bool executed = HandleArmedAbilityOnTarget(armedAbilitySlot, targetGO);
        if (executed) OnAbilityExecuted?.Invoke(armedAbilitySlot);

        if (executed)
        {
            awaitingTarget = false;
            armedAbilitySlot = -1;
        }
    }

    private IEnumerator RotateObject(GameObject target, float duration, Vector3 eulerPerSecond)
    {
        float t = 0f;
        while (t < duration)
        {
            target.transform.Rotate(eulerPerSecond * Time.deltaTime, Space.Self);
            t += Time.deltaTime;
            ParallaxClouds.rotating = true;
            yield return null;
            ParallaxClouds.rotating = false;
        }
    }



    private bool HandleArmedAbilityOnTarget(int slot, GameObject target)
    {
        switch (slot)
        {
            case 1:
                if (target.tag != "Player")
                {
                    var vanish = target.GetComponentInParent<TempVanish>();
                    if (vanish == null) vanish = target.AddComponent<TempVanish>();
                    vanish.Vanish(3f);
                    AudioManager.instance.Play("Disappear");
                }
                return true;             


            case 2:
                var freeze = target.GetComponentInParent<PlatformFreeze>();
                if (freeze == null) freeze = target.AddComponent<PlatformFreeze>();
                freeze.Activate(ability2Duration);
                AudioManager.instance.Play("Freeze");
                return true;

            case 3:
                AudioManager.instance.Play("Spin"); 
                if (target.tag == "Ground") StartCoroutine(RotateObject(target, 2f, new Vector3(0f, 0f, 180f)));
                return true;
        }
        return false;
    }
}
