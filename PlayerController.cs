using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _controller;
    private Animator _animator;

    [SerializeField]
    private float _playerSpeed = 5f;

    [SerializeField]
    private float _sprintSpeed = 10f;

    [SerializeField]
    private float _rotationSpeed = 10f;

    [SerializeField]
    private Camera _followCamera;

    private Vector3 _velocity;
    private bool _isGrounded;

    [SerializeField]
    private float _jumpHeight = 1.0f;
    [SerializeField]
    private float _gravityValue = -9.81f;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        if (_controller == null)
        {
            Debug.LogError("CharacterController component is missing!");
        }

        if (_animator == null)
        {
            Debug.LogError("Animator component is missing!");
        }
    }

    private void Update()
    {
        Movement();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        _controller.Move(_velocity * Time.fixedDeltaTime);
    }

    void Movement()
    {
        _isGrounded = _controller.isGrounded || GetGroundDistance() < 0.2f;

        HandleGroundedVelocity();

        Vector3 movementDirection = GetMovementDirection();
        float currentSpeed = GetCurrentSpeed(movementDirection);

        _controller.Move(movementDirection * currentSpeed * Time.deltaTime);
        RotateTowardsMovementDirection(movementDirection);

        HandleJumping();
        UpdateAnimation(movementDirection, currentSpeed);
    }

    void HandleGroundedVelocity()
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            _animator?.SetBool("IsFalling", false);
            _animator?.SetBool("IsJumping", false);
        }
    }

    Vector3 GetMovementDirection()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movementInput = new Vector3(horizontalInput, 0, verticalInput);

        if (_followCamera != null)
        {
            Vector3 cameraForward = _followCamera.transform.forward;
            Vector3 cameraRight = _followCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            return (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;
        }

        return movementInput.normalized;
    }

    float GetCurrentSpeed(Vector3 movementDirection)
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && movementDirection.magnitude > 0;
        float speedRatio = Mathf.Clamp(movementDirection.magnitude, 0f, 1f);
        return isSprinting ? _sprintSpeed : _playerSpeed * speedRatio;
    }

    void RotateTowardsMovementDirection(Vector3 movementDirection)
    {
        if (movementDirection != Vector3.zero)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(2 * _jumpHeight * -_gravityValue);
            _animator?.SetBool("IsJumping", true);
        }
    }

    void ApplyGravity()
    {
        if (!_isGrounded)
        {
            _velocity.y += _gravityValue * Time.fixedDeltaTime;
        }
    }

    void UpdateAnimation(Vector3 movementDirection, float currentSpeed)
    {
        if (_animator == null) return;

        float normalizedSpeed = Mathf.Clamp(currentSpeed / _sprintSpeed, 0f, 1f);
        _animator.SetFloat("Speed", normalizedSpeed);

        _animator.SetFloat("VerticalSpeed", _velocity.y);

        if (!_isGrounded && _velocity.y < -0.1f && GetGroundDistance() > 0.5f)
        {
            _animator.SetBool("IsFalling", true);
        }
    }

    float GetGroundDistance()
    {
        float maxDistance = 2.0f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxDistance))
        {
            return hit.distance;
        }
        return Mathf.Infinity;
    }
}
