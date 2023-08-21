using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Game UI")]
    public GameData data;
    public Slider currentDepthSlider;
    public Slider bestDepthSlider;
    public TMP_Text coinsText;
    private int currentCoins;
    public TMP_Text deathsText;
    private int currentDeaths;

    [Header("Timer")]
    public TMP_Text timerText;
    public float startTime;
    public float time;
    public float hou;
    public float min;
    public float sec;

    public void Start()
    {
        // ignore collisions with parts
        Physics2D.IgnoreLayerCollision(7, 8);
        // timer
        startTime = Time.time;

        // base variables
        currentCoins = 0;
        currentDeaths = 0;
        data.currentDeaths = 0;
        data.currentCollectedCoins = 0;
        data.currentDepth = 0;
        data.bestDepth = 0;

        // set the coins UI
        coinsText.text = currentCoins + "/" + data.totalCoins;

        // set up the depth sliders
        currentDepthSlider.minValue = data.minDepth;
        currentDepthSlider.maxValue = data.maxDepth;
        currentDepthSlider.value = currentDepthSlider.minValue;
        bestDepthSlider.minValue = data.minDepth;
        bestDepthSlider.maxValue = data.maxDepth;
        bestDepthSlider.value = bestDepthSlider.minValue;
    }

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

        // calculates the time based upon the current time the game has been running for
        time = Time.time - startTime;
        hou = TimeSpan.FromSeconds(time).Hours;
        min = TimeSpan.FromSeconds(time).Minutes;
        sec = TimeSpan.FromSeconds(time).Seconds;
        data.hou = hou;
        data.min = min;
        data.sec = sec;
        // sets the time to be in the correct format for the game
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hou, min, sec);
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

        // increase the current slider to the players current movement
        data.currentDepth = -transform.position.y;
        currentDepthSlider.value = -transform.position.y;
        if (-transform.position.y >= data.bestDepth)
        {
            bestDepthSlider.value = -transform.position.y;
            data.bestDepth = -transform.position.y;
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

        if (other.gameObject.CompareTag("Death")) // check for death
        {
            StartCoroutine(respawn());
        }
        if (other.gameObject.CompareTag("Ground")) // check for ground
        {
            if (Grounded())
                {
                    rb.angularVelocity = 0; // reset rotations
                    rb.velocity = Vector2.zero;
                }
            else
            {
                // bounce
                rb.velocity = new Vector2(-velocityCopy.x, rb.velocity.y);
            }
        }

        if (!other.gameObject.CompareTag("DeadPlayer")) // check for death
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            currentCoins++;
            coinsText.text = currentCoins + "/" + data.totalCoins;
        }
        else if (other.CompareTag("CheckPoint"))
        {
            spawnPoint = other.gameObject;
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
        // deaths text
        currentDeaths++;
        deathsText.text = currentDeaths.ToString();

        dead = true;
        // turn off the charge slider
        chargeUI.SetActive(false);
        chargeSlider.value = chargeSlider.minValue;
        chargingJump = false;

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
    }
}
