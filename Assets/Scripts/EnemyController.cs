using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {
	public enum EnemyState
	{
		Patrolling,
		Investigating,
		Attacking
	}

	private EnemyState currentState = EnemyState.Patrolling;
	private Vector3 investigationPoint;
	private Vector3[] patrollingPoints;
	private float currentWaitingTime;
	private float[] patrollingWaitTimes;
	private int patrollingIndex = 0;
	private NavMeshAgent agent;

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
	}
	
	// Update is called once per frame
	void Update() 
	{
		switch (currentState) 
		{
			case EnemyState.Patrolling:
				float distance = Vector3.Magnitude(transform.position - patrollingPoints[patrollingIndex]);
				if (distance < 0.05)
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
				break;
		}
	}
}
