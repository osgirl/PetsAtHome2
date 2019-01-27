using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class completionPoints : MonoBehaviour
{
    List<GameObject> Pillars;
    public LibPdInstance pdInstance;

    // Start is called before the first frame update
    void Start()
    {
        if (pdInstance == null)
            pdInstance = FindObjectOfType<LibPdInstance>();
    }

    // Update is called once per frame
    void Update()
    {
        Pillars = GameObject.FindGameObjectsWithTag("Pillar").ToList();
        int count = Pillars.Count();

        switch (count)
        {
            case 5:
                pdInstance.SendFloat("motifLayer1", 1);
                break;
            case 10:
                pdInstance.SendFloat("motifLayer2", 1);
                break;
            case 15:
                pdInstance.SendFloat("motifLayer3", 1);
                break;
            case 20:
                pdInstance.SendFloat("motifLayer4", 1);
                break;
            case 25:
                pdInstance.SendFloat("motifLayer5", 1);
                break;
            default:
                pdInstance.SendFloat("motifLayer0", 1);
                break;
        }
    }
}
