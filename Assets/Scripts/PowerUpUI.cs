﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour {

    NetworkCar car;
    public void manualStart()
    {
        Debug.Log("Print");
        car = gameObject.GetComponent<NetworkCar>();
        car.onPowerUpChange += changePowerUpImage;
        car.onPowerUpTimerChange += changePowerUpTime;  
    }
    public Sprite bulletSprite;
    public Sprite mineSprite;
    public Sprite rocketSprite;
    public Sprite healthSprite;
    public Image powerUpUI;
    public Text powerUpTimer;

    public void changePowerUpTime(float timer)
    {
        if (timer <= 0.1f)
        {
            powerUpTimer.enabled = false;
        }
        else
        {
            powerUpTimer.enabled = true;
        }
        int rounded = (int) timer;
        powerUpTimer.text = rounded.ToString();
    }

    public void changePowerUpImage(int powerup)
    {
        if (powerup == -1)
        {
            powerUpUI.enabled = false;
        }
        else
        {
            powerUpUI.enabled = true;
        }
        if(powerup == 0)
        {
            powerUpUI.sprite = bulletSprite;
        }
        else if (powerup == 1)
        {
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