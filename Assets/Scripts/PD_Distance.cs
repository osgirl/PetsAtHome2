using UnityEngine;

public class PD_Distance : MonoBehaviour
{
    public LibPdInstance pdInstance;
   
    [Range(0, 1)]
    public float distance = 0;

    [Range(0, 1)]
    public float hue = 1;

    private void Start()
    {
        if (pdInstance == null)
            pdInstance = FindObjectOfType<LibPdInstance>();
    }

    public void Update()
    {
        pdInstance.SendFloat("distance", distance);
        pdInstance.SendFloat("hueAngle", hue);
    }
}
