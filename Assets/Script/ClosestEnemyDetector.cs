using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestEnemyDetector : MonoBehaviour
{
    public List<GameObject> enemiesWithinRange = new List<GameObject>();

    public GameObject GetClosestEnemy()
    {
        if (enemiesWithinRange.Count == 0)
            return null;

        float minDistance = Vector2.Distance(this.transform.position, enemiesWithinRange[0].transform.position);
        GameObject output = enemiesWithinRange[0];
        for (int i = 0; i < enemiesWithinRange.Count; i++)
        {
            float distanceToCheck = Vector2.Distance(this.transform.position, enemiesWithinRange[i].transform.position);
            if (distanceToCheck < minDistance)
            {
                minDistance = distanceToCheck;
                output = enemiesWithinRange[i];
            }
        }

        return output;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (!enemiesWithinRange.Contains(collision.gameObject))
                enemiesWithinRange.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (enemiesWithinRange.Contains(collision.gameObject))
                enemiesWithinRange.Remove(collision.gameObject);
        }
    }
}
