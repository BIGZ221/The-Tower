using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float speedModifier;

    public GameObject body;

    void FixedUpdate()
    {
        makeMove();
    }

    void makeMove() {
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * speedModifier;
        dir *= Time.deltaTime;
        transform.Translate(dir);
        lookAtMouse();
    }

    void lookAtMouse() {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5.23f;
        Vector3 objectPos = Camera.main.WorldToScreenPoint (body.transform.position);
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;
        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        body.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    public void Damage(float amount) {

    }
}
