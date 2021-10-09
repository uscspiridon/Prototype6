using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {
    public GameObject objectToFollow;
    public float speed;

    private Vector3 offset;

    void Start() {
        offset = transform.position - objectToFollow.transform.position;
    }

    void FixedUpdate() {
        float x = Mathf.Lerp(transform.position.x, objectToFollow.transform.position.x + offset.x, speed * Time.deltaTime);
        
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
