using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowOnCollision : MonoBehaviour
{
    public GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get Player Transform/Position
        Transform playerTrans = Player.transform;
        Vector2 playerPos = new Vector2(playerTrans.position.x, playerTrans.position.z);

        // Get Pillar Transform/Position
        Transform trans = transform;
        Vector2 pos = new Vector2(trans.position.x, trans.position.z);

        // Compare the two distances
        var dist = Vector2.Distance(playerPos, pos);

        if (dist < 0.2)
        {
            BirdController birdScript = Player.GetComponent<BirdController>();

            //birdScript.SetSpeed(birdScript.GetSpeed() - 10);
        }
    }
}
