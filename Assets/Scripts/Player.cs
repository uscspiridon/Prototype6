using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Player : MonoBehaviour {
    // constants
    public float minSpeed;
    
    public float jumpForce;
    public float fallingGravityScale;

    // public float dashForce;
    public float dashSpeed;
    public float dashTime;
    public Color dashColor;
    
    // KeyCodes
    public KeyCode jumpKey;
    public KeyCode dashKey;

    // components & necessary junk
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Color originalColor;
    
    // state
    private bool inMidair;
    private bool isDashing;
    private float dashTimer;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start() {
        originalColor = sprite.color;
    }

    // Update is called once per frame
    void Update()
    {
        // on ground
        if (!inMidair) {
            rb.gravityScale = 1f;
            // move right
            if (rb.velocity.x < minSpeed) {
                rb.velocity = new Vector2(minSpeed, rb.velocity.y);
            }
        }

        // in midair
        if (inMidair) {
            // stronger gravity when falling
            rb.gravityScale = (rb.velocity.y < 1f) ? fallingGravityScale : 1f;
        }

        // dashing
        if (isDashing) {
            if (rb.velocity.x < dashSpeed) {
                rb.velocity = new Vector2(dashSpeed, rb.velocity.y);
            }
            // decrement dash timer
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) {
                isDashing = false;
                sprite.color = originalColor;
            }
        }

        // VERBS
        // jump
        if (Input.GetKeyDown(jumpKey)) {
            Jump();
        }
        // dash
        if (Input.GetKeyDown(dashKey)) {
            Dash();
        }
    }

    private void Jump() {
        rb.AddForce(new Vector2(0, jumpForce));
        inMidair = true;
    }

    private void Dash() {
        // rb.AddForce(new Vector2(dashForce, 0));
        isDashing = true;
        dashTimer = dashTime;
        sprite.color = dashColor;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Ground")) {
            inMidair = false;
        }
    }
}
