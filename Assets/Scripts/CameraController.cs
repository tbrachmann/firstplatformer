using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public GameObject gizmoTest;
    Camera camera;
    Rigidbody2D player;
    PlayerController playerController;
    bool? isLerping = null;

    // Use this for initialization
    void Start () {
        camera = this.GetComponent<Camera>();
        player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update () {
        /*Vector3 cameraPoint = camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));
        gizmoTest.transform.position = new Vector3(cameraPoint.x, cameraPoint.y, 0);*/
        //if player triggers a camera change by jumping onto a platform, then
        //a lerp is called
        //the lerp can't be stopped unless its done lerping!
        print(isLerping);
        if (playerController.cameraChange || (!isLerping.HasValue || isLerping.Value)) {
            Vector3 nextPosition = new Vector3(player.transform.position.x, player.transform.position.y + 2f, this.transform.position.z);
            isLerping = true;
            //this.transform.position = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime * 2f);
            this.transform.position = Vector3.MoveTowards(this.transform.position, nextPosition, Time.deltaTime * 3f);
            if (transform.position == nextPosition) {
                print(transform.position);
                print(nextPosition);
                playerController.cameraChange = false;
                isLerping = false;
            } 
        }
        this.transform.position = new Vector3(player.transform.position.x, this.transform.position.y, this.transform.position.z);
	}

    void OnDrawGizmos() {
        /*Vector3 bottomLeft = camera.ScreenToWorldPoint(new Vector3(pixelWidth * .3f, pixelHeight * .3f, cameraZ));
        Vector3 topLeft = camera.ScreenToWorldPoint(new Vector3(pixelWidth * .3f, pixelHeight * .6f, cameraZ));
        Vector3 topRight = camera.ScreenToWorldPoint(new Vector3(pixelWidth * .6f, pixelHeight * .6f, cameraZ));
        Vector3 bottomRight = camera.ScreenToWorldPoint(new Vector3(pixelWidth * .6f, pixelHeight * .3f, cameraZ));*/ 
        Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(.3f, .3f, 1));
        Vector3 topLeft = camera.ViewportToWorldPoint(new Vector3(.3f, .6f, 1));
        Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(.6f, .6f, 1));
        Vector3 bottomRight = camera.ViewportToWorldPoint(new Vector3(.6f, .3f, 1));
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        //Vector3 cameraCenter = camera.ViewportToWorldPoint(new Vector3(.5f, .5f, this.transform.position.z));
        //Gizmos.DrawWireCube(new Vector3(cameraCenter.x, cameraCenter.y, 0), new Vector3(.3f, .3f, .3f));
    }

    //start the lerp!
    void startLerping() {

    }

    //stop the lerp!
    void stopLerping() {

    }

}
