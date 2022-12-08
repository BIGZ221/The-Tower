using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemySpawning : MonoBehaviour
{
    public MapGenerator mapGen;
    public GameObject en;
    public float spawnRate;
    float timeLastSpawn;

    void Update()
    {
        if (Time.fixedTime - spawnRate > timeLastSpawn) {
            Spawn();
        }
    }

    void Spawn(){
        List<MapGenerator.Coord> possiblePoints = mapGen.GetRegions(0)[0];
        int spawnIndex = Random.Range(0, possiblePoints.Count - 1);
        GameObject go = Instantiate(en) as GameObject;
        Vector3 spawnCoord = mapGen.CoordToWorldPoint(possiblePoints[spawnIndex]);
        spawnCoord.y = spawnCoord.z;
        spawnCoord.z = 0;
        go.transform.position = spawnCoord;
        timeLastSpawn = Time.fixedTime;
    }
}
