using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EndScreenUI : MonoBehaviour
{
    public LibPdInstance pdInstance;

    // Start is called before the first frame update
    void Start()
    {
        if (pdInstance == null)
            pdInstance = FindObjectOfType<LibPdInstance>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void play(){
        SceneManager.LoadScene("Main");
    }
    public void quit(){
        pdInstance.SendFloat("pdReset", 1);
        SceneManager.LoadScene("Menu 3D");
    }
}
