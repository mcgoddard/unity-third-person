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

	private EnemyState m_currentState = EnemyState.Patrolling;
	private Vector3 m_investigationPoint;
	private Vector3[] m_patrollingPoints;
	private float m_currentWaitingTime;
	private float[] m_patrollingWaitTimes;
	private int m_patrollingIndex = 0;
	private NavMeshAgent m_agent;

	// Use this for initialization
	void Start () 
	{
		m_patrollingPoints = new Vector3[]
		{
			transform.position,
			new Vector3(0, transform.position.y, -6),
			new Vector3(6, transform.position.y, -6),
			new Vector3(-8, transform.position.y, 0),
		};
		m_patrollingWaitTimes = new float[]
		{
			5.0f,
			2.0f,
			2.0f,
			2.0f,
		};
		m_agent = GetComponent<NavMeshAgent>();
		m_agent.speed = 1.5f;
	}
	
	// Update is called once per frame
	void Update() 
	{
		switch (m_currentState) 
		{
			case EnemyState.Patrolling:
				float distance = Vector3.Magnitude(transform.position - m_patrollingPoints[m_patrollingIndex]);
				if (distance < 0.1)
				{
					if (m_currentWaitingTime < m_patrollingWaitTimes[m_patrollingIndex])
					{
						m_currentWaitingTime += Time.deltaTime;
					}
					else
					{
						m_currentWaitingTime = 0;
						m_patrollingIndex++;
						m_patrollingIndex = m_patrollingIndex % m_patrollingPoints.Length;
						m_agent.destination = m_patrollingPoints[m_patrollingIndex];
					}
				}
				break;
		}
	}
}
