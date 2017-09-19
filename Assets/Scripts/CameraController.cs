using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private GameObject player;

    private float m_screenWidth;
    private float m_screenHeight;

    private GameObject[] m_roofs;

	// Use this for initialization
	void Start ()
    {
        player = GameObject.Find("Player");

        m_screenWidth = Screen.width / 2;
        m_screenHeight = Screen.height / 2;

        m_roofs = GameObject.FindGameObjectsWithTag("Roof");
	}
	
	// Update is called once per frame
	void Update ()
    {
        var position = player.transform.position;
        position.y += 5f;
        position.z -= 0.5f;
        transform.position = position;


        //Can the camera see the player
        RaycastHit hit;
        // Ray ray = Camera.main.ViewportPointToRay(new Vector3(m_screenWidth, m_screenHeight, 0));
        // Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        Transform cam = Camera.main.transform;

        if(Physics.Raycast(cam.position, cam.forward, out hit, 500))
        {
//            Debug.Log("Can see: " + hit.transform.tag);
            if(hit.transform.tag == "Roof")
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
        } else {
            Debug.Log("player blocked");
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
