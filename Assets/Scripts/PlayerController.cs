using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeedMultiplier;
    [SerializeField] private float gravity;
    [SerializeField] private float groundedGravity;
    [SerializeField] private float sensitivity;
    [SerializeField] private float jumpForce;
    [SerializeField] private float parryRange;
    [SerializeField] private Transform cam;
    [SerializeField] private Bullet bullet;

    private float yVelocity;
    private float dt;
    private bool isSprinting;
    private Vector2 camRotation;
    private Transform cachedTransform;
    private CharacterController characterController;
    private InputActions inputActions;

    private void Awake() 
    {
        cachedTransform = transform;
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
        inputActions.OnFoot.Sprint.performed += Sprint_performed;
        inputActions.OnFoot.Sprint.canceled += Sprint_canceled;
    }

    private void OnDisable()
    {
        inputActions.Disable();

        inputActions.OnFoot.Attack.performed -= Attack_performed;
        inputActions.OnFoot.Jump.performed -= Jump_performed;
        inputActions.OnFoot.Sprint.performed -= Sprint_performed;
        inputActions.OnFoot.Sprint.canceled -= Sprint_canceled;
    }

    private void Update() 
    {
        dt = Time.deltaTime;
        Rotate();
        Move();
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
        Vector3 move = GetFootMovement() + GetVerticalMovement();

        characterController.Move(move * dt);
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
        if ((bullet.transform.position - cachedTransform.position).sqrMagnitude <= parryRange * parryRange) bullet.SetDirection(cachedTransform.forward);
    }
    
    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (characterController.isGrounded) yVelocity = jumpForce;
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