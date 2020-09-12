using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class FPController : MonoBehaviour
{
    public GameObject cam;
    public GameObject steavePrefab;

    public Transform shootDirection;
    

    public Animator anim;

    //variable used for GUI Bar
    public Slider healthBar;
    public Text bulletReserv;
    public Text bulletInGun;

    //hit zmobie blood
    public GameObject bloodPrefab;

    //player got attack
    public GameObject gameOver;
    public GameObject uiBloodSlpatter;
    public GameObject canvas;
    float canvasWidth;
    float canvasHeight;


    public AudioSource[] footsteps;
    public AudioSource jump;
    public AudioSource land;
    public AudioSource ammoPickup;
    public AudioSource healthPickup;
    public AudioSource triggerSound;
    public AudioSource deathSound;
    public AudioSource reloadSound;

    float speed = 0.1f;
    float Xsensitivity = 1;
    float Ysensitivity = 1;
    float MinimumX = -90;
    float MaximumX = 90;
    Rigidbody rb;
    CapsuleCollider capsule;
    Quaternion cameraRot;
    Quaternion characterRot;

    bool cursorIsLocked = true;
    bool lockCursor = true;

    float x;
    float z;

    //Inventory
    int ammo = 50;
    int maxAmmo = 50;
    int health;
    int maxHealth = 100;
    int ammoClip = 10;
    int ammoClipMax = 10;

    bool playingWalking = false;
    bool previouslyGrounded = true;


    //Variables for 3 chance to player
    GameObject steve;
    public int livesCount = 3;
    int deadCount = 0;
    Vector3 startPositionOfPlayer;

    //variable for chekpoints
    public GameObject[] checkPoints;
    public CompassBar bCompass;
    int currentCheckpoint = 0;
    public LayerMask checkPointLayer;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        capsule = this.GetComponent<CapsuleCollider>();
        cameraRot = cam.transform.localRotation;
        characterRot = this.transform.localRotation;

        health = maxHealth;
        // used for GUI Bar
        healthBar.value = health;
        bulletReserv.text = ammo + "";
        bulletInGun.text = ammoClip + "";

        //to get canvas width and height for blood slpatter
        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.GetComponent<RectTransform>().rect.height;

        startPositionOfPlayer = this.transform.position;

        bCompass.target = checkPoints[0];
    }

    public void TakeHitAmount(float amount)
    {
        if(GameStats.gameOver)
        {
            return;
        }
        health = (int )Mathf.Clamp(health - amount, 0, maxHealth);
        // used for GUI Bar
        healthBar.value = health;

        //Hit player by zombie attack
        GameObject slptter = Instantiate(uiBloodSlpatter);
        slptter.transform.SetParent(canvas.transform);
        slptter.transform.position = new Vector3(Random.RandomRange(0, canvasWidth), Random.Range(0, canvasHeight), 0);
        Destroy(slptter, 2.2f);
        //player death code
        if (health<=0)
        {
            Vector3 pos = new Vector3(this.transform.position.x,
                                    Terrain.activeTerrain.SampleHeight(this.transform.position),
                                    this.transform.position.z);
            steve = Instantiate(steavePrefab, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Death");
            //player is died
            GameStats.gameOver = true;
            steve.GetComponent<AudioSource>().enabled = false;
            deadCount++;
            if(deadCount==livesCount)
            {
                Destroy(this.gameObject);
            }
            else
            {
                steve.GetComponent<SwitchMainMenu>().enabled = false;
                cam.SetActive(false);
                Invoke("RespawnPlayer", 5);
            }
        }
    }
    void RespawnPlayer()
    {
        Destroy(steve);
        cam.SetActive(true);
        GameStats.gameOver = false;
        health = maxHealth;
        healthBar.value = health;
        this.transform.position = startPositionOfPlayer;
    }
    //victory dance at Home
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Home")
        {
            Vector3 pos = new Vector3(this.transform.position.x,
                                   Terrain.activeTerrain.SampleHeight(this.transform.position),
                                   this.transform.position.z);
            GameObject steve = Instantiate(steavePrefab, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Dance");
            GameStats.gameOver = true;
            Destroy(this.gameObject);
            GameObject gameOverText = Instantiate(gameOver);
            gameOverText.transform.SetParent(canvas.transform);
            gameOverText.transform.localPosition =  Vector3.zero;
        }

        if(col.gameObject.tag == "CheckPoint")
        {
            startPositionOfPlayer = this.transform.position;

            if (col.gameObject == checkPoints[currentCheckpoint])
            {
                currentCheckpoint++;
                bCompass.target = checkPoints[currentCheckpoint];
            }
        }
    }


    void ProcessZombieHit()
    {
        //this code for aim at the zombie and shooting them with ray(Raycast) from gun
        RaycastHit hitInfo;
        if(Physics.Raycast(shootDirection.position,shootDirection.forward,out hitInfo, 500, ~checkPointLayer))
        {
            GameObject hitZombie = hitInfo.collider.gameObject;
            if(hitZombie.tag=="Zombie")
            {
                //hit zombie then blood sprinkles
                GameObject blood = Instantiate(bloodPrefab, hitInfo.point, Quaternion.identity);
                blood.transform.LookAt(this.transform.position); //Player chya position la blood udav 
                Destroy(blood, 0.5f);
                hitZombie.GetComponent<ZombieController>().shotsTaken++;
                if (hitZombie.GetComponent<ZombieController>().shotsTaken == hitZombie.GetComponent<ZombieController>().shotsRequried)
                {
                    if (Random.Range(0, 10) < 5)
                    {
                        //This is for ragdolls
                        GameObject rdPrefab = hitZombie.GetComponent<ZombieController>().isRagdollAttach;
                        GameObject newRd = Instantiate(rdPrefab, hitZombie.transform.position, hitZombie
                            .transform.rotation);
                        newRd.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(shootDirection.transform.forward * 10000);
                        Destroy(hitZombie);
                    }
                    else
                    {
                        //This is for Animator zombies
                        hitZombie.GetComponent<ZombieController>().KillZombie();
                    }
                }
            }

        }
    }

    
    // Update is called once per frame
    void Update()
    {
        UpdateCursorLock();

        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool("arm", !anim.GetBool("arm"));
    

        if (Input.GetMouseButtonDown(0) && !anim.GetBool("fire") && anim.GetBool("arm") && GameStats.canShoot)
        {
            if (ammoClip > 0)
            {
                anim.SetTrigger("fire");
                ProcessZombieHit();
                ammoClip--;
                bulletInGun.text = ammoClip + "";
                GameStats.canShoot = false;
            }
            else
                triggerSound.Play();


            Debug.Log("Ammo Left in Clip: " + ammoClip);
        }

        if (/*Input.GetKeyDown(KeyCode.R)&&*/ Input.GetMouseButtonDown(1) && anim.GetBool("arm"))
        {
            if (ammo > 0)
            {
                anim.SetTrigger("reload");
                reloadSound.Play();
                int amountNeeded = ammoClipMax - ammoClip;
                int ammoAvailable = amountNeeded < ammo ? amountNeeded : ammo;
                ammo -= ammoAvailable;
                ammoClip += ammoAvailable;
                bulletReserv.text = ammo + "";
                bulletInGun.text = ammoClip + "";
                Debug.Log("Ammo Left: " + ammo);
                Debug.Log("Ammo in Clip: " + ammoClip);
            }
            else
                triggerSound.Play();
        }

        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!anim.GetBool("walking"))
            {
                anim.SetBool("walking", true);
                InvokeRepeating("PlayFootStepAudio", 0, 0.4f);
            }
        }
        else if (anim.GetBool("walking"))
        {
            anim.SetBool("walking", false);
            CancelInvoke("PlayFootStepAudio");
            playingWalking = false;
        }

        bool grounded = IsGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(0, 300, 0);
            jump.Play();
            if (anim.GetBool("walking"))
            {
                CancelInvoke("PlayFootStepAudio");
                playingWalking = false;
            }
        }
        else if (!previouslyGrounded && grounded)
        {
            land.Play();
        }

        previouslyGrounded = grounded;

    }

    void PlayFootStepAudio()
    {
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, footsteps.Length);

        audioSource = footsteps[n];
        audioSource.Play();
        footsteps[n] = footsteps[0];
        footsteps[0] = audioSource;
        playingWalking = true;
    }


    void FixedUpdate()
    {
        float yRot = Input.GetAxis("Mouse X") * Ysensitivity;
        float xRot = Input.GetAxis("Mouse Y") * Xsensitivity;

        cameraRot *= Quaternion.Euler(-xRot, 0, 0);
        characterRot *= Quaternion.Euler(0, yRot, 0);

        cameraRot = ClampRotationAroundXAxis(cameraRot);

        this.transform.localRotation = characterRot;
        cam.transform.localRotation = cameraRot;

        x = Input.GetAxis("Horizontal") * speed;
        z = Input.GetAxis("Vertical") * speed;

        transform.position += cam.transform.forward * z + cam.transform.right * x; //new Vector3(x * speed, 0, z * speed);

        UpdateCursorLock();
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    bool IsGrounded()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo,
                (capsule.height / 2f) - capsule.radius + 0.1f))
        {
            return true;
        }
        return false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Ammo" && ammo < maxAmmo)
        {
            ammo = Mathf.Clamp(ammo + 10, 0, maxAmmo);
            bulletReserv.text = ammo + "";
            Debug.Log("Ammo: " + ammo);
            Destroy(col.gameObject);
            ammoPickup.Play();

        }
        else if (col.gameObject.tag == "MedKit" && health < maxHealth)
        {
            health = Mathf.Clamp(health + 25, 0, maxHealth);
            // used for GUI Bar
            healthBar.value = health;
            Debug.Log("MedKit: " + health);
            Destroy(col.gameObject);
            healthPickup.Play();
        }
        else if (col.gameObject.tag == "Lava")
        {
            health = Mathf.Clamp(health - 50, 0, maxHealth);
            Debug.Log("Health Level: " + health);
            if (health <= 0)
                deathSound.Play();
        }

        else if (IsGrounded())
        {
            if (anim.GetBool("walking") && !playingWalking)
            {
                InvokeRepeating("PlayFootStepAudio", 0, 0.4f);
            }
        }
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)
            InternalLockUpdate();
    }

    public void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            cursorIsLocked = false;
        else if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            cursorIsLocked = true;

        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

}
