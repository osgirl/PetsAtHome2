using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HueAngleAudio : MonoBehaviour
{
    public GameObject bird;
    public LibPdInstance pdInstance;
    public Transform origin;

    public float angleVal;

    // Start is called before the first frame update
    void Start()
    {
        if (bird == null)
            bird = GameObject.Find("Cylinder");
        if (pdInstance == null)
            pdInstance = FindObjectOfType<LibPdInstance>();
        if (origin == null)
            origin = GameObject.Find("Bird").transform;
    }

    private void Update()
    {
        var direction = transform.InverseTransformDirection((origin.position - bird.transform.position).normalized);
        var azimuth = ((Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg) + 180.0f) / 360.0f;
        angleVal = azimuth;
        pdInstance.SendFloat("hueAngle", angleVal);
    }
}
