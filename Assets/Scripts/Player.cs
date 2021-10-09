using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Player : MonoBehaviour {
    // constants
    public float minSpeed;
    
    public float jumpForce;
    public float fallingGravityScale;

    public float dashSpeed;
    // public float dashForce;
    public float dashTime;
    public Color dashColor;
    
    // KeyCodes
    public KeyCode jumpKey;
    public KeyCode dashKey;
    public KeyCode restockVerbsKey;

    // components & necessary junk
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Color originalColor;
    public enum Verb {
        Jump,
        Dash,
    }
    
    // state
    private List<Verb> availableVerbs;
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
        availableVerbs = new List<Verb>();
        RestockVerbs();
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
        if (availableVerbs.Contains(Verb.Jump) && Input.GetKeyDown(jumpKey)) {
            Jump();
            availableVerbs.Remove(availableVerbs.Find(verb => verb == Verb.Jump));
            PrintAvailableVerbs();
        }
        // dash
        if (availableVerbs.Contains(Verb.Dash) && Input.GetKeyDown(dashKey)) {
            Dash();
            availableVerbs.Remove(availableVerbs.Find(verb => verb == Verb.Dash));
            PrintAvailableVerbs();
        }
        // restock verbs
        if(Input.GetKeyDown(restockVerbsKey)) RestockVerbs();
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

    private void RestockVerbs() {
        availableVerbs = new List<Verb>();
        // uniform distribution, all equal chance
        List<Verb> allVerbs = new List<Verb> {Verb.Jump, Verb.Dash};
        for (var i = 0; i < 3; i++) {
            var randomIndex = Random.Range(0, 3);
            var randomVerb = allVerbs[randomIndex];
            availableVerbs.Add(randomVerb);
        }
        Debug.Log("GENERATED NEW VERBS");
        PrintAvailableVerbs();
    }

    private void PrintAvailableVerbs() {
        string str = "[";
        for (int i = 0; i < 3; i++) {
            if (i >= availableVerbs.Count) {
                str += "__";
            }
            else {
                str += availableVerbs[i].ToString();
            }
            if (i < 2) str += ", ";
        }
        str += "]";
        Debug.Log(str);
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Ground")) {
            inMidair = false;
        }
    }
}
