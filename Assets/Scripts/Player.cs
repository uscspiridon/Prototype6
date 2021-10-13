using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class Player : MonoBehaviour {
    // constants
    public float minSpeed;
    
    public float jumpForce;
    public float longJumpForce;
    public float highJumpMultiplier;
    public float fallingGravityScale;
    public float groundPoundGravityScale;

    public float dashSpeed;
    // public float dashForce;
    public float dashTime;
    public Color dashColor;

    public float crouchTime;
    
    // KeyCodes
    public KeyCode jumpKey;
    public KeyCode dashKey;
    public KeyCode crouchKey;
    public KeyCode reshuffleKey;
    
    //Cards to interact with
    public Card[] cards;

    // components & necessary junk
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Color originalColor;
    private Animator animator;
    public static Player Instance;
    
    // Basically to help know when to set rb.y velocity to 0
    private bool jumpedAfterDash = false;
    private bool isAlive = true;
    
    public enum Verb {
        Jump,
        Dash,
        Crouch,
        None
    }
    
    // state
    private List<Verb> availableVerbs;
    private bool inMidair;
    
    public bool isDashing;
    private float dashTimer;

    public bool isCrouching;
    private float crouchTimer;
    private bool isPounding;
    private bool startedPound;
    
    private float deathDelay = 0f;

    private void Awake() {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start() {
        originalColor = sprite.color;
        availableVerbs = new List<Verb>(3);
        ReshuffleVerbs();
    }

    // Update is called once per frame
    void Update()
    {
        // on ground
        if (!inMidair) {
            rb.gravityScale = 1f;
            
            //Only add speed if alive
            if(isAlive){
                // move right
                if (rb.velocity.x < minSpeed) {
                    rb.velocity = new Vector2(minSpeed, rb.velocity.y);
                }
            }   

        }

        // in midair
        if (inMidair) {
            // stronger gravity when falling
            rb.gravityScale = (rb.velocity.y < 1f) ? fallingGravityScale : 1f;
        }

        // dashing
        if (isDashing) {
           if(!jumpedAfterDash){                
               rb.velocity = new Vector2(dashSpeed, 0);
               rb.gravityScale=0f;
           }   

           // decrement dash timer
           dashTimer -= Time.deltaTime;
           if (dashTimer <= 0) { 
               isDashing = false;
           }
        }
        else sprite.color = originalColor;

        // crouching
        if (isCrouching) {
            crouchTimer -= Time.deltaTime;
            if (crouchTimer <= 0) {
                isCrouching = false;
            }
        }
        animator.SetBool("crouched", isCrouching);


        if (isPounding)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            rb.gravityScale = groundPoundGravityScale;
        }

        if (startedPound)
        {
            rb.velocity = new Vector2(0, 0);
            rb.gravityScale = 0;
        }

        // VERBS
        // jump
        if (availableVerbs.Contains(Verb.Jump) && Input.GetKeyDown(jumpKey)) {
            inMidair = true;
            
            float xForce = 0;
            float yForce = jumpForce;
            // longer jump
            if (isDashing) {
                xForce = longJumpForce;
                isDashing = false;
            }
            // higher jump 
            if (isCrouching) {
                yForce *= highJumpMultiplier;
                isCrouching = false;
            }
            Debug.Log("adding force " + xForce + " and " + yForce);
            rb.AddForce(new Vector2(xForce, yForce));

            RemoveVerb(Verb.Jump);
            // RemoveCardUI(Array.Find(cards, card=>card.GetVerb()==Verb.Jump));
            SetCardsUI();

            PrintAvailableVerbs();
        }
        // dash
        if (availableVerbs.Contains(Verb.Dash) && Input.GetKeyDown(dashKey)) {
            Dash();
            RemoveVerb(Verb.Dash);
            // RemoveCardUI(Array.Find(cards, card=>card.GetVerb()==Verb.Dash));
            SetCardsUI();

            PrintAvailableVerbs();
            if(inMidair){
                jumpedAfterDash = false;
            }
        }
        // crouch
        if(availableVerbs.Contains(Verb.Crouch) && Input.GetKeyDown(crouchKey))
        {
            // crouch
            isCrouching = true;
            crouchTimer = crouchTime;
            // ground pound if in midair
            if (inMidair) {
                GroundPound();
            }
            
            RemoveVerb(Verb.Crouch);
            SetCardsUI();
            // RemoveCardUI(Array.Find(cards, card => card.GetVerb() == Verb.Crouch));
        }
        // restock verbs
        if (Input.GetKeyDown(reshuffleKey)) {
            ReshuffleVerbs();
            Score.Instance.ApplyReshufflePenalty();
        }
    }
    //Called by spikes
    public void Die()
    {
        isAlive = false;
        
        //Restart level after a delay
        StartCoroutine(RestartDelay(deathDelay));
    }

    private void Dash() {
        // rb.AddForce(new Vector2(dashForce, 0));
        isDashing = true;
        dashTimer = dashTime;
        sprite.color = dashColor;
    }

    private void GroundPound()
    {
        StartCoroutine(GroundPoundProcess(0.2f));
    }

    private void ReshuffleVerbs() {
        availableVerbs = new List<Verb>();
        // uniform distribution, all equal chance
        List<Verb> allVerbs = new List<Verb> {Verb.Jump, Verb.Dash, Verb.Crouch};
        for (var i = 0; i < 3; i++) {
            var randomIndex = Random.Range(0, allVerbs.Count);
            var randomVerb = allVerbs[randomIndex];
            availableVerbs.Add(randomVerb);
            // rebalance probabilities
            if (randomVerb == Verb.Dash || randomVerb == Verb.Crouch) {
                allVerbs.Add(Verb.Jump);
            }
            if (randomVerb == Verb.Jump) {
                allVerbs.Add(Verb.Dash);
                allVerbs.Add(Verb.Crouch);
            }
        }

        Verb lastVerb = availableVerbs[0];
        bool allTheSame = true;
        for (int i = 1; i < availableVerbs.Count; i++) {
            if (availableVerbs[i] != lastVerb) {
                allTheSame = false;
                break;
            }
        }
        if(allTheSame) ReshuffleVerbs();
        // Debug.Log("GENERATED NEW VERBS");
        // PrintAvailableVerbs();
        SetCardsUI();
    }

    private void SetCardsUI(){
        for (int i = 0; i < cards.Length; i++) {
            // if(i >= availableVerbs.Count) cards[i].SetCard(Verb.None);
            cards[i].SetCard(availableVerbs[i]);
        }
        // for(int i =0; i<cards.Length;i++){
        //     if (availableVerbs.Count > i)
        //     {
        //         cards[i].SetCard(availableVerbs[i]);
        //     }
        // }
    }

    private void RemoveVerb(Verb verb)
    {
        availableVerbs[availableVerbs.FindIndex(v => v == verb)] = Verb.None;
        
        foreach (Verb availableVerb in availableVerbs) {
            if (availableVerb != Verb.None) return;
        }
        ReshuffleVerbs();
    }

    private void RemoveCardUI(Card card){
        card.SetCard(Verb.None);
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
            jumpedAfterDash=false;
            isPounding = false;
        }
    }

    //Called upon player death
    IEnumerator RestartDelay(float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
    }

    //Called for GroundPound, rotate then fall
    IEnumerator GroundPoundProcess(float time)
    {
        StartCoroutine(RotateObject(180, new Vector3(0, 0, 1f), time));
        startedPound = true;
        yield return new WaitForSeconds(time);
        isPounding = true;
        startedPound = false;
    }

    IEnumerator RotateObject(float angle, Vector3 axis, float inTime)
    {
        // calculate rotation speed
        float rotationSpeed = angle / inTime;

        //while (true)
        //{
            // save starting rotation position
            Quaternion startRotation = transform.rotation;

            float deltaAngle = 0;

            // rotate until reaching angle
            while (deltaAngle < angle)
            {
                deltaAngle += rotationSpeed * Time.deltaTime;
                deltaAngle = Mathf.Min(deltaAngle, angle);

                transform.rotation = startRotation * Quaternion.AngleAxis(deltaAngle, axis);

                yield return null;
            }

            // delay here
            yield return new WaitForSeconds(1);
        //}
    }

}
