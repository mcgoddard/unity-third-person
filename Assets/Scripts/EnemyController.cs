using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {
	public enum EnemyState
	{
		Patrolling,
		Attacking
	}

    private static float maxVisionDistance = 10;

	private EnemyState currentState = EnemyState.Patrolling;
	private Vector3[] patrollingPoints;
	private float currentWaitingTime;
	private float[] patrollingWaitTimes;
	private int patrollingIndex = 0;
	private NavMeshAgent agent;
    private VisionCone leftCone;
    private VisionCone rightCone;
    private GameObject player;

	// Use this for initialization
	void Start () 
	{
        // Set up the patrol route and wait times for each point along that route
        // The patrol route will loop once complete
		patrollingPoints = new Vector3[]
		{
			transform.position,
			new Vector3(0, transform.position.y, -6),
			new Vector3(6, transform.position.y, -6),
			new Vector3(-8, transform.position.y, 0),
		};
		patrollingWaitTimes = new float[]
		{
			5.0f,
			2.0f,
			2.0f,
			2.0f,
		};
        // Set up the NavMeshAgent that will handle movement, avoid obstacles
		agent = GetComponent<NavMeshAgent>();
		agent.speed = 1.5f;
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
            case EnemyState.Attacking:
                Debug.Log("Attacking");
                agent.isStopped = true;
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
    				float distance = Vector3.Magnitude(transform.position - patrollingPoints[patrollingIndex]);
    				if (distance < 0.1)
    				{
    					if (currentWaitingTime < patrollingWaitTimes[patrollingIndex])
    					{
    						currentWaitingTime += Time.deltaTime;
    					}
    					else
    					{
    						currentWaitingTime = 0;
    						patrollingIndex++;
    						patrollingIndex = patrollingIndex % patrollingPoints.Length;
    						agent.destination = patrollingPoints[patrollingIndex];
    					}
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
    private static bool CanRayCastTarget(Vector3 position, Vector3 playerPosition, GameObject target)
    {
        Vector3 direction = playerPosition - position;
        Ray visionRay = new Ray(position, direction);
        RaycastHit hit;
        if (Physics.Raycast(visionRay, out hit, maxVisionDistance))
        {
            return (hit.collider.gameObject == target);
        }
        return false;
    }
}
