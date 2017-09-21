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

    // How far can we detect the player during a chase
    private const float maxVisionDistance = 10;
    // Speed whilst patrolling/investigating
    private const float walkingSpeed = 1.5f;
    // Speed whilst attacking
    private const float chaseSpeed = 3.0f;
    // Rotation speed modifier for turning during patrols
    private const float rotationSpeed = 3.0f;
    // Distance to approach the player during a chase
    private const float attackingDistance = 3.0f;
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

	private EnemyState currentState = EnemyState.Patrolling;
	private uint patrollingIndex = 0;
	private NavMeshAgent agent;
    private VisionCone leftCone;
    private VisionCone rightCone;
    private GameObject player;
    // How long we have been waiting (at the current patrol point)
    private float currentWaitingTime;
    // How long we have been investigating
    private float currentInvestigationTime = 0.0f;
    // The point at which the player was lost investigation will centre around this point
    private Vector3 investigationPoint;
    // The current point we'll navigate to in our investigation
    private Nullable<Vector3> investigationTarget = null;
    // How many frames since the last refresh
    private int currentFrameTimer = 0;

    public EnemyRouter Router;
    public uint Id;

	// Use this for initialization
	void Start () 
	{
        // Set up the NavMeshAgent that will handle movement, avoid obstacles
		agent = GetComponent<NavMeshAgent>();
        agent.speed = walkingSpeed;
        // Grab the components required for spotting the player
        leftCone = transform.Find("LeftVisionCone").GetComponent<VisionCone>();
        rightCone = transform.Find("RightVisionCone").GetComponent<VisionCone>();
        // Get a reference to the player game object
        player = GameObject.Find("Player");
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
                    CanRayCastTarget(transform.position, player.transform.position, player))
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
                else if (playerDistance < attackingDistance)
                {
                    agent.isStopped = true;
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
                    CanRayCastTarget(transform.position, player.transform.position, player))
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

    // Helper to check for a raycast hit to the player (are they behind cover?)
    private static bool CanRayCastTarget(Vector3 position, Vector3 targetPosition, GameObject target)
    {
        Vector3 direction = targetPosition - position;
        Ray visionRay = new Ray(position, direction);
        RaycastHit hit;
        if (Physics.Raycast(visionRay, out hit, maxVisionDistance))
        {
            return (hit.collider.gameObject == target);
        }
        return false;
    }
}
