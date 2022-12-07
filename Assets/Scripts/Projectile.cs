using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector]
    public float damage;
    [HideInInspector]
    public float speed;

    void FixedUpdate() {
        Vector3 forward = transform.right.normalized;
        forward.z = 0;
        transform.position += forward * speed;
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.GetComponent<Projectile>() != null) return;
        HealthController healthController = other.gameObject.GetComponent<HealthController>();
        if (healthController != null) {
            healthController.Damage(damage);
        }
        Destroy(gameObject);
    }

}
