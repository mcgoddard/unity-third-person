using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private float speed = 10;
    private bool forward = false;
    private bool backward = false;
    private bool left = false;
    private bool right = false;
    private Rigidbody body;

    // Use this for initialization
    void Start ()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        var velocity = body.velocity;
        if (Input.GetKeyDown("w"))
        {
            forward = true;
        }
        else if (Input.GetKeyUp("w"))
        {
            forward = false;
            velocity = Vector3.zero;
        }
        if (Input.GetKeyDown("s"))
        {
            backward = true;
        }
        else if (Input.GetKeyUp("s"))
        {
            backward = false;
            velocity = Vector3.zero;
        }
        if (Input.GetKeyDown("a"))
        {
            left = true;
        }
        else if (Input.GetKeyUp("a"))
        {
            left = false;
            velocity = Vector3.zero;
        }
        if (Input.GetKeyDown("d"))
        {
            right = true;
        }
        else if (Input.GetKeyUp("d"))
        {
            right = false;
            velocity = Vector3.zero;
        }
        if (forward)
        {
            velocity = (transform.forward * speed);
        }
        if (backward)
        {
            velocity = (transform.forward * -1 * speed);
        }
        if (left)
        {
            velocity = (transform.right * -1 * speed);
        }
        if (right)
        {
            velocity = (transform.right * speed);
        }
        //transform.position = position;
        body.velocity = velocity;
        
        Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
        float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);
        transform.rotation = Quaternion.Euler(new Vector3(0f, -angle, 0f));
    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
}
