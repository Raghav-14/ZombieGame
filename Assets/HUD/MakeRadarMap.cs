using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MakeRadarMap : MonoBehaviour
{
    public Image image;
    // Start is called before the first frame update
    void Start()
    {
        RadarMap.RegisterRadarObject(this.gameObject, image);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDestroy()
    {
        RadarMap.DestroyRadarObjects(this.gameObject);
    }
}
