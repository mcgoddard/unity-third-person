using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float Speed = 6f;              // The speed that the player will move at.
    private GameObject m_player;
    private float m_screenWidth;
    private float m_screenHeight;
    private GameObject[] m_roofs;

	// Use this for initialization
	void Start ()
    {
        m_player = GameObject.Find("Player");

        m_screenWidth = Screen.width / 2;
        m_screenHeight = Screen.height / 2;

        m_roofs = GameObject.FindGameObjectsWithTag("Roof");
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Move the camera (based on the players position)
        var position = m_player.transform.position;
        position.y += 4.9f;
        position.z -= 0.5f;
        transform.position = position;

        //Can the camera see the player
        RaycastHit hit;
        Transform cam = Camera.main.transform;
        var ray = new Ray(cam.position, cam.forward);

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

        if(Physics.Raycast(ray, out hit, 500, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
//            Debug.Log(hit.transform.tag);
            if(hit.transform.tag != "Player")
            {
                foreach(GameObject roof in m_roofs)
                {
                    SetTransparency(roof, 0.1f);
                }
            } else {
                foreach(GameObject roof in m_roofs)
                {
                    SetTransparency(roof, 1f);
                }
            }
        }
	}


    private void SetTransparency(GameObject g, float transparancy)
     {
         for (int i = 0; i < g.GetComponent<Renderer>().materials.Length; i++)
         {
             g.GetComponent<Renderer>().materials[i].shader = Shader.Find("Transparent/Diffuse");
             g.GetComponent<Renderer>().materials[i].SetColor("_Color", new Color(1, 1, 1, transparancy));
         }
         for (int i = 0; i < g.transform.childCount; i++)
             SetTransparency(g.transform.GetChild(i).gameObject, transparancy);
     }
}
