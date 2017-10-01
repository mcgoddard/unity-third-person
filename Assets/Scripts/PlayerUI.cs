using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {
    public PlayerController player;
    public GameObject BulletPrefab;
    public GameObject Healthbar;
    public GameObject AmmoCounter;
    public GameObject LoadedPanel;

    private const float fullHealthScale = 0.2f;

    private Text ammoCounterText;
    private float healthCache = -1;
    private int ammoCounterCache = -1;
    private int loadedCache = -1;
    private GameObject[] bullets;

	// Use this for initialization
	void Start() 
    {
        ammoCounterText = AmmoCounter.GetComponent<Text>();
        bullets = new GameObject[PlayerController.MagazineCount];
        for (int i = 0; i < PlayerController.MagazineCount; i++)
        {
            Vector3 position = new Vector3((i * -20) - 10, 25);
            bullets[i] = GameObject.Instantiate(BulletPrefab);
            bullets[i].transform.SetParent(LoadedPanel.transform, false);
            bullets[i].transform.localPosition = position;
        }
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
            for (int i = 0; i < PlayerController.MagazineCount; i++)
            {
                if (i < loadedCache)
                {
                    bullets[i].SetActive(true);
                }
                else
                {
                    bullets[i].SetActive(false);
                }
            }
        }
	}
}
