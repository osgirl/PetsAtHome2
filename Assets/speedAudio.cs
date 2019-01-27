using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedAudio : MonoBehaviour
{
	public GameObject bird;
	public LibPdInstance pdInstance;

	public float speed;

	// Start is called before the first frame update
	void Start()
	{
		if (pdInstance == null)
			pdInstance = FindObjectOfType<LibPdInstance>();
	}

	// Update is called once per frame
	public void Update()
	{
		if (bird == null)
			bird = GameObject.Find("Bird");

		if (bird == null)
			return;

		speed = BirdController.speedReference;
		pdInstance.SendFloat("movementSpeed", speed);
	}
}
