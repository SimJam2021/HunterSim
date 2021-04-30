using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deerController : MonoBehaviour
{
    // Deer Behaviour
    public float walkingSpeed = 1f;
    public float runningSpeed = 15f;
    public float visionCone = 40;
    public float visionRange = 100;
    public float hearingRange = 30;
    public float hearingSpeed = 10;
    public float checkIfSeen = 3.0f;
    private float checkIfSeenCounter;
    public float hotZone = 20;
    public float lukeWarmZone = 40;
    public float coolZone = 60;
    public float seenDistance = 30;
    private bool playerSeen = false;
    public float checkIfHeard = 0.5f;
    private float checkIfHeardCounter;
    private float probabilityHeard;
    public float veryCloseDist = 10;
    public float closeDist = 20;
    public float farDist = 30;
    public float minVeryCloseHeardProb = 0.6f;
    public float veryCloseHeardProbHeadroom = 0.4f;
    public float minCloseHeardProb = 0.35f;
    public float closeHeardProbHeadroom = 0.3f;
    public float minFarHeardProb = 0.0f;
    public float farHeardProbHeadroom = 0.3f;
    private bool playerHeard = false;
    private Vector3 newGrazePos;
    private float moveToNewGrazeTimer;
    private float acceptableRangeForArrival = 0.5f;
    public float newGrazeDistMin = -10;
    public float newGrazeDistMax = 10;
    public float runAwayDist = 100;
    private int leftRightRandVar;
    public float heardBufferCounterParm = 15;
    private float heardBufferCounter;
    private bool heardBuffer = false;

    //Moving on terain
    public float height = 0.5f;
    public float heightPadding = 0.05f;
    public LayerMask ground;
    public float maxGroundAngle = 40;
    public bool debug;
    private float groundAngle;
    private Vector3 forward;
    private RaycastHit hitInfo;
    private bool grounded;
    

    // Deer States
    private bool deerGrazing;
    private bool deerWalking;


    //Relationship to Player
    private float angleToPlayer;
    private Vector3 playerPosition;
    private Vector3 targetDir;
    private float distanceToPlayer;
    private Vector3 lastPosition;
    private float playerSpeed;
    private float deerSight;
    private float probSeen;
    public float playerMaxSpeed = 0.064f;
    private float howFastPlayer;

    //General
    private double randSeenVar;
    private double randHeardVar;

    // Start is called before the first frame update
    void Start()
    {
        lastPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        deerGrazing = true;
        moveToNewGrazeTimer = Random.Range(5.0f, 10.0f);
        Debug.Log("Deer is currently grazing");
        Debug.Log("New graze timer : " + moveToNewGrazeTimer);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // GET RELATIONAL VARIABLES
        // Get current position of player
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        // Get direction between player and deer
        targetDir = playerPosition - transform.position;
        // Get angle between deer "forward" vector and angle to player
        angleToPlayer = Vector3.Angle(targetDir,transform.forward);
        // Get distance to player
        distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        // Get Player Speed
        playerSpeed = (playerPosition - lastPosition).magnitude;
        lastPosition = playerPosition;

        if (groundAngle >= maxGroundAngle)
        {
            transform.Rotate(0, Random.Range(150, 210), 0);
        }

        // Control Angle of Deer on Terrain

        //CalculateForward
        if (!grounded)
        {
            forward = transform.forward;
        }
        else
        {
            forward = Vector3.Cross(transform.right, hitInfo.normal);
        }

        //Calculate Ground Angle
        if (!grounded)
        {
            groundAngle = 90;
        }
        else
        {
            groundAngle = Vector3.Angle(hitInfo.normal, transform.forward);
        }

        //Check Ground

        if (Physics.Raycast(transform.position,-Vector3.up,out hitInfo,height + heightPadding,ground))
        {
            if (Vector3.Distance(transform.position,hitInfo.point) < height)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.up * height, 5 * Time.deltaTime);
            }
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        //Apply Gravity
        if(!grounded)
        {
            transform.position += Physics.gravity * Time.deltaTime;
        }

        Debug.Log("Grounded : " + grounded);
        Debug.Log("Ground angle : " + groundAngle);





        // Graze Behaviour

        if (deerGrazing)
        {
            moveToNewGrazeTimer -= 1 * Time.deltaTime;

            if (moveToNewGrazeTimer <= 0)
            {
                //Debug.Log("Deer moving to new grazing spot");
                newGrazePos = new Vector3(transform.position.x + Random.Range(newGrazeDistMin, newGrazeDistMax), transform.position.y, transform.position.z + Random.Range(newGrazeDistMin, newGrazeDistMax));
                moveToNewGrazeTimer = Random.Range(5.0f, 10.0f);
                transform.LookAt(newGrazePos);
                deerGrazing = false;
                deerWalking = true;
            }
        }

        // walking behaviour
        if (deerWalking)
        {
            transform.position += transform.forward * Time.deltaTime * walkingSpeed;
            if ((transform.position - newGrazePos).magnitude < acceptableRangeForArrival)
            {
                //Debug.Log("Arrived at new grazing spot");
                deerGrazing = true;
                deerWalking = false;
            }
        }

        // seen behaviour

        if (playerSeen)
        {
            transform.LookAt(playerPosition);
            transform.Rotate(0, 200, 0);
            transform.position += transform.forward * Time.deltaTime * runningSpeed;
            if (distanceToPlayer >= runAwayDist)
            {
                //Debug.Log("Deer has found safe spot");
                deerGrazing = true;
                //Debug.Log("Deer is now grazing");
                moveToNewGrazeTimer = Random.Range(5.0f, 10.0f);
                playerSeen = false;
            }

        }

        // Heard behavious

        if(playerHeard)
        {
            transform.LookAt(playerPosition);
            // then turn either a bit left or right so deer isn't always looking directly at player
            leftRightRandVar = Random.Range(1, 3);
            if (leftRightRandVar == 1)
            {
                transform.Rotate(0, Random.Range(270, 360), 0);
            }
            else
            {
                transform.Rotate(0, Random.Range(0, 90), 0);
            }
            playerHeard = false;
            heardBuffer = true;
            heardBufferCounter = heardBufferCounterParm;
        }

        // CHECK IF SEEN
        // Every few second deer checks if it can see the player
        checkIfSeenCounter -= 1 * Time.deltaTime;

        if(checkIfSeenCounter <= 0)
        {
            // Check which zone player is in
            
            //Debug.Log("SEEN STATS");
            //Debug.Log("Angle to deer : " + angleToPlayer);
            //Debug.Log("Distance to deer : " + distanceToPlayer);

            // Hot Zone
            if (angleToPlayer <= hotZone)
            {
                //Debug.Log("---- You're in the hot zone ----");
                deerSight = 1 - (distanceToPlayer / seenDistance);
                //Debug.Log("Deer sight : " + deerSight);
                if (deerSight <= 1)
                {
                    probSeen = 0.75f + (0.25f * deerSight);
                    randSeenVar = Random.Range(0.0F, 1.0F);
                    //Debug.Log("Probability of being seen : " + probSeen);
                    //Debug.Log("Random seen variable : " + randSeenVar);
                    if (randSeenVar < probSeen)
                    {
                        playerSeen = true;
                        deerGrazing = false;
                        deerWalking = false;
                        //Debug.Log("You've been spotted - deer is running away");

                    }
                }
            }
            // Luke Warm Zone
            else if (angleToPlayer <= lukeWarmZone)
            {
                //Debug.Log("---- You're in the luke warm zone ----");
                deerSight = 1 - (distanceToPlayer / seenDistance);
                //Debug.Log("Deer sight : " + deerSight);
                if (deerSight <= 1)
                {
                    probSeen = 0.5f + (0.25f * deerSight);
                    randSeenVar = Random.Range(0.0F, 1.0F);
                    //Debug.Log("Probability of being seen : " + probSeen);
                    //Debug.Log("Random seen variable : " + randSeenVar);
                    if (randSeenVar < probSeen)
                    {
                        playerSeen = true;
                        deerGrazing = false;
                        deerWalking = false;
                        //Debug.Log("You've been spotted - deer is running away");
                    }
                }
            }

            // Cool Zone
            else if (angleToPlayer <= coolZone)
            {
                //Debug.Log("---- You're in the cool zone ----");
                deerSight = 1 - (distanceToPlayer / seenDistance);
                //Debug.Log("Deer sight : " + deerSight);
                if (deerSight <= 1)
                {
                    probSeen = 0.25f + (0.25f * deerSight);
                    randSeenVar = Random.Range(0.0F, 1.0F);
                    //Debug.Log("Probability of being seen : " + probSeen);
                    //Debug.Log("Random seen variable : " + randSeenVar);
                    if (randSeenVar < probSeen)
                    {
                        playerSeen = true;
                        deerGrazing = false;
                        deerWalking = false;
                        //Debug.Log("You've been spotted - deer is running away");
                    }
                }
            }

            checkIfSeenCounter = checkIfSeen;
        }


        // CHECK IF HEARD
        // Every second check if player can be heard
        // Every few second deer checks if it can see the player

        if (!heardBuffer)
        {
            checkIfHeardCounter -= 1 * Time.deltaTime;

            if (checkIfHeardCounter <= 0)
            {
                howFastPlayer = playerSpeed / playerMaxSpeed;

                //Debug.Log("HEARING STATS");
                //Debug.Log("How fast player : " + howFastPlayer);

                // PLayer very close
                if (distanceToPlayer <= veryCloseDist)
                {
                    //Debug.Log("---- You are very close to the deer ----");
                    if (howFastPlayer > 0.001)
                    {
                        probabilityHeard = minVeryCloseHeardProb + (howFastPlayer * veryCloseHeardProbHeadroom);
                        randHeardVar = Random.Range(0.0F, 1.0F);
                        //Debug.Log("Probability of being heard : " + probabilityHeard);
                        //Debug.Log("Random hearing var : " + randHeardVar);
                        if (randHeardVar < probabilityHeard)
                        {
                            //Debug.Log("You've startled the deer - it's alert to your threat");
                            playerHeard = true;
                        }
                    }
                }

                // Player close
                else if (distanceToPlayer <= closeDist)
                {
                    //Debug.Log("---- You are quite close to the deer ----");
                    if (howFastPlayer > 0.001)
                    {
                        probabilityHeard = minCloseHeardProb + (howFastPlayer * closeHeardProbHeadroom);
                        randHeardVar = Random.Range(0.0F, 1.0F);
                        //Debug.Log("Probability of being heard : " + probabilityHeard);
                        //Debug.Log("Random hearing var : " + randHeardVar);
                        if (randHeardVar < probabilityHeard)
                        {
                            //Debug.Log("You've startled the deer - it's alert to your threat");
                            playerHeard = true;
                        }
                    }

                }

                // Payer far
                else if (distanceToPlayer <= farDist)
                {
                    //Debug.Log("---- You are far from the deer ----");
                    if (howFastPlayer > 0.001)
                    {
                        probabilityHeard = minFarHeardProb + (howFastPlayer * farHeardProbHeadroom);
                        randHeardVar = Random.Range(0.0F, 1.0F);
                        //Debug.Log("Probability of being heard : " + probabilityHeard);
                        //Debug.Log("Random hearing var : " + randHeardVar);
                        if (randHeardVar < probabilityHeard)
                        {
                            //Debug.Log("You've startled the deer - it's alert to your threat");
                            playerHeard = true;
                        }
                    }
                }

                checkIfHeardCounter = checkIfHeard;
            }
        }

        // Head Buffer Count Down
        if (heardBuffer)
        {
            heardBufferCounter -= 1 * Time.deltaTime;
            if (heardBufferCounter <= 0)
            {
                heardBuffer = false;
            }
        }

    }



}
