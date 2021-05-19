using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTrigger : MonoBehaviour
{
    public UnityEvent triggerEnter;
    public UnityEvent triggerStay;
    public UnityEvent triggerExit;

    public LayerMask layersToConsider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ShouldInvoke(collision))
        {
            print(collision.gameObject.name);
            triggerEnter.Invoke();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (ShouldInvoke(collision))
            triggerStay.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (ShouldInvoke(collision))
            triggerExit.Invoke();
    }

    bool ShouldInvoke(Collider2D collision)
    {
        if (collision.isTrigger == false && layersToConsider == (layersToConsider | (1 << collision.gameObject.layer)))
            return true;

        return false;
    }
}
