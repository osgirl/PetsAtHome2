using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GetAmountPlayed : MonoBehaviour
{
    public float amountplayed;
    public Text amountplayedtext;
    // Start is called before the first frame update
    void Start()
    {
        amountplayed = PlayerPrefs.GetFloat("Played");
        amountplayedtext.text = amountplayed.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
