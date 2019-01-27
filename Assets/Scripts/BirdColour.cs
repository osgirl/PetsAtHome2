using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdColour : MonoBehaviour
{
    public GameObject[] feathers;
    public GameObject[] tagables;
    public Color color;
    public GameObject winobject;
    bool playing = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playing = winobject.GetComponent<Win>().playing;

        if(playing == true){
        tagables = GameObject.FindGameObjectsWithTag("Pillar");
        if(tagables.Length >= 1){
            Color color2 = new Color(0, 0, 0);

           foreach (var tagable in tagables)
           {
              color2 += tagable.GetComponent<MeshRenderer>().material.color;
          }

            color2 = color2 / tagables.Length;
            for(int i = 0; i < feathers.Length; i++){
            feathers[i].GetComponent<SkinnedMeshRenderer>().material.color = color2;
            }
            color = color2;
        }
        }
    }
}
