using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour 
{

    void OnCollisionEnter2D(Collision2D collision)
    {
        var hit = collision.gameObject;
        var health = hit.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(100);
        }
        Destroy(gameObject);
    }
}
