using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTime : MonoBehaviour
{
    public float timescore = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timescore += Time.deltaTime;
        //Debug.Log(timescore.ToString());
        PlayerPrefs.SetFloat("timescore", timescore);
        PlayerPrefs.Save();
    }
}
