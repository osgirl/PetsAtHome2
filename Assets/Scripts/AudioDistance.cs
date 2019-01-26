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
        if (bird == null)
            bird = GameObject.Find("Bird");
        if (pdInstance == null)
            pdInstance = FindObjectOfType<LibPdInstance>();
        if (origin == null)
            origin = GameObject.Find("Cylinder").transform;
    }

    // Update is called once per frame
    public void Update()
    {
        distance = Vector3.Distance(origin.position, bird.transform.position);
        distanceValue = Mathf.InverseLerp(0, maxDistance, distance);
        pdInstance.SendFloat("distance", distanceValue);
    }
}
