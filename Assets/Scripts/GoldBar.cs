using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GoldBar : MonoBehaviour {

	// Use this for initialization
	void Start() 
    {
		
	}
	
	// Update is called once per frame
	void Update() 
    {
		
	}

    public bool CanSteal()
    {
        return gameObject.activeSelf;
    }

    public void Steal()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}
