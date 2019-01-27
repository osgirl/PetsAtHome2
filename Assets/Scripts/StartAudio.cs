using UnityEngine;

public class StartAudio : MonoBehaviour
{
    public GameObject audioPrefab;
    static Object instance;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = Instantiate(audioPrefab);
            DontDestroyOnLoad(instance);
            return;
        }
        Destroy(gameObject);
    }
}
