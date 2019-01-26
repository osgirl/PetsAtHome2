using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Win : MonoBehaviour
{
    List<GameObject> Pillars = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Pillars = GameObject.FindGameObjectsWithTag("Pillar").ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if (Pillars.Count() > 30)
        {

        }
    }
}
