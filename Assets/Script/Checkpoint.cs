using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    bool isActivated;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
    {
        if (isActivated)
            return;

        isActivated = true;
        PauseManager.instance.SavePosition();
        CustomFunctions.PlaySound(CustomFunctions.instance.allCollectibleTiles[1].collectibleGetSound);
        GetComponent<SpriteRenderer>().color = Color.blue;
        PlayerHealth.instance.GetComponent<IDamageable>().Heal(2);
    }
}
