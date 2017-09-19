using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PatrolPoint {
    private Vector3 position;
    private Vector3 rotation;
    private float waitTime;

    public Vector3 Position
    {
        get
        {
            return position;
        }
    }

    public Vector3 Rotation
    {
        get
        {
            return rotation;
        }
    }

    public float WaitTime
    {
        get
        {
            return waitTime;
        }
    }

    public PatrolPoint(Vector3 position, Vector3 rotation, float waitTime)
    {
        this.position = position;
        this.rotation = rotation;
        this.waitTime = waitTime;
    }
}

public abstract class EnemyRouter : MonoBehaviour {
    public static EnemyRouter Instance
    {
        get;
        private set;
    }

    // Get the route for an enemy with the given ID
    public abstract PatrolPoint[] GetRoute(uint id);

    // Get the transform at the given index for an enemy with the given ID
    public abstract PatrolPoint GetPoint(uint id, uint index);
}
