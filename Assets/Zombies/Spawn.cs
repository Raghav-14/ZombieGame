using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawn : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ZombiePreFab;
    public int number;
    public float SpawnRadious;
    public bool isTriggerSpawnStart = true;
    void Start()
    {
        if (isTriggerSpawnStart)
        {
            SpawnAll();
        }
    }
    void SpawnAll()
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 randomPoint = this.transform.position + Random.insideUnitSphere * SpawnRadious;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
            {
                Instantiate(ZombiePreFab, hit.position, Quaternion.identity);
            }
            else
            {
                i--;
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!isTriggerSpawnStart && col.gameObject.tag == "Player")
        {
            SpawnAll();
        }
    }
}
