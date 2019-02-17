using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float timeToJumpApex = .4f;
    [SerializeField] private float wallSlideSpeedMax = 3;
    [SerializeField] private float wallStickTime = .25f;
    [SerializeField] private Vector2 wallJumpClimb;
    [SerializeField] private Vector2 wallJumpOff;
    [SerializeField] private Vector2 wallLeap;

    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .3f;
    float velocityXSmoothing;
    float gravity;
    float jumpVelocity;
    float timeToWallUnstick;

    int wallDirectionX;

    bool wallSliding;
    bool wasGrounded;
    bool facingRight;


    Vector3 velocity;
    Vector2 directionalInput;

    Controller2D controller;
    Animator animator;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        Debug.Log("Gravity: " + gravity + " Jump Velocity: " + jumpVelocity);
    }

    private void Update()
    {
        CalculateVelocity();
        HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.below && !wasGrounded)
        {
            Debug.Log("landed");
            animator.SetBool("IsJumping", false);
        }

        wasGrounded = controller.collisions.below;

        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        animator.SetBool("IsJumping", true);

        if (wallSliding)
        {
            if (wallDirectionX == directionalInput.x)
            {
                velocity.x = -wallDirectionX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirectionX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirectionX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }

        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) // Not jumping against max slope
                {
                    velocity.y = jumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = jumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = jumpVelocity;
            }
        }
    }

    private void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        animator.SetFloat("Speed", Mathf.Abs(directionalInput.x));

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.x = directionalInput.x * moveSpeed;
        velocity.y += gravity * Time.deltaTime;

        if (directionalInput.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (directionalInput.x < 0 && facingRight)
        {
            Flip();
        }
    }

    private void HandleWallSliding()
    {
        wallDirectionX = (controller.collisions.left) ? -1 : 1;

        wallSliding = false;

        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirectionX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = facingRight;
    }
}
