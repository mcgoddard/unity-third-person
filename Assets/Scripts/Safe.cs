using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Safe : MonoBehaviour {
    private enum State 
    {
        Closed,
        Opening,
        Open,
        Empty
    }

    private GameObject door;
    private GameObject doorLock;
    private GameObject goldPile;
    private State currentState = State.Closed;
    private AudioSource audioSource;

    private static Quaternion unlockedRotation = Quaternion.Euler(new Vector3(180, 0, 0));
    private static Quaternion openedRotation = Quaternion.Euler(new Vector3(0, 0, 270));
    private static float unlockTime = 0.05f;

	// Use this for initialization
	void Start() 
    {
        door = transform.Find("Door Hinge").gameObject;
        doorLock = door.transform.Find("Safe Door").Find("Lock").gameObject;
        goldPile = transform.Find("Gold Pile").gameObject;
        audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update() 
    {
        if (currentState == State.Opening)
        {
            if (Quaternion.Angle(doorLock.transform.localRotation, unlockedRotation) > 1)
            {
                Quaternion rotation = doorLock.transform.localRotation;
                Quaternion newRotation = Quaternion.Lerp(rotation, unlockedRotation, unlockTime);
                doorLock.transform.localRotation = newRotation;
            }
            else if (Quaternion.Angle(door.transform.localRotation, openedRotation) > 1)
            {
                Quaternion rotation = door.transform.localRotation;
                Quaternion newRotation = Quaternion.Lerp(rotation, openedRotation, unlockTime);
                door.transform.localRotation = newRotation;
            }   
            else
            {
                currentState = State.Open;
            }
        }
        else if (currentState == State.Empty && goldPile.activeSelf)
        {
            goldPile.SetActive(false);
        }
	}

    public bool CanOpen()
    {
        return currentState == State.Closed;
    }

    public void Open()
    {
        currentState = State.Opening;
        audioSource.Play();
    }

    public bool CanSteal()
    {
        return currentState == State.Open;
    }

    public void Steal()
    {
        if (currentState == State.Open)
        {
            currentState = State.Empty;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}
