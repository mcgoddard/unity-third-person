using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private float speed = 10;
    private bool forward = false;
    private bool backward = false;
    private bool left = false;
    private bool right = false;

    // Use this for initialization
    void Start ()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
	}
	
	// Update is called once per frame
	void Update ()
    {
        var position = transform.position;
        if (Input.GetKeyDown("w"))
        {
            forward = true;
        }
        else if (Input.GetKeyUp("w"))
        {
            forward = false;
        }
        if (Input.GetKeyDown("s"))
        {
            backward = true;
        }
        else if (Input.GetKeyUp("s"))
        {
            backward = false;
        }
        if (Input.GetKeyDown("a"))
        {
            left = true;
        }
        else if (Input.GetKeyUp("a"))
        {
            left = false;
        }
        if (Input.GetKeyDown("d"))
        {
            right = true;
        }
        else if (Input.GetKeyUp("d"))
        {
            right = false;
        }
        if (forward)
        {
            position += (transform.forward * speed * Time.deltaTime);
        }
        if (backward)
        {
            position += (transform.forward * -1 * speed * Time.deltaTime);
        }
        if (left)
        {
            position += (transform.right * -1 * speed * Time.deltaTime);
        }
        if (right)
        {
            position += (transform.right * speed * Time.deltaTime);
        }
        transform.position = position;
        
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
