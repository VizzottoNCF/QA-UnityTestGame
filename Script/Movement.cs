using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{

    public static PlayerInput PlayerInput;

    public static bool gameHasStarted = false;

    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    public bool isFacingRight = true;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;

    private Animator _anim;

    [Header("Dash Vars")]
    public float dashForce;
    public float startDashTimer;
    public float dashCooldown;
    private float currentDashTimer;
    private float dashCooldownTimer;
    private float dashDirection;
    private bool isDashing = false;

    public static bool jumpWasPressed;
    public static bool jumpWasReleased;
    public static bool dashWasPressed;
    public static bool dashWasReleased;

    private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _dashAction = PlayerInput.actions["Dash"];
        _anim = GetComponent<Animator>();
    }


    void Update()
    {
        if (!gameHasStarted) { return; }

        // animation vars
        _anim.SetBool("Jumping", !IsGrounded());
        _anim.SetFloat("Walking", _moveAction.ReadValue<Vector2>().x);

        // actions vars
        jumpWasPressed = _jumpAction.WasPressedThisFrame();
        jumpWasReleased = _jumpAction.WasReleasedThisFrame();
        dashWasPressed = _dashAction.WasPressedThisFrame();
        dashWasReleased = _dashAction.WasReleasedThisFrame();

        horizontal = _moveAction.ReadValue<Vector2>().x;

        if (jumpWasPressed && IsGrounded() && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }

        if (jumpWasReleased && rb.linearVelocity.y > 0f && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        if (dashWasPressed && !isDashing && horizontal != 0 && dashCooldownTimer <= 0)
        {
            isDashing = true;
            currentDashTimer = startDashTimer;
            rb.linearVelocity = Vector2.zero;
            dashDirection = (int)horizontal;
        }

        if (isDashing)
        {
            currentDashTimer -= Time.deltaTime;

            if (currentDashTimer <= 0) 
            { 
                isDashing = false;
                dashCooldownTimer = dashCooldown;
            }
        }
        else if (dashCooldownTimer>0) { dashCooldownTimer -= Time.deltaTime; }


            Flip();
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocityY);
        }
        else
        {
            rb.linearVelocity = transform.right * dashDirection * dashForce;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
