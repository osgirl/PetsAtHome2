using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColour : MonoBehaviour
{
    public GameObject Player;
    public MeshRenderer Rend;
    public float close = 7.5f;
    private float invertFloat = 1f;
    private float scaleValue = 256f;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Rend = GetComponent<MeshRenderer>();
        Rend.material.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        // Get Player Transform/Position
        Transform playerTrans = Player.transform;
        Vector2 playerPos = new Vector2(playerTrans.position.x, playerTrans.position.z);

        // Get Pillar Transform/Position
        Transform trans = this.transform;
        Vector2 pos = new Vector2(trans.position.x, trans.position.z);

        // Compare the two distances
        var dist = Vector2.Distance(playerPos, pos);

        if (dist < close)
        {
            //Set the main Color of the Material to red
            Rend.material.SetColor("_Color", Color.HSVToRGB((Mathf.Atan2(transform.position.x * invertFloat, transform.position.z * invertFloat) + Mathf.PI) / (2 * Mathf.PI), 1, 1));
        }
    }
}
