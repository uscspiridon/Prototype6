using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Player : MonoBehaviour {
    // constants
    public float minSpeed;
    
    public float jumpForce;
    public float fallingGravityScale;
    public float groundPoundGravityScale;

    public float dashSpeed;
    // public float dashForce;
    public float dashTime;
    public Color dashColor;
    
    // KeyCodes
    public KeyCode jumpKey;
    public KeyCode dashKey;
    public KeyCode groundPoundKey;
    public KeyCode restockVerbsKey;


    //Cards to interact with
    public Card[] cards;

    // components & necessary junk
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Color originalColor;


    //Basically to help know when to set rb.y velocity to 0
    private bool jumpedAfterDash = false;
    private bool isAlive = true;
    public enum Verb {
        Jump,
        Dash,
        GroundPound,
        None
    }
    
    // state
    private List<Verb> availableVerbs;
    private bool inMidair;
    private bool isDashing;
    private bool isPounding;
    private bool startedPound;
    private float dashTimer;
    private float deathDelay = 2f;


    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start() {
        originalColor = sprite.color;
        availableVerbs = new List<Verb>(3);
        RestockVerbs();
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
            //If statement below needed?

            //if (rb.velocity.x < dashSpeed) {
               if(!jumpedAfterDash){                
                   rb.velocity = new Vector2(dashSpeed, 0);
                   rb.gravityScale=0f;
               }   

            
            //}
            
            // decrement dash timer
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) {
                isDashing = false;
                sprite.color = originalColor;
            }
        }

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
            Jump();
            RemoveVerb(Verb.Jump);
            RemoveCardUI(Array.Find(cards, card=>card.GetVerb()==Verb.Jump));
            PrintAvailableVerbs();
            JumpsToGroundPounds();
        }
        // dash
        if (availableVerbs.Contains(Verb.Dash) && Input.GetKeyDown(dashKey)) {
            Dash();
            RemoveVerb(Verb.Dash);
            RemoveCardUI(Array.Find(cards, card=>card.GetVerb()==Verb.Dash));
            PrintAvailableVerbs();
            if(inMidair){
                jumpedAfterDash = false;
            }
        }

        // ground pound
        if(availableVerbs.Contains(Verb.GroundPound) && Input.GetKeyDown(groundPoundKey))
        {
            GroundPound();
            RemoveVerb(Verb.GroundPound);
            RemoveCardUI(Verb.GroundPound);
        }
        // restock verbs
        if(Input.GetKeyDown(restockVerbsKey)) RestockVerbs();
    }
    //Called by spikes
    public void Die()
    {
        isAlive = false;
        
        //Restart level after a delay
        StartCoroutine(RestartDelay(deathDelay));
    }
    private void Jump() {
        rb.AddForce(new Vector2(0, jumpForce));
        inMidair = true;
        if(isDashing){
            jumpedAfterDash=true;
        }
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

    private void RestockVerbs() {
        availableVerbs = new List<Verb>();
        // uniform distribution, all equal chance
        List<Verb> allVerbs = new List<Verb> {Verb.Jump, Verb.Dash};
        for (var i = 0; i < 3; i++) {
            var randomIndex = Random.Range(0, 2);
            Debug.Log(randomIndex);
            var randomVerb = allVerbs[randomIndex];
            availableVerbs.Add(randomVerb);
        }
        //Debug.Log("GENERATED NEW VERBS");
        //PrintAvailableVerbs();
        if (inMidair)
        {
            JumpsToGroundPounds();
        }
        else
        {
            SetCardsUI();
        }
    }

    private void SetCardsUI(){
        for(int i =0; i<cards.Length;i++){
            if (availableVerbs.Count > i)
            {
                cards[i].SetCard(availableVerbs[i]);
            }
        }
    }

    private void RemoveVerb(Verb verb)
    {
        availableVerbs[availableVerbs.FindIndex(v => v == verb)] = Verb.None;
    }

    private void JumpsToGroundPounds(){
        while (availableVerbs.Contains(Verb.Jump))
        {
            availableVerbs[availableVerbs.FindIndex(verb => verb == Verb.Jump)] = Verb.GroundPound;
        }
        SetCardsUI();
    }
    private void GroundPoundsToJumps()
    {
        //Debug.Log("S:"+availableVerbs.FindIndex(verb => verb == Verb.GroundPound));
        while (availableVerbs.Contains(Verb.GroundPound))
        {
            Debug.Log("GPS");
            availableVerbs[availableVerbs.FindIndex(verb => verb == Verb.GroundPound)] = Verb.Jump;
        }
        SetCardsUI();
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
            GroundPoundsToJumps();
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
