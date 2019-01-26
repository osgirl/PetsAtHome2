﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Win : MonoBehaviour
{
    bool running = true;
    public GameObject testegg;
    List<GameObject> Pillars = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Pillars = GameObject.FindGameObjectsWithTag("Pillar").ToList();

        if (Pillars.Count() >= 2)
        {
            float time = gameObject.GetComponent<MeshRenderer>().material.GetFloat("_ScaleFloat");
            time += 30f * Time.deltaTime;
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_ScaleFloat", time);
            for(int i = 0; i < Pillars.Count(); i++){
                Pillars[0].GetComponent<MeshRenderer>().material.color = Color.grey;
            }
            if(running == true){
            StartCoroutine(spawnegg());
            running = false;
            }
        }
    }
    private IEnumerator spawnegg(){
        
            
        yield return new WaitForSeconds(5f);
        Vector3 spawnpos = new Vector3(0,2.5f,0);
        Instantiate(testegg, spawnpos, Quaternion.identity);
        
        
    }
}
