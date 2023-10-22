using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovementController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private CharacterController _characterController;

    [Header("Movement")]
    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private bool _isMovementPressed;

    private float _gravity = -9.8f;
    private float _groundedGravity = -0.05f;

    [SerializeField] private float _rotationFactorPerFrame = 15f;

    [Header("Running")]
    private Vector3 _currentRunMovement;
    private bool _isRunPressed;

    [SerializeField] private float _runMultiplier = 3f;

    [Header("Jumping")]
    private bool _isJumpPressed = false;
    private float _initialJumpVelocity;
    [SerializeField] private float _maxJumpHeight = 6f;
    [SerializeField] private float _maxJumpTime = 0.75f;
    private bool _isJumping = false;
    [SerializeField] private float _fallMultiplier = 2f;
    [SerializeField] private float _maxVelocityInFall = -20f;

    private int _jumpCount = 0;
    private Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    private Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();

    private Coroutine _currentJumpResetRoutine = null;


    [Header("Animation")]
    private Animator _animator;

    private static readonly int s_isWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int s_isRunningHash = Animator.StringToHash("isRunning");
    private static readonly int s_isJumpingHash = Animator.StringToHash("isJumping");
    private static readonly int s_jumpCountHash = Animator.StringToHash("jumpCount");

    private bool _isJumpAnimating = false;

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Player.Disable();
    }

    private void Start()
    {
        CalculateAndInitializeJumpVariables();

        _playerInput.Player.Move.started += OnMovementInput;
        _playerInput.Player.Move.performed += OnMovementInput;
        _playerInput.Player.Move.canceled += OnMovementInput;

        _playerInput.Player.Run.started += OnRunInput;
        _playerInput.Player.Run.canceled += OnRunInput;

        _playerInput.Player.Jump.started += OnJumpInput;
        _playerInput.Player.Jump.canceled += OnJumpInput;
    }

    private void CalculateAndInitializeJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;

        _initialJumpVelocity = CalculateJumpVelocity(timeToApex, _maxJumpHeight);
        float secondJumpVelocity = CalculateJumpVelocity(timeToApex * 1.25f, _maxJumpHeight + 2);
        float thirdJumpVelocity = CalculateJumpVelocity(timeToApex * 1.5f, _maxJumpHeight + 4);

        _initialJumpVelocities.Add(1, _initialJumpVelocity);
        _initialJumpVelocities.Add(2, secondJumpVelocity);
        _initialJumpVelocities.Add(3, thirdJumpVelocity);

        _gravity = CalculateJumpGravity(timeToApex, _maxJumpHeight);
        float secondJumpGravity = CalculateJumpGravity(timeToApex * 1.25f, _maxJumpHeight + 2);
        float thirdJumpGravity = CalculateJumpGravity(timeToApex * 1.5f, _maxJumpHeight + 4);

        _jumpGravities.Add(0, _gravity);
        _jumpGravities.Add(1, _gravity);
        _jumpGravities.Add(2, secondJumpGravity);
        _jumpGravities.Add(3, thirdJumpGravity);
    }

    float CalculateJumpGravity(float timeToApex, float maxJumpHeight)
    {
        return (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
    }

    float CalculateJumpVelocity(float timeToApex, float maxJumpHeight)
    {
        return (2 * maxJumpHeight) / timeToApex;
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();

        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;

        _currentRunMovement.x = _currentMovementInput.x * _runMultiplier;
        _currentRunMovement.z = _currentMovementInput.y * _runMultiplier;

        _isMovementPressed = (_currentMovementInput.x != 0) || (_currentMovementInput.y != 0);
    }

    private void OnRunInput(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
    }

    private void Update()
    {
        HandleRotation();
        HandleAnimation();

        HandleMovement();
        HandleGravity();
        HandleJump();
    }

    private void HandleAnimation()
    {
        bool isWalking = _animator.GetBool(s_isWalkingHash);
        bool isRunning = _animator.GetBool(s_isRunningHash);

        if (_isMovementPressed && !isWalking)
        {
            _animator.SetBool(s_isWalkingHash, true);
        }
        else if (!_isMovementPressed && isWalking)
        {
            _animator.SetBool(s_isWalkingHash, false);
        }

        if ((_isMovementPressed && _isRunPressed) && !isRunning)
        {
            _animator.SetBool(s_isRunningHash, true);
        }
        else if ((!_isMovementPressed || !_isRunPressed) && isRunning)
        {
            _animator.SetBool(s_isRunningHash, false);
        }
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt = new Vector3(_currentMovement.x, 0f, _currentMovement.z);

        Quaternion currentRotation = transform.rotation;
        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }
    }

    private void HandleMovement()
    {
        if (_isRunPressed)
        {
            _characterController.Move(_currentRunMovement * Time.deltaTime);
        }
        else
        {
            _characterController.Move(_currentMovement * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (!_characterController.isGrounded)
        {
            return;
        }

        if (!_isJumping && _isJumpPressed)
        {
            if (_jumpCount < 3 && _currentJumpResetRoutine != null)
            {
                StopCoroutine(_currentJumpResetRoutine);
            }

            _animator.SetBool(s_isJumpingHash, true);
            _isJumpAnimating = true;

            _isJumping = true;
            _jumpCount++;
            _animator.SetInteger(s_jumpCountHash, _jumpCount);

            _currentMovement.y = _initialJumpVelocities[_jumpCount] * 0.5f;
            _currentRunMovement.y = _initialJumpVelocities[_jumpCount] * 0.5f;
        }
        else if (_isJumping && !_isJumpPressed)
        {
            _isJumping = false;
        }
    }

    IEnumerator JumpResetRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        _jumpCount = 0;
    }

    private void HandleGravity()
    {
        bool isFalling = (_currentMovement.y <= 0f) || !_isJumpPressed;

        if (_characterController.isGrounded)
        {
            if (_isJumpAnimating)
            {
                _animator.SetBool(s_isJumpingHash, false);
                _isJumpAnimating = false;

                _currentJumpResetRoutine = StartCoroutine(JumpResetRoutine());
                if (_jumpCount == 3)
                {
                    _jumpCount = 0;
                    _animator.SetInteger(s_jumpCountHash, _jumpCount);
                }
            }

            _currentMovement.y = _groundedGravity;
            _currentRunMovement.y = _groundedGravity;
        }
        else
        {
            float previousYVelocity = _currentMovement.y;

            float acceleration = _jumpGravities[_jumpCount] * Time.deltaTime;
            if (isFalling)
            {
                acceleration *= _fallMultiplier;
            }

            float newYVelocity = _currentMovement.y + acceleration;

            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            if (isFalling)
            {
                nextYVelocity = Mathf.Max(nextYVelocity, _maxVelocityInFall);
            }

            _currentMovement.y = nextYVelocity;
            _currentRunMovement.y = nextYVelocity;
        }
    }
}
