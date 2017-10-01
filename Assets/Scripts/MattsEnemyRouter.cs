using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MattsEnemyRouter : EnemyRouter {
    private static MattsEnemyRouter instance;
    private PatrolPoint[][] routes;

    // Construct hardcoded routes and store locally 
    private MattsEnemyRouter()
    {        
        routes = new PatrolPoint[2][];
        routes[0] = new PatrolPoint[]
        {
            new PatrolPoint(new Vector3(-2.52f, 0.95f, -0.41f), new Vector3(0, 180, 0), 2.0f),
            new PatrolPoint(new Vector3(7, 0.95f, -0.41f), new Vector3(0, 90, 0), 2.0f),
            //new PatrolPoint(new Vector3(7, 0.95f, -6), new Vector3(0, 180, 0), 2.0f),
            //new PatrolPoint(new Vector3(2.4f, 0.95f, -6), new Vector3(0, 0, 0), 2.0f),
            new PatrolPoint(new Vector3(2.4f, 0.95f, -0.41f), new Vector3(0, 270, 0), 2.0f),
        };
        routes[1] = new PatrolPoint[]
        {
            new PatrolPoint(new Vector3(-4.16f, 0.955f, 4.64f), new Vector3(0, 90, 0), 5.0f),
            new PatrolPoint(new Vector3(7, 0.955f, 4.64f), new Vector3(0, 90, 0), 2.0f),
            new PatrolPoint(new Vector3(-4.16f, 0.955f, 4.64f), new Vector3(0, 270, 0), 2.0f),
        };
    }

    // Access this singleton using the Instance property
    public static new EnemyRouter Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MattsEnemyRouter();
            }
            return Instance;
        }
        private set
        {
            instance = (MattsEnemyRouter)value;
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
