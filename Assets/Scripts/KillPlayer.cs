using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag=="Player"){
            other.gameObject.GetComponent<Player>().Die();
        }
    }
}
