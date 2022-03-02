using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private List<Image> images;
    [SerializeField] private Color LifeColor;
    [SerializeField] private Color DeathColor;

    private int healthValue = 0;

    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }

    public void setHealth(int newHealth)
    {
        healthValue = newHealth;
        for (int i = 0; i < images.Count; i++)
        {
            if(i < healthValue)
            {
                images[i].color = LifeColor;
            } else
            {
                images[i].color = DeathColor;
            }
            
        }
    }
}
