using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScript3D : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject Player;
    public GameObject Follower;

    private Rigidbody rbPlayer;
    private Rigidbody rbFollower;

    [Header("Parameters")]
    public float speedMovingPlayer;
    public float speedWalkingPlayer;
    public float speedRunPlayer;
    public float speedMovingFollower;
    public float timeToRun;
    public float speedForTeleport;
    public float maxDistanceToPlayer;
    public float speedTranslate;
    [HideInInspector]
    public float distanceToPlayer;

    // Start is called before the first frame update
    void Start()
    {
        rbPlayer = Player.GetComponent<Rigidbody>();
        rbFollower = Follower.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            speedMovingPlayer = Mathf.Lerp(speedMovingPlayer, speedRunPlayer, timeToRun * Time.deltaTime);
        }
        else speedMovingPlayer = Mathf.Lerp(speedMovingPlayer, speedWalkingPlayer, timeToRun * Time.deltaTime);

        rbPlayer.velocity = new Vector3(Input.GetAxis("Horizontal") * speedMovingPlayer,
            rbPlayer.velocity.y, Input.GetAxis("Vertical") * speedMovingPlayer);

        distanceToPlayer = (Player.transform.position.x - Follower.transform.position.x) * (Player.transform.position.x - Follower.transform.position.x) + (Player.transform.position.z - Follower.transform.position.z) * (Player.transform.position.z - Follower.transform.position.z);
        //rbPlayer.velocity.magnitude > speedForTeleport && 
        if (distanceToPlayer >= maxDistanceToPlayer)
        {
            rbFollower.velocity = (Player.transform.position - Follower.transform.position) * speedMovingFollower;
        }
        else rbFollower.velocity = new Vector3(0, 0, 0);
    }
}
