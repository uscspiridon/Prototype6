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

    public float dashSpeed;
    // public float dashForce;
    public float dashTime;
    public Color dashColor;
    
    // KeyCodes
    public KeyCode jumpKey;
    public KeyCode dashKey;
    public KeyCode restockVerbsKey;

    //Cards to interact with
    public Card[] cards;

    // components & necessary junk
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Color originalColor;


    //Basically to help know when to set rb.y velocity to 0
    private bool jumpingWhileDashing = false;

    private bool isAlive = true;
    public enum Verb {
        Jump,
        Dash,
        None
    }
    
    // state
    private List<Verb> availableVerbs;
    private bool inMidair;
    private bool isDashing;
    private float dashTimer;
    private float deathDelay = 2f;


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
               if(!jumpingWhileDashing){
                
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

        // VERBS
        // jump
        if (availableVerbs.Contains(Verb.Jump) && Input.GetKeyDown(jumpKey)) {
            Jump();
            availableVerbs.Remove(availableVerbs.Find(verb => verb == Verb.Jump));
            RemoveCardUI(Array.Find(cards, card=>card.GetVerb()==Verb.Jump));
            PrintAvailableVerbs();
        }
        // dash
        if (availableVerbs.Contains(Verb.Dash) && Input.GetKeyDown(dashKey)) {
            Dash();
            availableVerbs.Remove(availableVerbs.Find(verb => verb == Verb.Dash));
            RemoveCardUI(Array.Find(cards, card=>card.GetVerb()==Verb.Dash));

            PrintAvailableVerbs();
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
            jumpingWhileDashing=true;
        }
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
        List<Verb> allVerbs = new List<Verb> {Verb.Jump, Verb.Dash, Verb.None};
        for (var i = 0; i < 3; i++) {
            var randomIndex = Random.Range(0, 3);
            Debug.Log(randomIndex);
            var randomVerb = allVerbs[randomIndex];
            availableVerbs.Add(randomVerb);
        }
        Debug.Log("GENERATED NEW VERBS");
        PrintAvailableVerbs();
        SetCardsUI(); 
    }

    private void SetCardsUI(){
        for(int i =0; i<cards.Length;i++){
            cards[i].SetCard(availableVerbs[i]);
        }
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
            jumpingWhileDashing=false;
        }
    }

    //Called upon player death
    IEnumerator RestartDelay(float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
    }

}
