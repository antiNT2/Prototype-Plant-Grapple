using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f;
    public UnityEvent onTrigger;
    public bool explodeOnTrigger;
    public LayerMask layersToCollideWith;

    public float Speed { get => speed; set => speed = value; }

    private void Update()
    {
        transform.Translate(Vector2.right * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (layersToCollideWith == (layersToCollideWith | (1 << collision.gameObject.layer)))
        {
            onTrigger.Invoke();

            if (explodeOnTrigger)
            {
                speed = 0;
                GetComponent<Animator>()?.Play("Explode");
                Destroy(this.gameObject, .5f);
            }
        }
    }
}
