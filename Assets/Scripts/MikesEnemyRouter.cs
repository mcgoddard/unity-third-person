﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MikesEnemyRouter : EnemyRouter {
    private static MikesEnemyRouter instance;
    private PatrolPoint[][] routes;

    // Construct hardcoded routes and store locally 
    private MikesEnemyRouter()
    {
        routes = new PatrolPoint[2][];
        routes[0] = new PatrolPoint[]
        {
            new PatrolPoint(new Vector3(0, 0, 0), new Vector3(0, 180, 0), 5.0f),
            new PatrolPoint(new Vector3(0, 0, -6), new Vector3(0, 180, 0), 2.0f),
            new PatrolPoint(new Vector3(6, 0, -6), new Vector3(0, 180, 0), 2.0f),
            new PatrolPoint(new Vector3(-8, 0, 0), new Vector3(0, 315, 0), 2.0f),
        };
        routes[1] = new PatrolPoint[]
        {
            new PatrolPoint(new Vector3(-8.27f, 0.98f, 8.27f), new Vector3(0, 90, 0), 2.0f),
            new PatrolPoint(new Vector3(-8.27f, 0.98f, -8.27f), new Vector3(0, 0, 0), 2.0f),
            new PatrolPoint(new Vector3(8.27f, 0.98f, -8.27f), new Vector3(0, 270, 0), 2.0f),
            new PatrolPoint(new Vector3(8.27f, 0.98f, 8.27f), new Vector3(0, 180, 0), 2.0f),
        };
    }

    // Access this singleton using the Instance property
    public static new EnemyRouter Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MikesEnemyRouter();
            }
            return Instance;
        }
        private set
        {
            instance = (MikesEnemyRouter)value;
        }
    }

    // Get the route for an enemy with the given ID
    public override PatrolPoint[] GetRoute(uint id)
    {
        return routes[id];
    }

    // Get the transform at the given index for an enemy with the given ID
    public override PatrolPoint GetPoint(uint id, uint index)
    {
        return routes[id][index];
    }
}
