using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeedMultiplier;
    [SerializeField] private float gravity;
    [SerializeField] private float groundedGravity;
    [SerializeField] private float sensitivity;
    [SerializeField] private float jumpForce;
    [SerializeField] private float parryRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashSpeed;    
    [SerializeField] private float dashDuration; 
    [SerializeField] private float swordAnimSpeed;
    [SerializeField] private Vector3 swordTargetPos;
    [SerializeField] private Quaternion swordTargetRot;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform sword;
    [SerializeField] private Bullet bullet;

    private float yVelocity;
    private float dt;
    private float lastAttackTime;
    private float lastDashTime;
    private float dashEndTime;
    private bool isSprinting;
    private bool isSwinging;
    private bool isDashing;
    private Vector2 camRotation;
    private Vector3 swordInitialPos;
    private Vector3 dashDirection;
    private Quaternion swordInitialRot;
    private Transform cachedTransform;
    private CharacterController characterController;
    private InputActions inputActions;

    private void Awake() 
    {
        cachedTransform = transform;
        swordInitialPos = sword.localPosition;
        swordInitialRot = sword.localRotation;
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputActions = new InputActions();
    }
    
    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.OnFoot.Attack.performed += Attack_performed;
        inputActions.OnFoot.Jump.performed += Jump_performed;
        inputActions.OnFoot.Dash.performed += Dash_performed;
        inputActions.OnFoot.Sprint.performed += Sprint_performed;
        inputActions.OnFoot.Sprint.canceled += Sprint_canceled;
    }

    private void OnDisable()
    {
        inputActions.Disable();

        inputActions.OnFoot.Attack.performed -= Attack_performed;
        inputActions.OnFoot.Jump.performed -= Jump_performed;
        inputActions.OnFoot.Dash.performed -= Dash_performed;
        inputActions.OnFoot.Sprint.performed -= Sprint_performed;
        inputActions.OnFoot.Sprint.canceled -= Sprint_canceled;
    }

    private void Update() 
    {
        dt = Time.deltaTime;
        Rotate();
        Move();
        SwordAnimation();
    }

    private void Rotate()
    {
        Vector2 input = dt * sensitivity * inputActions.Camera.Look.ReadValue<Vector2>();
        camRotation.x += input.x;
        camRotation.y = Mathf.Clamp(camRotation.y - input.y, -90f, 90f);

        cachedTransform.rotation = Quaternion.Euler(0f, camRotation.x, 0f);
        cam.localRotation = Quaternion.Euler(camRotation.y, 0f, 0f);
    }

    private void Move()
    {
        if (isDashing)
        {
            if (Time.time >= dashEndTime) isDashing = false;
            else
            {
                characterController.Move(dashDirection * dashSpeed * dt);
                return;
            }
        }
        
        Vector3 move = GetFootMovement() + GetVerticalMovement();

        characterController.Move(move * dt);
    }

    private void SwordAnimation()
    {
        Vector3 targetPos = isSwinging ? swordTargetPos : swordInitialPos;
        Quaternion targetRot = isSwinging ? swordTargetRot : swordInitialRot;

        float t = 1f - Mathf.Exp(-swordAnimSpeed * Time.deltaTime);
        sword.SetLocalPositionAndRotation(Vector3.Lerp(sword.localPosition, targetPos, t), Quaternion.Slerp(sword.localRotation, targetRot, t));

        if (isSwinging && (sword.localPosition - swordTargetPos).sqrMagnitude < 0.0001f) isSwinging = false;
    }

    private Vector3 GetFootMovement()
    {
        Vector2 input = Vector2.ClampMagnitude(inputActions.OnFoot.Move.ReadValue<Vector2>(), 1.0f) * moveSpeed;
        
        if (isSprinting) input *= sprintSpeedMultiplier;

        return cachedTransform.right * input.x + cachedTransform.forward * input.y;
    }

    private Vector3 GetVerticalMovement()
    {
        if (characterController.isGrounded && yVelocity < 0f) yVelocity = -groundedGravity;
        else yVelocity -= gravity * dt;

        return Vector3.up * yVelocity;
    }

    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        float time = Time.time;
        if (time < lastAttackTime + attackCooldown) return;
        lastAttackTime = time;
        isSwinging = true;

        if (bullet == null) return;

        Vector3 toBullet = bullet.transform.position - cachedTransform.position;
        bool inRange = toBullet.sqrMagnitude <= parryRange * parryRange;
        bool inFront = Vector3.Dot(cachedTransform.forward, toBullet.normalized) > 0.5f;

        if (inRange && inFront)
            bullet.SetDirection(cachedTransform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Bullet _)) return;

        SceneManager.LoadScene(1);
    }
    
    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (characterController.isGrounded) yVelocity = jumpForce;
    }
    
    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (lastDashTime + dashCooldown > Time.time) return;
        lastDashTime = Time.time;

        Vector2 input = inputActions.OnFoot.Move.ReadValue<Vector2>();
        Vector3 dir = cachedTransform.right * input.x + cachedTransform.forward * input.y;

        dashDirection = dir.sqrMagnitude > 0.01f ? dir.normalized : cachedTransform.forward;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
    }

    private void Sprint_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSprinting = true;
    }
    private void Sprint_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSprinting = false;
    }
}