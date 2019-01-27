using UnityEngine;

public class StartAudio : MonoBehaviour
{
    public GameObject audioPrefab;

    // Start is called before the first frame update
    void Start()
    {
        var instance = Instantiate(audioPrefab);
        DontDestroyOnLoad(instance);
    }
}
