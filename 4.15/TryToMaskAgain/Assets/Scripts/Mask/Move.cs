using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

    private Vector3 direction = Vector3.zero;
    public float speed = 2.0f;

	void Update () {
        direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        GetComponent<Rigidbody>().MovePosition(transform.position + direction * Time.deltaTime * speed);
    }
}
