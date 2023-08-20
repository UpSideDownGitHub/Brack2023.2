using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;

public class PlayerMovement : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject spawnPoint;
    public float respawnWaitTime;
    public SpriteRenderer[] renderers;
    public GameObject[] parts;
    public float[] forces;

    [Header("Rotation")]
    public float groundRotationSpeed = 0.1f;
    public float airRotationSpeed = 0.2f;
    public float correctionRotationSpeed;
    public float maxAngle = 40f;

    [Header("Ground")]
    public float groundCheckDistance;
    public LayerMask groundLayer;

    [Header("Jumping")]
    public float initialJumpForce = 10f;
    public float maxJumpTime = 0.5f;
    public float jumpForcePerSecond = 20f;
    
    [SerializeField] private Rigidbody2D rb;
    public float jumpStartTime;

    [Header("Jumping UI")]
    public GameObject chargeUI;
    public Slider chargeSlider;
    private bool chargingJump;
    private Vector2 velocityCopy;

    private bool dead = false;

    // Update is called once per frame
    void Update()
    {
        if (dead)
            return;

        // jumping 
        if (Input.GetKeyDown(KeyCode.Space) && Grounded())
        {
            chargingJump = true;
            chargeUI.SetActive(true);
            jumpStartTime = Time.time;
        }
        if (Input.GetKeyUp(KeyCode.Space) && Grounded())
        {
            chargingJump = false;
            chargeUI.SetActive(false);
            chargeSlider.value = chargeSlider.minValue;
            float jumpTime = Time.time - jumpStartTime;
            if (jumpTime > maxJumpTime)
                jumpTime = maxJumpTime;
            rb.AddForce(transform.up * (initialJumpForce + jumpForcePerSecond * jumpTime));
        }

        // rotation
        if (Grounded()) // ground rotation
        {
            var horizontalInput = Input.GetAxis("Horizontal");
            if (horizontalInput > 0) // rotate right
            {
                rb.angularVelocity = 0;
                rb.AddTorque(-groundRotationSpeed);
            }
            else if (horizontalInput < 0) // rotate left
            {
                rb.angularVelocity = 0;
                rb.AddTorque(groundRotationSpeed);
            }
            else // rotate back to the center
            {
                if (transform.rotation.eulerAngles.z > 0 && transform.rotation.eulerAngles.z < 180)
                {
                    rb.AddTorque(correctionRotationSpeed);
                }
                else
                {
                    rb.AddTorque(-correctionRotationSpeed);
                }
            }
        }
        else // air rotation
        {
            var horizontalInput = Input.GetAxis("Horizontal");
            if (horizontalInput > 0) // rotate right
            {
                rb.angularVelocity = 0;
                rb.AddTorque(-airRotationSpeed);
            }
            else if (horizontalInput < 0) // rotate left
            {
                rb.angularVelocity = 0;
                rb.AddTorque(airRotationSpeed);
            }
        }
    }

    void LateUpdate()
    {
        if (dead)
            return;

        // fill the slider to show the current fill rate
        if (chargingJump)
        {
            float jumpTime = Time.time - jumpStartTime;
            if (jumpTime > maxJumpTime)
                chargeSlider.value = chargeSlider.maxValue;
            else
                chargeSlider.value = jumpTime / maxJumpTime;
        }

        if (Grounded())
        {
            float angle = Quaternion.Angle(transform.rotation, Quaternion.identity);
            // Check if the angle is greater than or equal to the required rotation
            if (angle > maxAngle)
            {
                StartCoroutine(respawn());
            }
        }

        velocityCopy = rb.velocity;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (dead)
            return;


        if (other.gameObject.CompareTag("Death"))
        {
            StartCoroutine(respawn());
        }
        if (other.gameObject.CompareTag("Ground"))
        {
            if (Grounded())
                rb.angularVelocity = 0; // reset rotations
            else
            {
                // bounce
                rb.velocity  = new Vector2(-velocityCopy.x, rb.velocity.y);
            }
        }

        if (!other.gameObject.CompareTag("DeadPlayer"))
        {
            // check at an allowed angle if we are then ignore if not then kill the player
            float angle = Quaternion.Angle(transform.rotation, Quaternion.FromToRotation(Vector2.up, other.contacts[0].normal));
            if (angle >= 180)
            {
                angle = Quaternion.Angle(transform.rotation, Quaternion.FromToRotation(other.contacts[0].normal, Vector2.down));
                print(angle);
                if (angle < maxAngle)
                {
                    StartCoroutine(respawn());
                }
            }
            else if (angle > maxAngle)
            {
                StartCoroutine(respawn());
            }
        }
    }

    public bool Grounded()
    {
        Debug.DrawRay(transform.position, new Vector2(0, -groundCheckDistance), Color.red);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null)
            {
                if (hits[i].collider.CompareTag("Ground"))
                    return true;
            }
        }
        return false;
    }

    public IEnumerator respawn()
    {
        dead = true;
        // Spawn a particle effect
        for (int i = 0; i < renderers.Length; i++) { renderers[i].enabled = false; }
        for (int i = 0; i < parts.Length; i++)
        {
            var temp = Instantiate(parts[i], transform.position, Quaternion.identity);
            Vector2 force = new Vector2(Random.Range(-forces[0], forces[0]), Random.Range(0, forces[0]));
            float torque = Random.Range(-forces[1], forces[1]);
            temp.GetComponent<Rigidbody2D>().AddForce(force);
            temp.GetComponent<Rigidbody2D>().AddTorque(torque);
        }
        yield return new WaitForSeconds(respawnWaitTime);
        // create a dead body at the current position of the player
        for (int i = 0; i < renderers.Length; i++) { renderers[i].enabled = true; }
        rb.angularVelocity = 0;
        rb.velocity = Vector2.zero;
        transform.rotation = Quaternion.identity;
        transform.position = spawnPoint.transform.position;
        dead = false;
        chargeUI.SetActive(false);
        chargeSlider.value = chargeSlider.minValue;
        chargingJump = false;
    }
}
