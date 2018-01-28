﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{

    Gamemode controller;
    bool solved, rotated;
    float speed;
	List<SemaphoreGestureTarget> flagsRequired;
    GameObject shipEater;

	private Sprite speechBubbleWhite;
	private Sprite speechBubbleGreen;

	private SpriteRenderer iconRenderer;
	private SpriteRenderer speechBubbleRenderer;

    // Use this for initialization
    void Start()
    {
        solved = false;
        rotated = false;
        controller = GameObject.Find("DefaultGamemode").GetComponent<Gamemode>();
		speechBubbleWhite = Resources.Load<Sprite> ("SpeechBubble");
		speechBubbleGreen = Resources.Load<Sprite> ("SpeechBubbleGreen");
		iconRenderer = this.transform.Find ("Icon").GetComponent<SpriteRenderer> ();
		speechBubbleRenderer = this.transform.Find ("SpeechBubble").GetComponent<SpriteRenderer> ();
    }

    // Update is called once per frame
    void Update()
    {
        //rotate the ship by 45 degrees if the puzzle for it has been solved
        if (solved == true && rotated == false) {
            rotated = true;
            StartCoroutine(DescendMe(Vector3.right * 5, 6f));
        } else {
            //move the boat by a step
            float step = speed * Time.deltaTime;
            transform.position += transform.forward * step;

            //check for gameover
            if (this.gameObject.GetComponent<Collider>().bounds.Intersects(shipEater.GetComponent<Collider>().bounds)) {
                Destroy(this.gameObject);
                controller.gameOver();
            }
        }
    }

    //rotate the ship by "byAngles" degrees in "inTime" seconds
    IEnumerator DescendMe(Vector3 byAngle, float inTime)
    {
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + byAngle);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime) {
            transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            transform.position += (transform.forward * 0.01f);
            transform.position -= (transform.up * 0.005f);
            yield return null;
        }
        Destroy(this.gameObject);
    }

	public void Initialize(float newSpeed, Transform destination, List<SemaphoreGestureTarget> requiredFlags)
    {
		flagsRequired = requiredFlags;
        speed = newSpeed;
        transform.LookAt(destination);
        shipEater = GameObject.Find("ShipEater");
        UpdateFlag();
    }


	public bool ReceiveGesture(SemaphoreGesture sg) {
		if (sg.Equals (flagsRequired[0])) {
			ResolveGesture ();
		}

		return solved;
	}

	IEnumerator UpdateFlag()
    {
		speechBubbleRenderer.sprite = speechBubbleGreen;
		WaitForSeconds (0.5f);

		iconRenderer.sprite = flagsRequired[0].GetIcon();
		speechBubbleRenderer.sprite = speechBubbleWhite;
		yield return null;

    }

    void ResolveGesture()
    {
        //Remove the head of the required flags
        flagsRequired.RemoveAt(0);

        //If there's no flags left then you're solved
		if (flagsRequired.Count == 0) {
			solved = true;
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                sr.enabled = false;
            }
            controller.IncrementScore();
		} else {
			StartCoroutine(UpdateFlag ());
		}
    }


}
