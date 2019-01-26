using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdColour : MonoBehaviour
{
    public GameObject[] feathers;
    public GameObject[] tagables;
  
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tagables = GameObject.FindGameObjectsWithTag("Pillar");
        if(tagables.Length >= 1){
            Color color = new Color(0, 0, 0);

           foreach (var tagable in tagables)
           {
              color += tagable.GetComponent<MeshRenderer>().material.color;
          }

            color = color / tagables.Length;
            for(int i = 0; i < feathers.Length; i++){
            feathers[i].GetComponent<SkinnedMeshRenderer>().material.color = color;
            }
        }
        
    }
}
