using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

    private bool Stop = false;
    private Vector3 direction = Vector3.zero;
    public float speed = 2.0f;

	void Update () {
        move();
	}

    void move()
    {
        if (Input.GetKeyDown(KeyCode.A))
            direction = new Vector3(-1, 0, 0);
        if (Input.GetKeyDown(KeyCode.D))
            direction = new Vector3(1, 0, 0);
        if (Input.GetKeyDown(KeyCode.W))
            direction = new Vector3(0, 0, 1);
        if (Input.GetKeyDown(KeyCode.S))
            direction = new Vector3(0, 0, -1);
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            direction = Vector3.zero;
        GetComponent<Rigidbody>().MovePosition(transform.position + direction * Time.deltaTime * speed);
    }
    
}
