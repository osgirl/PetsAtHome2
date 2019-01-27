using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdPushBack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision other){
       
        
             Debug.Log("ENTERTRIGGER");
            this.gameObject.transform.Translate(Vector3.back * 10);
        
    }
}
