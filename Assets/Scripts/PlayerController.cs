using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour {
    public float Speed = 6.0f;

    private const float fireDistance = 20.0f;   // Max distance of a shot
    private const float fireCooldown = 0.2f;    // Time between shots
    private const float reloadCooldown = 2.0f;  // Time to reload
    private const float damage = 50.0f;   // Amount of damage to deal on a successful shot
    private const int startAmmo = 18;     // Amount of (unloaded) ammo at starting
    private const float interactionDistance = 4.0f; // Distance at which the player can interact with an object

    public const int MagazineCount = 6;  // Number of rounds when the weapon is fully loaded
    public const float MaxHealth = 100;   // How much health the player has at starting

    int m_floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    Rigidbody m_playerRigidbody;          // Reference to the player's rigidbody.
    LineRenderer gunRenderer;             // Reference to the player's line renderer to use for gunfire.
    float currentFireCooldown = -0.1f;    // How much cooldown is left before the player can fire again.
    float currentReloadCooldown = -0.1f;  // How much cooldown is left before the player has reloaded. 
    int currentRemainingAmmo;             // Amount of (unloaded) ammo remaining
    int currentMagazineCount;             // Number of currently loaded bullets
    float currentHealth;                  // Current health
    Vector3 m_movement;                   // The vector to store the direction of the player's movement.
    private GameObject m_player;
    private GameObject[] interactives;
    private int goldStolen = 0;

	void Start() 
    {
        gunRenderer = GetComponent<LineRenderer>();
        currentRemainingAmmo = startAmmo - MagazineCount;
        currentMagazineCount = MagazineCount;
        currentHealth = MaxHealth;

        m_player = GameObject.FindGameObjectWithTag("Player");
        interactives = GameObject.FindGameObjectsWithTag("Interactive");
	}

	void Awake() 
    {
        m_floorMask = LayerMask.GetMask("Floor");

        m_playerRigidbody = GetComponent<Rigidbody>();
	}

    void Update() 
    {
        //Keyboard Controls
        var x = 0;
        var z = 0;
        if(Input.GetKey(KeyCode.W))
        {
            z = 1;
        }
        if(Input.GetKey(KeyCode.A))
        {
            x = -1;
        }
        if(Input.GetKey(KeyCode.S))
        {
            z = -1;
        }
        if(Input.GetKey(KeyCode.D))
        {
            x = 1;
        }
		Move(x,z);
        //Turn the player
		Turning();

        // Interact logic
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Find the closest interactable
            GameObject selected = null;
            float selectedDistance = 0.0f;
            foreach (var interactive in interactives)
            {
                var distance = (interactive.transform.position - transform.position).magnitude;
                if (distance < interactionDistance && 
                    (selected == null || selectedDistance > distance))
                {
                    selected = interactive;
                    selectedDistance = distance;
                }
            }
            if (selected != null)
            {
                // Differentiate the interaction required
                if (selected.name == "Safe")
                {
                    Safe safeScript = selected.GetComponent<Safe>();
                    if (safeScript.CanOpen())
                    {
                        safeScript.Open();
                    }
                    else if (safeScript.CanSteal())
                    {
                        safeScript.Steal();
                        goldStolen += 10;
                    }
                }
                else if (selected.name == "Gold Bar")
                {
                    GoldBar barScript = selected.GetComponent<GoldBar>();
                    if (barScript.CanSteal())
                    {
                        barScript.Steal();
                        goldStolen += 1;
                    }
                }
            }
        }

        // Shooting logic
        if (gunRenderer.enabled)
        {
            gunRenderer.enabled = false;
        }
        if (currentFireCooldown > 0)
        {
            currentFireCooldown -= Time.deltaTime;
        }
        else if (currentReloadCooldown > 0)
        {
            currentReloadCooldown -= Time.deltaTime;
            if (currentReloadCooldown <= 0)
            {
                int toReload = MagazineCount - currentMagazineCount;
                int availableForReload = currentRemainingAmmo >= toReload ? toReload : currentRemainingAmmo;
                currentMagazineCount += availableForReload;
                currentRemainingAmmo -= availableForReload;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && currentMagazineCount >= 1)
            {
                currentMagazineCount -= 1;
                RaycastHit hit = new RaycastHit();
                Vector3 fireFrom = transform.position;
                fireFrom.y += 1.18f;
                Ray bulletRay = new Ray(fireFrom, m_player.transform.rotation * Vector3.forward);
                if (Physics.Raycast(bulletRay, out hit, fireDistance))
                {
                    gunRenderer.SetPosition(0, fireFrom);
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
            else if (currentMagazineCount < MagazineCount && currentRemainingAmmo > 0 && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.R)))
            {
                currentReloadCooldown = reloadCooldown;
            }
        }
    }

    void Move(float x, float z)
	{
        // Set the movement vector based on the axis input.
        m_movement.Set (x, 0f, z);
        
        // Normalise the movement vector and make it proportional to the speed per second.
        m_movement = m_movement.normalized * Speed * Time.deltaTime;

        //Debug.Log("m_movement:" + m_movement);
        // Move the player to it's current position plus the movement.
        this.transform.Translate(m_movement);
	}

    void Turning()
    {
    	// Generate a plane that intersects the transform's position with an upwards normal.
    	Plane playerPlane = new Plane(Vector3.up, m_player.transform.localPosition);
 
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
        	Quaternion targetRotation = Quaternion.LookRotation(targetPoint - m_player.transform.position);
            targetRotation *= Quaternion.Euler(270, 0, 90); // this adds a 90 degrees Y rotation
 
        	// Smoothly rotate towards the target point.
        	m_player.transform.localRotation = Quaternion.Slerp(m_player.transform.localRotation, targetRotation, Speed * Time.deltaTime);
		}
        //Keep base rotation fixed
        transform.rotation = new Quaternion(0,0,0,0);
    }

    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
    }

    public int CurrentAmmo
    {
        get
        {
            return currentRemainingAmmo;
        }
    }

    public int CurrentLoaded
    {
        get
        {
            return currentMagazineCount;
        }
    }
}
