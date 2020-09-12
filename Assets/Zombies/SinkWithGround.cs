using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkWithGround : MonoBehaviour
{
    float DestroyHeight;
    public float timeToSinkWithGround; //for sink time with ground
    // Start is called before the first frame update
    void Start()
    {
        //code for ragdoll only merge into ground
        if (this.gameObject.tag == "Ragdoll")
        {
            //invoking after 5 sec from death
            Invoke("SinkStart", 5);
        }

    }

    public void SinkStart()
    {
        //-5 for under the ground when it died
        //basically calculate height of zombie with terrain
        DestroyHeight = Terrain.activeTerrain.SampleHeight(this.transform.position) - 5;
        //collider used for destroying all collidle bodies with zombies.
        Collider[] col = this.transform.GetComponentsInChildren<Collider>();
        foreach (Collider c in col)
        {
            Destroy(c);
        }
        //invoking after 5 sec from death
        InvokeRepeating("ZombieSinkWithGround", timeToSinkWithGround, 0.3f);
    }
    void ZombieSinkWithGround()
    {
        //it is in loop ...call agian and again to decompost body
        this.transform.Translate(0, -0.001f, 0);
        if (this.transform.position.y < DestroyHeight)
        {
            Destroy(this.gameObject);
        }
    }
}
