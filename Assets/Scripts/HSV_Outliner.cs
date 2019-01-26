using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSV_Outliner : HSV_Base
{
    private Outline outline;
    void Start()
    {
        outline = GetComponent<Outline>();
    }

    // Update is called once per frame
    void Update()
    {
        outline.OutlineColor = Color.HSVToRGB((Mathf.Atan2(transform.position.x * invertFloat, transform.position.z * invertFloat) + Mathf.PI)/(2*Mathf.PI),1,1);
    }
}
