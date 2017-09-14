using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private GameObject player;
	// Use this for initialization
	void Start ()
    {
        player = GameObject.Find("Player");
	}
	
	// Update is called once per frame
	void Update ()
    {
        var position = player.transform.position;
        position.x -= 3;
        position.y += 3;
        position.z -= 3;
        transform.position = position;
	}
}
