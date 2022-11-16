using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    private const int V = 0;
    [SerializeField] float runSpeed = 6f;
    [SerializeField] float jumpHeight = 17f;
    [SerializeField] float climbSpeed = 8f;
    [SerializeField] float maxAirTime = 2f;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform weapon;
    Vector2 onDeathJump = new Vector2 (10f, 20f);
    Vector2  moveInput;
    Rigidbody2D playerRigidbody;
    Animator playerAnimator;
    CapsuleCollider2D playerBodyCollider;
    BoxCollider2D playerFeetCollider;
    float startPlayerGravity;
    bool isJumping;
    bool isAlive = true;

    float currentAirTime;   
    
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerBodyCollider = GetComponent<CapsuleCollider2D>();
        playerFeetCollider = GetComponent<BoxCollider2D>();
        startPlayerGravity = playerRigidbody.gravityScale;
    }

    void Update()
    {
        if(!isAlive) { return; }
        Run();
        FlipSprite();
        JumpCheck();
        ClimbLadder();
        Death();
    }

    void OnMove(InputValue value)
    {
        if(!isAlive) { return; }
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if(!isAlive) { return; }
        if (!playerFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground", "Ladder")))
        {
            return;
        }
        if(value.isPressed)
        {
            isJumping = true;
            currentAirTime = 0;
            playerRigidbody.velocity = new Vector2(0f, jumpHeight);
        }
    }

    void OnFire(InputValue value)
    {
        if(!isAlive) { return; }
        Instantiate(bullet, weapon.position, transform.rotation);
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, playerRigidbody.velocity.y);
        playerRigidbody.velocity = playerVelocity;

        bool playerIsMoving = Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Epsilon;
        playerAnimator.SetBool("isRunning", playerIsMoving);
        
    }

    void FlipSprite()
    {
        bool playerIsMoving = Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Epsilon;
        if(playerIsMoving)
        {
        transform.localScale = new Vector2 (Mathf.Sign(playerRigidbody.velocity.x), 1f);
        }
    }

     void ClimbLadder()
    {
        if (!playerFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ladder")) || isJumping)
        {
            playerRigidbody.gravityScale = startPlayerGravity;
            playerAnimator.SetBool("isClimbing", false); 
            return;
        }  
        if(moveInput.y != 0)
        {
            Vector2 climbVelocity = new Vector2(playerRigidbody.velocity.x, moveInput.y * climbSpeed);
            playerRigidbody.velocity = climbVelocity; 
            playerRigidbody.gravityScale = 0f;


            bool playerIsClimbing = Mathf.Abs(playerRigidbody.velocity.y) > Mathf.Epsilon;
            playerAnimator.SetBool("isClimbing", playerIsClimbing); 
        }
        if(moveInput.y == 0f && isJumping == false && moveInput.x == 0f)
        {
            playerRigidbody.velocity = Vector2.zero;
            playerRigidbody.gravityScale = 0f;
            playerAnimator.SetBool("isClimbing", false);

        }
    }

    void JumpCheck()
    {
        if(isJumping && currentAirTime < maxAirTime)
        {
            currentAirTime += Time.deltaTime;
        }
        if(isJumping && currentAirTime > maxAirTime)
        {
            isJumping = false;
        }
        
    }
        
    void Death()
    {
        if(playerBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            isAlive = false;
            playerAnimator.SetTrigger("Dying");
            playerRigidbody.velocity = onDeathJump;
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }
}
