using UnityEngine;
using System.Collections;

public class Ladder : MonoBehaviour {

    Collider2D thisCollider;
    float moveVertical;
    public LayerMask whatIsLadder;
    LayerMask whatIsGround;
    Collider2D playerCollider = null;
    PlayerController playerController;
    float ladderHit;
    private bool readyToClimb;

    //used to check if player has entered trigger area, before the ladder can be set to a collider.
    //checks to make sure that if raycast hits ground BUT any part of player collider is still 
    //touching the ladder that the raycast wont yet be deactivated
    public bool colliderStay;
    //ladder's three bools
    //is the player standing on top of the ladder? always false if the player is climbing (can't 
    //be automatically checked because the player's head will enter the space while climbing up
    //checked every frame - performance hit?
    public bool standingOnLadder;
    //is the player currently climbing the ladder? set bool manually in playerController when the 
    //player grabs the ladder (presses up) and then gets its value by checking playerController
    //checked every frame - performance hit?
    public bool climbingLadder = false;
    //is the ladder a collider or a trigger? set manually and used to move between states
    public bool isCollider = false;


    // Use this for initialization
    void Start () {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        thisCollider = GetComponent<Collider2D>();
        whatIsGround = playerController.whatIsGround;
        //thisCollider.isTrigger = true;
    }

    // Update is called once per frame
    void Update () {
        //print(transform.TransformPoint(thisCollider.transform.position));
        //print(ladderHit);
        climbingLadder = playerController.climbingLadder;
        moveVertical = Input.GetAxis("Vertical");
        //if the player is not climbing the ladder (which makes this automatically false) then
        //the ladder consults checkForPlayer() - which checks the area above the ladder - this is not working yet
        standingOnLadder = climbingLadder ? false : checkForPlayer();
        //standingOnLadder = climbingLadder;
        //check for holding down on top of ladder happens here instead
        if (standingOnLadder && moveVertical >= 0)
        {
            isCollider = true;
        }
        else
        {
            isCollider = false;
        }
        //the master switch for switching between states - everything simply changes the isCollider bool
        if (isCollider)
        {
            thisCollider.isTrigger = false;
        }
        else
        {
            thisCollider.isTrigger = true;
        }
	}

    void OnCollisionEnter2D(Collision2D coll)
    {
        //can i set it back to trigger here?
        //print("is this even called?");
        //colliderStay = true;
        ladderHit = coll.contacts[0].point.x;
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        //the player presses down while standing on the ladder and is now climbing the ladder
        //makes the ladder into a trigger, and then OnTriggerStay code activates
        ladderHit = coll.contacts[0].point.x;
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        print("exiting collider");
        colliderStay = false;
        //ladderHit = coll.contacts[0].point;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        playerCollider = other;
        colliderStay = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        playerController.inLadderArea = true;
        //playerCollider = other;
        //playerCollider = null;
        //use ladderHit here to check - ladderhit should have been already determined in playerCheck - a global variable
        Vector2 characterPosition = other.transform.position;
        //print(characterPosition.x + "," + characterPosition.y);
        //cant stick to the ladder by pressing down if you're grounded
        bool grounded = playerController.grounded;
        //if (((moveVertical != 0 && !grounded) || (moveVertical > 0 && grounded)) && !climbingLadder && readyToClimb)
        //no other way but copying and pasting code?
        //three conditions: 
        //1. standing on the ground trying to climb up (you can only use the up arrow)
        //2. jumping into the ladder area from the side and trying to climb it (use up or down)
        //3. standing on top of the ladder, press down to start climbing down
        if ((grounded && !standingOnLadder && moveVertical > 0 && readyToClimb) || (moveVertical != 0 && !climbingLadder && !grounded) || 
            (moveVertical < 0 && readyToClimb && !climbingLadder && standingOnLadder))
        {
            float newXPos = Mathf.Floor(ladderHit) + 0.5f;
            other.transform.position = new Vector2(newXPos, characterPosition.y);
            //tell the playerController that the player is now climbing the ladder
            playerController.climbingLadder = true;
            playerController.standingOnLadder = false;
            other.attachedRigidbody.isKinematic = true;
        }
        //check for ground while climbing the ladder - allows the player to exit ladder at the bottom
        if (climbingLadder) {
            Debug.DrawRay(characterPosition, new Vector3(0, -0.75f, 0));
            if (Physics2D.Raycast(characterPosition, new Vector2(0, -1), 0.75f, whatIsGround).collider != null)
            {
                playerController.climbingLadder = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //the player is not only not climbing the ladder, but has left the ladder space completely
        //switch to a collider until the player has left the checked area above the ladder
        //this fires when the player reaches the top of the ladder and climbs off
        playerController.climbingLadder = false;
        playerController.inLadderArea = false;
        isCollider = true;
        colliderStay = false;
        //playerCollider = null;
    }

    bool checkForPlayer()
    {
        if (playerCollider)
        {
            //this is to check if player is standing on top of the ladder - and if he presses
            //down then he can start climbing the ladder
            //the problem is here that below raycasts don't return a value IF player is holding
            //down while approaching the ladder
            //because the if statement terminates here
            Vector2 raycastStart = playerCollider.transform.position;
            //start position for raycast from feet
            Vector2 sideRaycastStart = new Vector2(raycastStart.x, raycastStart.y - 0.6f);
            Debug.DrawRay(raycastStart, new Vector3(0, -0.75f, 0));
            Debug.DrawRay(sideRaycastStart, new Vector3(0.5f, 0, 0));
            //check for ground - if player hits the ground then cancel all raycasts
            //also player is obviously no longer standing on the ladder
            //does not activate while colliderStay is active
            if (Physics2D.Raycast(raycastStart, new Vector2(0, -1), 0.75f, whatIsGround).collider != null && !colliderStay)
            {
                playerController.standingOnLadder = false;
                playerCollider = null;
                return false;
            }
            RaycastHit2D rightCast = Physics2D.Raycast(raycastStart, new Vector2(1, 0), 0.5f, whatIsLadder);
            RaycastHit2D leftCast = Physics2D.Raycast(raycastStart, new Vector2(-1, 0), 0.5f, whatIsLadder);
            RaycastHit2D rightLocCast = Physics2D.Raycast(sideRaycastStart, new Vector2(1, 0), 0.5f, whatIsLadder);
            RaycastHit2D leftLocCast = Physics2D.Raycast(sideRaycastStart, new Vector2(-1, 0), 0.5f, whatIsLadder);
            RaycastHit2D ladderCheck = Physics2D.Raycast(raycastStart, new Vector2(0, -1), 0.75f, whatIsLadder);
            if (ladderCheck.collider != null)
            {
                ladderHit = ladderCheck.point.x;
                readyToClimb = true;
            }
            else if (rightLocCast.collider != null || rightCast.collider != null)
            {
                ladderHit = rightLocCast.point.x;
                readyToClimb = false;
            }
            else if (leftLocCast.collider != null || leftCast.collider != null)
            {
                ladderHit = leftLocCast.point.x;
                readyToClimb = false;
            }
            else
            {
                readyToClimb = false;
            }
            //bug-fixing code because ladder collider protrudes on pixel farther left than it should
            /*if (ladderHit != ladderCheck.point.x && ladderCheck.collider == null) {
                if(!playerController.facingRight) ladderHit = ladderHit - 1;
                else ladderHit = Mathf.RoundToInt(ladderHit);
            }*/
            //THE FINAL SOLUTION
            //BECAUSE THERE'S THAT ONE PLACE WHERE THEY OVERLAP, SO JUST CHECK IF ITS OVERLAPPING LOL
            //NOT THE DIRECTION AWAY FROM THE PLAYER THOUGH MAN... COME ON!
            if (playerController.grounded && standingOnLadder) {
                ladderHit = ladderHit + (0.1f * DirectionAwayFromPlayer());
            }
            //else if(ladderHit.x != ladderCheck.point.x && !playerController.facingRight) 
            bool approachingLadderFromSide = rightCast.collider != null && leftCast.collider != null;
            //the value that is returned
            bool finalCheck = ladderCheck.collider != null && !approachingLadderFromSide;
            if (finalCheck) playerController.standingOnLadder = true;
            return finalCheck;
        }
        else
        {
            return false;
        }
    }

    float DirectionAwayFromPlayer() {
        return playerController.facingRight ? 1 : -1;
    }

}
