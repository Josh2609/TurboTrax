using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour {

    NetworkCar car;
    public void manualStart()
    {
        Debug.Log("Print");
        car = gameObject.GetComponent<NetworkCar>();
        car.onPowerUpChange += changeImage;      
    }
    public Sprite bulletSprite;
    public Sprite mineSprite;
    public Sprite rocketSprite;
    public Sprite healthSprite;
    public Image powerUpUI;

    public void changeImage(int powerup)
    {
        Debug.Log("Worked");
        if(powerup == 0)
        {
            powerUpUI.sprite = bulletSprite;
        }
        else if (powerup == 1)
        {
            Debug.Log("plz");
            powerUpUI.sprite = mineSprite;
        }
        else if (powerup == 2)
        {
            powerUpUI.sprite = rocketSprite;
        }
        else if (powerup == 4)
        {
            powerUpUI.sprite = healthSprite;
        }
    }
}
