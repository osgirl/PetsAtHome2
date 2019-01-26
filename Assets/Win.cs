using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour
{

    List<GameObject> Pillars;

    // Start is called before the first frame update
    void Start()
    {
        Pillars = GameObject.FindGameObjectsWithTag("Pillar").ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
