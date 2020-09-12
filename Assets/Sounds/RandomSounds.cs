using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSounds : MonoBehaviour
{
    //variables
    public AudioSource sound;
    public bool Sound3d = true;
    public float firstPlay;
    public float randomSoundMin;
    public float randomSoundMax;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("PlaySounds", firstPlay);
    }

    void PlaySounds()
    {
        GameObject newSound = new GameObject();
        AudioSource newAS = newSound.AddComponent<AudioSource>();
        newAS.clip = sound.clip;
        if(Sound3d)
        {
            newAS.spatialBlend = 1.0f;
            newAS.maxDistance = sound.maxDistance;
            newSound.transform.SetParent(this.transform);
            newSound.transform.localPosition = Vector3.zero;
        }
        newAS.Play();
        Invoke("PlaySounds", Random.Range(randomSoundMin, randomSoundMax));
        Destroy(newSound, sound.clip.length);
    }
}
