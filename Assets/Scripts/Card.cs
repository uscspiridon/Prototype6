using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Card : MonoBehaviour
{
    public Sprite dash;
    public Sprite jump;
    public Sprite groundPound;
    public Sprite blank;
    private Player.Verb verb = Player.Verb.None;
    private Image thisImage;

    private void Awake() {
        thisImage = gameObject.GetComponent<Image>();
    }
    public void SetCard(Player.Verb verb){
        this.verb=verb;
        if(verb==Player.Verb.Dash){
           thisImage.sprite=dash;
        }
        else if (verb == Player.Verb.Crouch)
        {
            thisImage.sprite = groundPound;
        }
        else if(verb==Player.Verb.Jump){
           thisImage.sprite=jump;
        }
        else if(verb==Player.Verb.None){
            thisImage.sprite=blank;
        }
    }
    public Player.Verb GetVerb(){
        return verb;
    }


}
