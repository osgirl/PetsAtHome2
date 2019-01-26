using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdColour : MonoBehaviour
{
    public GameObject[] feathers;
    public GameObject[] tagable;
    Material mat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tagable = GameObject.FindGameObjectsWithTag("Pillar");
        if(tagable.Length >= 1){
            for(int i = 0; i < feathers.Length; i++){
            feathers[i].GetComponent<SkinnedMeshRenderer>().material.color = tagable[0].GetComponent<MeshRenderer>().material.color;
            }
        }
        
    }
}
