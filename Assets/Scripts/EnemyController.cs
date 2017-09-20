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
    private const float investigationTime = 10.0f;

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
    private Vector3 investigationTarget;

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
                if (PointInTriangle(player.transform.position, transform.position, leftCone.GetConeEnd(), rightCone.GetConeEnd()) && 
                    CanRayCastTarget(transform.position, player.transform.position, player))
                {
                    currentState = EnemyState.Attacking;
                }
                else if (currentInvestigationTime < investigationTime)
                {
                    currentInvestigationTime += Time.deltaTime;
                }
                else
                {
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
                else
                {
                    agent.isStopped = false;
                    agent.speed = chaseSpeed;
                    agent.destination = player.transform.position;
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
                    if (distance < 0.1)
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
