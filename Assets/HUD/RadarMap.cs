using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This class contains actually radar objects in unity
//it take image and gameobject
public class RadarObject
{
    public Image icon { get; set; }
    public GameObject zombieOwner { get; set;}
}
public class RadarMap : MonoBehaviour
{
    public Transform PlayerPosition;
    //variable for how much scale of zombies see in map
    public float mapScale = 2.0f;

    //how much radar objects in map , count is store in list
    public static List<RadarObject> radObjects = new List<RadarObject>();

    public static void RegisterRadarObject(GameObject go,Image i)
    {
        //It iamge only in inspaector , does exists in radar 
        //Hance instantiate it here
        Image image = Instantiate(i);
        //register the object 
        radObjects.Add(new RadarObject()
        {
            zombieOwner = go,
            icon = image
        });
    }

    public static void DestroyRadarObjects(GameObject go)
    {
        //here we do make new list to store values of old list objects
        //except the zombie(object) that are we killed.
        //we can seperate that zombie from list only this way in c#
        List<RadarObject> newList = new List<RadarObject>();
        for(int i=0;i<radObjects.Count; i++)
        {
            if(radObjects[i].zombieOwner==go)
            {
                Destroy(radObjects[i].icon);
                continue;
            }
            else
            {
                newList.Add(radObjects[i]);
            }
        }
        //line maens remove old list
        radObjects.RemoveRange(0, radObjects.Count);
        //add new list except that zombie (object)which was killed
        radObjects.AddRange(newList);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //code for relative angle moving in radar map wrt to player 

        if(PlayerPosition==null)
        {
            return;
        }
        foreach(RadarObject ro in radObjects)
        {
            //calculate player position wrt zombie position
            Vector3 radarPosition = ro.zombieOwner.transform.position - PlayerPosition.transform.position;
            //distance ... last parameter is mapScale that what sort of distance zombies 
            //inside the map
            float distToObject = Vector3.Distance(PlayerPosition.position, ro.zombieOwner.transform.position) * mapScale;
            
            //calulate rotation angle
            float deltaY = Mathf.Atan2(radarPosition.x, radarPosition.z)
                            * Mathf.Rad2Deg - 180 - PlayerPosition.eulerAngles.y;
            radarPosition.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad) * -1.0f;
            //-1 for fliping , that basically it show forward dierction 
            //from the player
            radarPosition.z = distToObject * Mathf.Sin(deltaY * Mathf.Deg2Rad);

            //inside map it will shows zombie icons
            ro.icon.transform.SetParent(this.transform);
            //canvas is 2d and zombies are 3d , hence we  put
            //radarPosition.z + rt.pivot.y to get 2d values in canvas + position 
            //of radar itself
            RectTransform rt = this.GetComponent<RectTransform>();
            ro.icon.transform.position = new Vector3(radarPosition.x + rt.pivot.x,
                                                       radarPosition.z + rt.pivot.y,
                                                       0)+this.transform.position; 
            
            
        }
    }

}
