using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseHitboxes : MonoBehaviour
{
    public List<HitboxTrigger> hitboxesToFuse = new List<HitboxTrigger>();

    List<Collider2D> blacklist = new List<Collider2D>();

    private void OnEnable()
    {
        for (int i = 0; i < hitboxesToFuse.Count; i++)
        {
            hitboxesToFuse[i].OnBeforeHit += BlacklistBehaviour;
        }
    }

    private void Update()
    {
        if (AllTriggersHaveBeenExited())
            ResetBlacklist();
    }

    void BlacklistBehaviour(Collider2D coll)
    {
        if (blacklist.Contains(coll))
        {
            for (int i = 0; i < hitboxesToFuse.Count; i++)
            {
                hitboxesToFuse[i].disableDamage = true;
            }
        }
        else
        {
            blacklist.Add(coll);
            for (int i = 0; i < hitboxesToFuse.Count; i++)
            {
                hitboxesToFuse[i].disableDamage = false;
            }
        }
    }

    void ResetBlacklist()
    {
        blacklist.Clear();
        for (int i = 0; i < hitboxesToFuse.Count; i++)
        {
            hitboxesToFuse[i].disableDamage = false;
        }

    }

    bool AllTriggersHaveBeenExited()
    {
        bool output = true;

        for (int i = 0; i < hitboxesToFuse.Count; i++)
        {
            if (hitboxesToFuse[i].triggerIsTriggered == true)
            {
                output = false;
                break;
            }
        }

        return output;
    }

    private void OnDisable()
    {
        for (int i = 0; i < hitboxesToFuse.Count; i++)
        {
            hitboxesToFuse[i].OnBeforeHit -= BlacklistBehaviour;
        }
    }
}
