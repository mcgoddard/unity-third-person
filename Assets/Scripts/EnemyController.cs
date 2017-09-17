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
		agent = GetComponent<NavMeshAgent>();
		agent.speed = 1.5f;
        leftCone = transform.Find("LeftVisionCone").GetComponent<VisionCone>();
        rightCone = transform.Find("RightVisionCone").GetComponent<VisionCone>();
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
                if (PointInTriangle(player.transform.position, transform.position, leftCone.GetConeEnd(), rightCone.GetConeEnd()) && 
                    CanRayCastPlayer(transform.position, player.transform.position))
                {
                    currentState = EnemyState.Attacking;
                }
                else 
                {
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
    private static bool CanRayCastPlayer(Vector3 position, Vector3 playerPosition)
    {
        return true;
    }
}
