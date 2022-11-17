using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyMovement : MonoBehaviour
{
    public float speed = 4f;

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.Find("Player");
        Vector3 dir = Vector3.Normalize(player.transform.position - transform.position) * speed;
        dir *= Time.deltaTime;
        float dist = Vector3.Distance(player.transform.position, transform.position);
        if(dist <= 7f){
            transform.Translate(dir);
        }
    }
}
