using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;
    public float lerpTime;
    public Vector3 cameraOffset;

    // Update is called once per frame
    void FixedUpdate()
    {
        var newPos = new Vector3(0, player.transform.position.y, 0);
        transform.position = Vector3.Lerp(transform.position, newPos + cameraOffset, lerpTime);
    }
}
