using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemySpawning : MonoBehaviour
{
    public GameObject en;
    // Start is called before the first frame update
    void Start()
    {
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {
         
    }

    void Spawn(){
        GameObject go = Instantiate(en) as GameObject;
        go.transform.position = new Vector3(Random.Range(-40f, 40f), Random.Range(-40f, 40f), 0f);
    }
}
