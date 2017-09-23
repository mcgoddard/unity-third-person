using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour {
    public bool Left = false;
    public bool Visible = true;

    private LineRenderer line;

	// Use this for initialization
	void Start () 
    {
        line = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown("l"))
        {
            Visible = !Visible;
        }
        if (Visible)
        {
            Vector3[] positions = new Vector3[line.positionCount];
            line.GetPositions(positions);
            var start = transform.position;
            var end = transform.position;
            start.y += 1.6f;
            end.y += 1.6f;
            if (Left)
            {
                end.x += 5;
            }
            else
            {
                end.x += -5;
            }
            end.z += 4;
            positions[0] = start;
            positions[1] = end;
            var direction = positions[1] - positions[0];
            direction = transform.parent.rotation * direction;
            positions[1] = direction + positions[0];
            line.SetPositions(positions);
        }
        else
        {
            Vector3[] positions = new Vector3[]
            {
                transform.position,
                transform.position
            };
            line.SetPositions(positions);
        }
	}

    public Vector3 GetConeEnd()
    {
        return line.GetPosition(1);
    }
}
