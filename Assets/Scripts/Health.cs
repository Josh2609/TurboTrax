using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


public class Health : NetworkBehaviour {

    public const int maxHealth = 100;

    public static bool dead = false;

    [SyncVar(hook = "OnChangeHealth")]
    public int currentHealth = maxHealth;

    public RectTransform healthBar;

    public void TakeDamage(int amount)
    {
        if (!isServer)
            return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
            RpcRespawn();
        }
    }

    public void refillHealth()
    {
        if (!isServer)
            return;
        currentHealth = maxHealth;
    }

    void OnChangeHealth(int health)
    {
        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            NetworkCar car = gameObject.GetComponent<NetworkCar>();
            Transform checkpoint = (Transform)GameObject.Find(car.lastCheckpoint).GetComponent(typeof(Transform));
            transform.rotation = checkpoint.rotation;
            transform.position = new Vector3(checkpoint.transform.position.x, checkpoint.transform.position.y, 0);
        }
    }
    
}
