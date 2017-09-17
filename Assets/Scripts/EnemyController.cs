﻿using System.Collections;
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
	private int patrollingIndex = 0;
	private NavMeshAgent agent;

	// Use this for initialization
	void Start () 
	{
		patrollingPoints = new Vector3[]
		{
			transform.position,
		 	new Vector3(0, 0.6f, -6),
			new Vector3(6, 0.6f, -6),
			new Vector3(-8, 0.6f, 0),
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
					patrollingIndex++;
					patrollingIndex = patrollingIndex % patrollingPoints.Length;
					agent.destination = patrollingPoints[patrollingIndex];
				}
				break;
		}
	}
}