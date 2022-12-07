using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [System.Serializable]
    public struct ProjectileType {
        public int count;
        public int spread;
        public float speed;
        public float damage;
        public float delay;
        public GameObject sprite;
        [HideInInspector]
        public float lastShot;
    }

    public Transform body;
    public ProjectileType main;
    public ProjectileType secondary;
    public LayerMask layer;

    void Start() {
        main.lastShot = Mathf.NegativeInfinity;
        secondary.lastShot = Mathf.NegativeInfinity;
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            shootProjectile(ref main);
        }
        if (Input.GetMouseButtonDown(1)) {
            shootProjectile(ref secondary);
        }
    }

    void shootProjectile(ref ProjectileType type) {
        Debug.Log(Time.fixedTime - type.delay <= type.lastShot);
        if (Time.fixedTime - type.delay <= type.lastShot) return;
        Vector3 pos = body.position + body.transform.right.normalized * 2;
        pos.z = 0;
        float turnBound = type.spread / 2f;
        float turnIncrement = type.spread / (float) type.count;
        float total = 0;
        for (int i = 0; i < type.count; i++) {
            Vector3 rot = body.rotation.eulerAngles;
            rot.z += Mathf.LerpAngle(-turnBound, turnBound, total);
            GameObject go = Instantiate(type.sprite, pos, Quaternion.Euler(rot));
            Projectile projectile = go.GetComponent<Projectile>();
            Debug.Log(projectile);
            go.layer = layer;
            projectile.speed = type.speed;
            projectile.damage = type.damage;
            total += turnIncrement;
        }
        type.lastShot = Time.fixedTime;
    }

}
