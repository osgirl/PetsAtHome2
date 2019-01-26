using UnityEngine;

public class PD_Distance : MonoBehaviour
{
    public LibPdInstance pdInstance;
   
    [Range(0, 1)]
    public float distance = 0;

    [Range(0, 1)]
    public float hue = 1;

    public bool motifToggle0;
    public bool motifToggle1;
    public bool motifToggle2;
    public bool motifToggle3;
    public bool motifToggle4;
    public bool motifToggle5;

    private void Start()
    {
        if (pdInstance == null)
            pdInstance = FindObjectOfType<LibPdInstance>();
    }

    public void Update()
    {
        pdInstance.SendFloat("distance", distance);
        pdInstance.SendFloat("hueAngle", hue);
        pdInstance.SendFloat("motifLayer0", motifToggle0 ? 1.0f : 0.0f);
        pdInstance.SendFloat("motifLayer1", motifToggle1 ? 1.0f : 0.0f);
        pdInstance.SendFloat("motifLayer2", motifToggle2 ? 1.0f : 0.0f);
        pdInstance.SendFloat("motifLayer3", motifToggle3 ? 1.0f : 0.0f);
        pdInstance.SendFloat("motifLayer4", motifToggle4 ? 1.0f : 0.0f);
        pdInstance.SendFloat("motifLayer5", motifToggle5 ? 1.0f : 0.0f);

    }
}
