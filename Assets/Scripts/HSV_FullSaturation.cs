using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSV_FullSaturation : HSV_Base
{
    private Renderer rend;
    void Start(){
        rend = GetComponent<Renderer>();
    }
    void Update()
    {
        rend.material.SetColor("_Color",Color.HSVToRGB((Mathf.Atan2(transform.position.x * invertFloat, transform.position.z * invertFloat) + Mathf.PI)/(2*Mathf.PI),1,1));
    }
}
