using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public GameObject characterPlayer;//Hero

    public GameObject isRagdollAttach;//Ragdoll

    //This 2 variables to change value from Inspector beq every agent has its differnt speed
    //of walking and running
    //More naturally looking game 
    public float walkingSpeed;
    public float runningSpeed;
    Animator anim;
    NavMeshAgent agent;

    //sound for damage
    public AudioSource[] damageSound;
    //damage amount
    public float damageAmount;
    //State machine for changing states
    enum STATE { IDLE, WANDER, ATTACK, CHASE, DEAD };

    STATE state = STATE.IDLE;
    // Start is called before the first frame update

    //more than 1 shot zombie take
    public int shotsRequried = 1;
    public int shotsTaken;
    void Start()
    {
        anim = this.GetComponent<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
    }

    //switch off all the state
    void TurnOffTriggers()
    {
        anim.SetBool("isWalking", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isDead", false);
    }

    float DistanceFromPlayer()
    {
        //when player is died then forgot distance from player,it set to infinity
        if (GameStats.gameOver)
        {
            return Mathf.Infinity;
        }
        //otherwise calculate distance from player
        return Vector3.Distance(characterPlayer.transform.position, this.transform.position);
    }
    bool CanSeePlayer()
    {
        if (DistanceFromPlayer() < 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool ForgotPlayerPath()
    {
        if (DistanceFromPlayer() > 20)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void KillZombie()
    {
        //code for dead animation by trigger
        TurnOffTriggers();
        anim.SetBool("isDead", true);
        state = STATE.DEAD;
    }

    public void DamagePlayer()
    {
        if (characterPlayer != null)
        {
            characterPlayer.GetComponent<FPController>().TakeHitAmount(damageAmount);
            DamageSound();
        }
    }

    //play sound for damage
    void DamageSound()
    {
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, damageSound.Length);

        audioSource = damageSound[n];
        audioSource.Play();
        damageSound[n] = damageSound[0];
        damageSound[0] = audioSource;
    }
    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.R))
        //{
        //    if (Random.Range(0, 10) < 5)
        //    {
        //        //code for Ragdolls to die
        //        GameObject rd = Instantiate(isRagdollAttach, this.transform.position, this.transform.rotation);
        //        rd.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 10000);
        //        Destroy(this.gameObject);
        //    }
        //    else
        //    {
        //        //code for dead animation by trigger
        //        TurnOffTriggers();
        //        anim.SetBool("isDead", true);
        //        state = STATE.DEAD;
        //    }
        //    return;
        //}
        //cant find player then come here and find with tag
        if (characterPlayer == null && GameStats.gameOver == false)
        {
            characterPlayer = GameObject.FindWithTag("Player");
            return;
        }
        switch (state)
        {
            case STATE.IDLE:
                if (CanSeePlayer())
                {
                    state = STATE.CHASE;
                }
                else if (Random.Range(0, 1000) < 5)
                {
                    //There is only 5 chance in 1000 to get wander state 
                    //This code for more realstick , to get stand and walk effect naturally
                    state = STATE.WANDER;
                }
                break;
            //ikde tikde Bhtkane  
            case STATE.WANDER:
                if (!agent.hasPath)
                {
                    //This are random value where zomie is wander in any direction
                    float newX = this.transform.position.x + Random.Range(-5, 5);
                    float newZ = this.transform.position.z + Random.Range(-5, 5);
                    //y calculation for zombie with using terrain sampleheight value
                    float newY = Terrain.activeTerrain.SampleHeight(new Vector3(newX, 0, newZ));
                    Vector3 dest = new Vector3(newX, newY, newZ);
                    agent.SetDestination(dest);
                    agent.stoppingDistance = 0;
                    TurnOffTriggers();
                    agent.speed = walkingSpeed;
                    anim.SetBool("isWalking", true);
                }
                if (CanSeePlayer())
                {
                    state = STATE.CHASE;
                }
                else if (Random.Range(0, 1000) < 5)
                {
                    //There is only 5 chance in 1000 to get idel state 
                    //This code for more realstick , to get stand and walk effect naturally
                    state = STATE.IDLE;
                    TurnOffTriggers();
                    //Dont want to walk hence reset path
                    agent.ResetPath();
                }
                break;
            //Patlag karne
            case STATE.CHASE:
                //When player died , zombies back to wander state
                if(GameStats.gameOver)
                {
                    TurnOffTriggers();
                    state= STATE.WANDER;
                    return;
                }
                agent.SetDestination(characterPlayer.transform.position);
                agent.stoppingDistance = 5;
                TurnOffTriggers();
                anim.SetBool("isRunning", true);
                agent.speed = runningSpeed;
                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    state = STATE.ATTACK;
                }
                if (ForgotPlayerPath())
                {
                    state = STATE.WANDER;
                    //if player is far away from zombie ,then zombie must be forgot player and 
                    //reset the path
                    agent.ResetPath();
                }
                break;
            case STATE.ATTACK:
                //When player died , zombies back to wander state
                if (GameStats.gameOver)
                {
                    TurnOffTriggers();
                    state = STATE.WANDER;
                    return;
                }
                TurnOffTriggers();
                anim.SetBool("isAttacking", true);
                this.transform.LookAt(characterPlayer.transform.position);

                if (DistanceFromPlayer() > agent.stoppingDistance + 2)
                {
                    state = STATE.CHASE;
                }
                break;
            case STATE.DEAD:
                Destroy(agent);
                AudioSource[] sounds = this.GetComponents<AudioSource>();
                foreach(AudioSource s in sounds)
                {
                    s.volume = 0;
                }

                this.gameObject.GetComponent<SinkWithGround>().SinkStart();
                break;
        }
    }
}
