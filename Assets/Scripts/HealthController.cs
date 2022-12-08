using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{

    public float health;
    public float healRate;
    public float maxHealth;
    public float timeDelayToHeal;
    public bool shouldDestroy = true;

    private float timeLastDamaged;

    void Update() {
        if (health < 0 && shouldDestroy) {
            Destroy(gameObject);
        }
        if (Time.fixedTime - timeLastDamaged > timeDelayToHeal) {
            health = Mathf.Clamp(health + healRate * Time.deltaTime, 0, maxHealth);
        }
    }

    public void Damage(float dmg) {
        timeLastDamaged = Time.fixedTime;
        health -= dmg;
    }

}
