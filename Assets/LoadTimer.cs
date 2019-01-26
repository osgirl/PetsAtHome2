using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LoadTimer : MonoBehaviour
{
    public float score;
    public Text scoretext;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        score = PlayerPrefs.GetFloat("timescore");
        score = Mathf.RoundToInt(score);
        scoretext.text = score.ToString();
    }
}
