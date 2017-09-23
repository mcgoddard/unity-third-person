using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour {
    public PlayerController player;
    public Sprite Bullet;
    public GameObject Healthbar;

    private const float fullHealthScale = 0.2f;

	// Use this for initialization
	void Start() 
    {
    	
	}
	
	// Update is called once per frame
	void Update() 
    {
        Vector3 scale = Healthbar.transform.localScale;
        scale.x = Mathf.Max((player.CurrentHealth / PlayerController.MaxHealth) * fullHealthScale, 0);
        Healthbar.transform.localScale = scale;
	}
}
