﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour {
    private const float fireDistance = 20.0f;
    private const float fireCooldown = 0.5f;
    private const float damage = 50.0f;

    public float speed = 6f;              // The speed that the player will move at.
    Vector3 m_movement;                   // The vector to store the direction of the player's movement.
    int m_floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    Rigidbody m_playerRigidbody;          // Reference to the player's rigidbody.
    LineRenderer gunRenderer;             // Reference to the player's line renderer to use for gunfire.
    float currentFireCooldown = 0;        // How much cooldown is left before the player can fire again.

	void Start() {
        gunRenderer = GetComponent<LineRenderer>();
	}

	void Awake() {
        m_floorMask = LayerMask.GetMask("Floor");

        m_playerRigidbody = GetComponent<Rigidbody>();
	}

    void Update() {
        if (currentFireCooldown > 0)
        {
            currentFireCooldown -= Time.deltaTime;
        }
        if (gunRenderer.enabled)
        {
            gunRenderer.enabled = false;
        }
        if (Input.GetMouseButtonDown(0) && currentFireCooldown <= 0)
        {
            RaycastHit hit = new RaycastHit();
            Ray bulletRay = new Ray(transform.position, transform.rotation * Vector3.forward);
            if (Physics.Raycast(bulletRay, out hit, fireDistance))
            {
                gunRenderer.SetPosition(0, transform.position);
                gunRenderer.SetPosition(1, hit.point);
                gunRenderer.enabled = true;
                currentFireCooldown = fireCooldown;
                EnemyController hitEnemy = hit.collider.gameObject.GetComponent<EnemyController>();
                if (hitEnemy != null)
                {
                    hitEnemy.TakeDamage(damage);
                }
            }
        }
    }
	
	// Update is called once per frame
	void FixedUpdate() {
        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");
		
		Move(x,z);

		Turning();
	}

	void Move(float x, float z)
	{
		//If the player is still, stop moving
		if((Math.Abs(z) > 0.5f) || (Math.Abs(x) > 0.5f))
		{
			// Set the movement vector based on the axis input.
			m_movement.Set (x, 0f, z);
			
			// Normalise the movement vector and make it proportional to the speed per second.
			m_movement = m_movement.normalized * speed * Time.deltaTime;

			// Move the player to it's current position plus the movement.
			transform.Translate(m_movement);
			
		} else {
			// Stop moving
			m_movement.Set (0f, 0f, 0f);
			transform.Translate(m_movement);
		}

	}

    void Turning()
    {
    	// Generate a plane that intersects the transform's position with an upwards normal.
    	Plane playerPlane = new Plane(Vector3.up, transform.position);
 
    	// Generate a ray from the cursor position
    	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
 
    	// Determine the point where the cursor ray intersects the plane.
    	// This will be the point that the object must look towards to be looking at the mouse.
    	// Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
    	//   then find the point along that ray that meets that distance.  This will be the point
    	//   to look at.
    	float hitdist = 0.0f;
    	// If the ray is parallel to the plane, Raycast will return false.
    	if (playerPlane.Raycast(ray, out hitdist)) 
		{
        	// Get the point along the ray that hits the calculated distance.
        	Vector3 targetPoint = ray.GetPoint(hitdist);
 
        	// Determine the target rotation.  This is the rotation if the transform looks at the target point.
        	Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
 
        	// Smoothly rotate towards the target point.
        	transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
		}
    }


}
