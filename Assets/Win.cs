using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Win : MonoBehaviour
{
    public bool playing = true;
    public GameObject Bird;
    Color colorforbird;
    public GameObject explosioneffect;
    public GameObject amountofpillarsgameobject;
    int pillarsneeded = 0;
    bool running = true;
    public GameObject testegg;
    List<GameObject> Pillars = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        amountofpillarsgameobject = GameObject.Find("AmountOfPillarsNeeded");
        pillarsneeded = amountofpillarsgameobject.GetComponent<PillarsPublicAmountNeeded>().pillarsneededtowin;
        
    }

    // Update is called once per frame
    void Update()
    {
        colorforbird = Bird.GetComponent<BirdColour>().color;
        Pillars = GameObject.FindGameObjectsWithTag("Pillar").ToList();

        if (Pillars.Count() >= pillarsneeded)
        {
            playing = false;
            float time = gameObject.GetComponent<MeshRenderer>().material.GetFloat("_ScaleFloat");
            time += 30f * Time.deltaTime;
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_ScaleFloat", time);
            for(int i = 0; i < Pillars.Count(); i++){
                Pillars[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
            if(running == true){
            StartCoroutine(spawnegg());
            running = false;
            }
        }

    }
    private IEnumerator spawnegg(){
        
            
        yield return new WaitForSeconds(5f);
        Vector3 spawnpos = new Vector3(0,1.5f,0);
        GameObject egg = Instantiate(testegg, spawnpos, Quaternion.identity);
        egg.GetComponent<MeshRenderer>().material.color = colorforbird;
        Instantiate(explosioneffect, spawnpos, Quaternion.identity);
        float number = PlayerPrefs.GetFloat("Played");
        PlayerPrefs.SetFloat("Played", number + 1f); 
        
    }
}
