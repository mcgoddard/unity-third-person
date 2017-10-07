using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class EnemyController : MonoBehaviour {
	public enum EnemyState
	{
		Patrolling,
		Attacking,
        Investigating
	}

    public AudioClip FireClip;
    public AudioClip ReloadClip;

    private const float fireDistance = 20.0f;   // Max distance of a shot
    // How far can we detect the player during a chase
    private const float maxVisionDistance = 10;
    // Speed whilst patrolling/investigating
    private const float walkingSpeed = 4.5f;
    // Speed whilst attacking
    private const float chaseSpeed = 3.0f;
    // Rotation speed modifier for turning during patrols
    private const float rotationSpeed = 3.0f;
    // Distance to approach the player during a chase
    private const float attackingDistance = 5.0f;
    // Distance at which we cannot lose the player during a chase, even if LoS is broken
    private const float minLosingDistance = 5.0f;
    // Time to spend investigating before returning to the patrol
    private const float investigationTime = 20.0f;
    // Distance from a target at which to assume it's arrived
    private const float basicallyThereDistance = 0.1f;
    // Furthest distance an investigation point can be from the last place a player was spotted
    private const float maxInvestigationDistance = 5.0f;
    // Number of frames to recalculate routing
    private const int recalculateFrameTimer = 10;
    //Maximum health of the enemy
    private const float maxHealth = 100;
    public const int MagazineCount = 6;  // Number of rounds when the weapon is fully loaded
    private const int startAmmo = 18;     // Amount of (unloaded) ammo at starting
    private const float fireCooldown = 1.0f;    // Time between shots
    private const float reloadCooldown = 1.0f;  // Time to reload
    private const float damage = 20.0f;   // Amount of damage to deal on a successful shot

	private EnemyState currentState = EnemyState.Patrolling;
	private uint patrollingIndex = 0;
	private NavMeshAgent agent;
    private VisionCone leftCone;
    private VisionCone rightCone;
    private List<Bounds> safeZones;
    private GameObject player;
    private int playerInstanceId;
    private float currentWaitingTime;// How long we have been waiting (at the current patrol point)   
    private float currentInvestigationTime = 0.0f;// How long we have been investigating  
    private Vector3 investigationPoint;// The point at which the player was lost investigation will centre around this point
    private Nullable<Vector3> investigationTarget = null;// The current point we'll navigate to in our investigation
    private int currentFrameTimer = 0;// How many frames since the last refresh
    public EnemyRouter Router;
    public uint Id;
    private float health;    
    LineRenderer gunRenderer;  // Reference to the player's line renderer to use for gunfire.   
    private AudioSource audioSource;           
    private int currentRemainingAmmo; // Amount of (unloaded) ammo remaining     
    int currentMagazineCount; // Number of currently loaded bullets        
    float currentFireCooldown = -0.1f;    // How much cooldown is left before the player can fire again.
    float currentReloadCooldown = -0.1f;  // How much cooldown is left before the player has reloaded.  

	// Use this for initialization
	void Start () 
	{
        // Set up the NavMeshAgent that will handle movement, avoid obstacles
		agent = GetComponent<NavMeshAgent>();
        agent.speed = walkingSpeed;
        // Grab the components required for spotting the player        
        safeZones = new List<Bounds>();
        GameObject[] gos = GameObject.FindGameObjectsWithTag("SafeZone");
        saveBounds(gos);
        //safeZone.SetActive(true);
        leftCone = transform.Find("LeftVisionCone").GetComponent<VisionCone>();
        rightCone = transform.Find("RightVisionCone").GetComponent<VisionCone>();
        // Get a reference to the player game object
        player = GameObject.Find("Player");
        playerInstanceId = player.GetInstanceID();

        health = maxHealth;
        gunRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        currentRemainingAmmo = startAmmo - MagazineCount;
        currentMagazineCount = MagazineCount;
	}
    
    // Update is called once per frame
    void Update() 
	{
		switch (currentState) 
		{
            case EnemyState.Investigating:
                // Invesigate another point
                if (!investigationTarget.HasValue || (transform.position - investigationTarget.Value).magnitude < basicallyThereDistance)
                {
                    investigationTarget = null;
                    while (!investigationTarget.HasValue)
                    {
                        float rotation = UnityEngine.Random.value * 360;
                        float distance = UnityEngine.Random.value * maxInvestigationDistance;
                        Vector3 newInvestigationTarget = (Quaternion.Euler(new Vector3(0, rotation, 0)) * new Vector3(0, 0, distance)) + investigationPoint;
                        NavMeshPath path = new NavMeshPath();
                        agent.CalculatePath(newInvestigationTarget, path);
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            agent.isStopped = false;
                            investigationTarget = newInvestigationTarget;
                            agent.destination = newInvestigationTarget;
                        }
                    }
                }
                if (PointInTriangle(player.transform.position, transform.position, leftCone.GetConeEnd(), rightCone.GetConeEnd()) &&
                    CanRayCastTarget(transform.position, player.transform.position, player) && 
                    !playerInSafeZone())
                {
                    investigationTarget = null;
                    currentInvestigationTime = 0.0f;
                    currentState = EnemyState.Attacking;                    
                }
                else if (currentInvestigationTime < investigationTime)
                {
                    currentInvestigationTime += Time.deltaTime;
                }
                else
                {
                    investigationTarget = null;
                    currentInvestigationTime = 0.0f;
                    currentState = EnemyState.Patrolling;
                }
                break;
            case EnemyState.Attacking:
                float playerDistance = (transform.position - player.transform.position).magnitude;
                if (playerDistance > maxVisionDistance || (playerDistance > minLosingDistance && !CanRayCastTarget(transform.position, player.transform.position, player)))
                {
                    currentState = EnemyState.Investigating;
                    investigationPoint = player.transform.position;
                }
                else if (playerDistance < attackingDistance && CanRayCastTarget(transform.position, player.transform.position, player))
                {
                    agent.isStopped = true;
                    Ray shootRay = new Ray(transform.position, transform.forward);
                    RaycastHit shootHit = new RaycastHit();
                    if (Physics.Raycast(shootRay, out shootHit, attackingDistance) && shootHit.collider.gameObject == player)
                    {
                        Shoot();
                    }
                    else
                    {
                        // Turn towards player
                        Vector3 direction = player.transform.position - transform.position;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
                    }
                }
                else if (currentFrameTimer >= recalculateFrameTimer)
                {
                    agent.isStopped = false;
                    agent.speed = chaseSpeed;
                    agent.destination = player.transform.position;
                    currentFrameTimer = 0;
                }
                else
                {
                    currentFrameTimer++;
                }
                break;
			case EnemyState.Patrolling:
                // Check if we can spot a player, if so switch to attacking state
                if (PointInTriangle(player.transform.position, transform.position, leftCone.GetConeEnd(), rightCone.GetConeEnd()) && 
                    CanRayCastTarget(transform.position, player.transform.position, player) && 
                    !playerInSafeZone())
                {
                    currentState = EnemyState.Attacking;                    
                }
                // Otherwise continue the patrol route
                else 
                {
                    // The NavMeshAgent is handling moving between points so simply check if we've reached a point
                    // handle the wait time at that point, then set a new destination for the NavMeshAgent.
                    float distance = Vector3.Magnitude(transform.position - Router.GetPoint(Id, patrollingIndex).Position);
                    if (distance < basicallyThereDistance)
                    {
                        if (currentWaitingTime < Router.GetPoint(Id, patrollingIndex).WaitTime)
                        {
                            if (currentWaitingTime < 0.001)
                            {
                                // Set turn direction
                                agent.isStopped = true;
                            }
                            Vector3 rotation = Vector3.Lerp(transform.rotation.eulerAngles, Router.GetPoint(Id, patrollingIndex).Rotation, Time.deltaTime * rotationSpeed);
                            transform.rotation = Quaternion.Euler(rotation);
                            currentWaitingTime += Time.deltaTime;
                        }
                        else
                        {
                            // Set new destination
                            currentWaitingTime = 0;
                            patrollingIndex++;
                            patrollingIndex = (uint)(patrollingIndex % Router.GetRoute(Id).Length);
                            agent.destination = Router.GetPoint(Id, patrollingIndex).Position;
                            agent.isStopped = false;
                        }
                    }
                    else if (agent.destination != Router.GetPoint(Id, patrollingIndex).Position)
                    {
                        agent.destination = Router.GetPoint(Id, patrollingIndex).Position;
                        agent.isStopped = false;
                    }
                }
				break;
		}
	}


    // Shooting logic
    void Shoot()
    {      
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
            if (currentMagazineCount >= 1)
            {
                currentFireCooldown = fireCooldown;
                audioSource.PlayOneShot(FireClip);
                currentMagazineCount -= 1;
                RaycastHit hit = new RaycastHit();
                Vector3 fireFrom = transform.position;
                fireFrom.y += 1.18f;
                Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90, 90);
                Ray bulletRay = new Ray(fireFrom, targetRotation * Vector3.up);
                if (Physics.Raycast(bulletRay, out hit, fireDistance))
                {
                    gunRenderer.SetPosition(0, fireFrom);
                    gunRenderer.SetPosition(1, hit.point);
                    gunRenderer.enabled = true;
                    PlayerController hitEnemy = hit.collider.gameObject.GetComponent<PlayerController>();
                    if (hitEnemy != null)
                    {
                        hitEnemy.TakeDamage(damage);
                    }
                }
            }
            else if (currentMagazineCount < MagazineCount && currentRemainingAmmo > 0)
            {
                currentReloadCooldown = reloadCooldown;
                audioSource.PlayOneShot(ReloadClip);
            }
        }
    }

    // Helper to decide if two points are on the same side of a line
    private static bool SameSide(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b)
    {
        var cp1 = Vector3.Cross(b - a, p1 - a);
        var cp2 = Vector3.Cross(b - a, p2 - a);
        return Vector3.Dot(cp1, cp2) >= 0;
    }

    // Helper to decide if a point is within a triangle
    private static bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        return (SameSide(p, a, b, c) && SameSide(p, b, a, c) && SameSide(p, c, a, b));
    }

    //Save the bounds of the safe zones so we don't have to keep pulling them
    private void saveBounds(GameObject[] zones)
    {
        foreach (GameObject zone in zones)
        {
            Collider c = (Collider)(zone.GetComponent(typeof(Collider)));
            Bounds b = c.bounds;
            safeZones.Add(b);
        }
    }

    // Is player in any listed safe zone
    private bool playerInSafeZone()
    {
        foreach (Bounds b in safeZones)
        {
            //nice quick vector maths
            if (b.Contains(player.transform.position))
            {
                return true;
            }
        }
        return false;
    }

    // Helper to check for a raycast hit to the player (are they behind cover?)
    private static bool CanRayCastTarget(Vector3 position, Vector3 targetPosition, GameObject target)
    {
        Vector3 direction = targetPosition - position;
        Ray visionRay = new Ray(position, direction);
        RaycastHit hit;
        if (Physics.Raycast(visionRay, out hit, maxVisionDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return (hit.collider.gameObject == target);
        }
        return false;
    }

    //Enemy health property
    public float Health
    {
        get {return health;}
    }

    //Enemy takes damage
    public void TakeDamage(float damage)
    {
        //Take damage and wait for sweet embrase of death
        if((health -= damage) <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
