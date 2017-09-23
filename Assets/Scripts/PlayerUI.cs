using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {
    public PlayerController player;
    public Sprite Bullet;
    public GameObject Healthbar;
    public GameObject AmmoCounter;

    private const float fullHealthScale = 0.2f;

    private Text ammoCounterText;
    private float healthCache = -1;
    private int ammoCounterCache = -1;
    private int loadedCache = -1;

	// Use this for initialization
	void Start() 
    {
        ammoCounterText = AmmoCounter.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update() 
    {
        if (healthCache != player.CurrentHealth)
        {
            Vector3 scale = Healthbar.transform.localScale;
            scale.x = Mathf.Max((player.CurrentHealth / PlayerController.MaxHealth) * fullHealthScale, 0);
            Healthbar.transform.localScale = scale;
            healthCache = player.CurrentHealth;
        }
        if (ammoCounterCache != player.CurrentAmmo)
        {
            ammoCounterText.text = player.CurrentAmmo.ToString();
            ammoCounterCache = player.CurrentAmmo;
        }
        if (loadedCache != player.CurrentLoaded)
        {
            loadedCache = player.CurrentLoaded;
        }
	}
}
