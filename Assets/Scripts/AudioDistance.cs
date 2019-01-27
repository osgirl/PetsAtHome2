using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDistance : MonoBehaviour
{
    public GameObject bird;
    public LibPdInstance pdInstance;
    public Transform origin;

    public float maxDistance = 100;

    public float distance;
    public float distanceValue;

    // Start is called before the first frame update
    void Start()
    {
        if (pdInstance == null)
            pdInstance = FindObjectOfType<LibPdInstance>();
		maxDistance = GlobalSettings.worldSize/2;
    }

    // Update is called once per frame
    public void Update()
    {
        if (bird == null)
            bird = GameObject.Find("Bird");
        if (origin == null)
        {
            var go = GameObject.Find("Cylinder");
            if (go != null)
                origin = go.transform;
        }

        if (bird == null || origin == null)
            return;

        distance = Vector3.Distance(origin.position, bird.transform.position);
        distanceValue = Mathf.InverseLerp(0, maxDistance, distance);
        pdInstance.SendFloat("distance", distanceValue);
    }
}
