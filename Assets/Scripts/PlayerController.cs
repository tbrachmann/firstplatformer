using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public float timeToFall;
    float wallRideTimer;

    public Animator anim;

    private Rigidbody2D rb;
    public bool facingRight = true;
    private bool doubleJump = false;
    public float jumpForce;

    float moveHorizontal;
    float moveVertical;
    bool movingBackward;
    bool movingForward;
    //bool airBraked = false;
    private bool wallRiding;
    public bool climbingLadder = false;
    public bool inLadderArea = false;
    public bool standingOnLadder = false;
    //private Collider2D lastWall;

    public bool grounded;
    public LayerMask whatIsGround;
    public Transform groundCheckLeft;
    public Transform groundCheckRight;

    //stuff for camera
    public float lastYpos;
    public bool cameraChange = false;

    RaycastHit2D walls;
    public LayerMask whatIsWall;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.freezeRotation = true;
        lastYpos = rb.position.y;
    }
	
	void FixedUpdate () {
        //grounded check, runs every physics call
        //right now ladder is also set to be ground - need to change this to get rid
        //of infinite jumps in ladder
        //this needs to change for when you get off the ladder at the bottom
        grounded = CheckForGround() || standingOnLadder;
        //preserve the last y value on the ground?
        if (grounded)
        {
            //the initial condition
            //if (float.IsNaN(lastYpos)) lastYpos = rb.position.y;
            if (!(rb.position.y >= lastYpos - 0.2f && rb.position.y <= lastYpos + 0.2f))
            {
                //if grounded and y-value has changed then it should trigger a camera move
                cameraChange = true;
                lastYpos = rb.position.y;
            }
            /*else
            {
                cameraChange = false;
            }*/
        }
        anim.SetFloat("Speed", Mathf.Abs(moveHorizontal));
        anim.SetFloat("vSpeed", rb.velocity.y);
        anim.SetBool("Grounded", grounded);
        if (grounded)
        {
            //lastWall = null;
            //rb.gravityScale = 1.5f;
            doubleJump = false;
            wallRiding = false; 
            //airBraked = false;
            //if (moveVertical <= 0) climbingLadder = false;
        }
        if (!climbingLadder) {
            rb.isKinematic = false;
            //rb.gravityScale = 1;
        }
        //do i still need this with change?
        walls = CheckForWalls();
        Collider2D wallCollider = walls.collider;
        //this checks if you are wallriding
        //this is where you would check that the wall you are trying to ride is NOT
        //the same one as the wall you just jumped off of
        if (wallCollider)
        {
            if (movingForward)
            {
                //lastWall = walls.collider;
                ResetTimer();
                //if wallriding:
                //rb.gravityScale = .5f;
                wallRiding = true;
                doubleJump = true;
            }
        }
        //walking on the ground 
        if (grounded && !climbingLadder)
        {
            rb.velocity = new Vector2(moveHorizontal * 8, rb.velocity.y);

        }
        //flying through the air or falling
        else if (!wallRiding && !climbingLadder)
        {
            DragSimulation();
            //if (airBraked) - not based on airbraking any longer
            if(Mathf.Abs(rb.velocity.x) <= 7.5f)
            {
                rb.AddForce(new Vector2(moveHorizontal * 15f, 0));
            }
        }
        //climbing a ladder
        else if (climbingLadder)
        {
            rb.velocity = new Vector2(0, moveVertical * 5f);
        }
        //the rest of this code handles switching directions
        if (moveHorizontal < 0 && facingRight)
        {
            Flip();
        }
        else if (moveHorizontal > 0 && !facingRight) {
            Flip();
        } 
    }

    void Update() {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        movingForward = (facingRight && rb.velocity.x > 0) || (!facingRight && rb.velocity.x < 0);
        if (wallRiding) {
            wallRideTimer -= Time.deltaTime;
            if (wallRideTimer < 0) {
                //what happens when you fall
                wallRiding = false;
                ResetTimer();
            }
        }
        //defunct code to check if you've airbraked
        /*if (!grounded && !wallRiding && !movingForward) {
            airBraked = true;
        }*/
        float moveJump = Input.GetAxis("Jump");
        Vector2 firstJump = new Vector2(0, moveJump * jumpForce);
        //jump code
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (grounded && !inLadderArea)
            {
                //jumping from the ground
                //no camera change if jumping from ground
                cameraChange = false;
                rb.AddForce(new Vector2(0, moveJump * jumpForce));
                doubleJump = false;
            }
            else if (!doubleJump && !wallRiding && !climbingLadder)
            {
                //doubleJump in the air
                //setting y velocity to 0 is enough to get rid of grav acceleration
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(firstJump);
                doubleJump = true;
            }
            else if (wallRiding)
            {
                //jumping off the wall
                //jump off the wall in the opposite direction
                rb.velocity = new Vector2(DirectionAwayFromWall() * 10, 0);
                rb.AddForce(firstJump);
                doubleJump = true;
                wallRiding = false;
            }
            else if (climbingLadder)
            {
                //jumping off the ladder
                //check left or right
                //print("Before:" + climbingLadder);
                rb.isKinematic = false;
                if (moveHorizontal != 0)
                {
                    rb.velocity = new Vector2(moveHorizontal * 8, 0);
                    rb.AddForce(firstJump);
                }
                //check down
                else if (moveVertical < 0)
                {
                    print("down jumping");
                    //reset it so you don't immediately grab the ladder again
                    Input.ResetInputAxes();
                    //print(Input.GetKeyDown(KeyCode.S));
                }
                else
                {
                    rb.AddForce(firstJump);
                    Input.ResetInputAxes();
                }
                climbingLadder = false;
                //not allowed to doubleJump after you jump off the ladder
                doubleJump = true;
                //print("After:" + climbingLadder);
            }
        }
    }

    void DragSimulation() {
        if (Mathf.Abs(rb.velocity.x) >= 7.5f) {
            rb.AddForce(new Vector2(-1 * (rb.velocity.x * 5), 0));
        }
    }

    bool CheckForGround() {
        Vector2 directionVector = new Vector2(0, -1);
        RaycastHit2D left = Physics2D.Raycast(groundCheckLeft.position, directionVector, 0.2f, whatIsGround);
        Debug.DrawRay(groundCheckLeft.position, directionVector * 0.2f);
        RaycastHit2D right = Physics2D.Raycast(groundCheckRight.position, directionVector, 0.2f, whatIsGround);
        Debug.DrawRay(groundCheckRight.position, directionVector * 0.2f);
        return left.collider || right.collider;
    }

    RaycastHit2D CheckForWalls() {
        Vector2 directionVector = new Vector2(facingRight ? 1 : -1, 0);
        Debug.DrawRay(rb.position, directionVector * 0.4f);
        return Physics2D.Raycast(rb.position, directionVector, 0.4f, whatIsWall);
    }

    //make this into a convenience function that always returns the direction away from a an object passed in as 
    //an argument
    float DirectionAwayFromWall() {
        if (walls.collider == null) {
            return Mathf.Sign(transform.localScale.x);
        } 
        else {
            float wallPos = walls.point.x;
            float characterPos = transform.position.x;
            return Mathf.Sign(characterPos - wallPos);
        }
    }

    void ResetTimer() {
        wallRideTimer = timeToFall;
    }

    void Flip() {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

}
