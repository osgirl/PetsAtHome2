using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour
{
    public LibPdInstance pdInstance;

    // Start is called before the first frame update
    void Start()
    {
        if (pdInstance == null)
            pdInstance = FindObjectOfType<LibPdInstance>();
    }

    public void LoadScene()
    {
        pdInstance.SendFloat("introStart", 1);
        SceneManager.LoadScene("Main");
    }
}
