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

    [Header("Easy Mode")]
    public Vector3 lastSafePosition;
    public Vector3?[] previousPositions = new Vector3?[3];
    public Vector3[] previousPositionsFAKE;
    public float spawnTime = 0.1f;
    private float _timeOfLastSpawn;

    [Header("End of the Game")]
    public GameObject endScreen;

    public void Start()
    {
        // ignore collisions with parts
        Physics2D.IgnoreLayerCollision(7, 8); // player & parts
        Physics2D.IgnoreLayerCollision(8, 9); // parts & hazards
        Physics2D.IgnoreLayerCollision(8, 8); // parts & parts
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
        for (int i = 0; i < previousPositions.Length; i++)
        {
            if (previousPositions[i] != null)
                previousPositionsFAKE[i] = previousPositions[i].Value;
            else
                previousPositionsFAKE[i] = Vector3.zero;
        }

        if (data.easyMode && Time.timeScale != 0)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                StopCoroutine("respawn");
                Vector3? tempPosition = getSafePosition();
                if (tempPosition != null)
                    lastSafePosition = tempPosition.Value;
                StartCoroutine(respawn());
            }
        }

        if (dead || Time.timeScale == 0)
            return;
        

        // jumping 
        if (Input.GetKeyDown(KeyCode.Space) && Grounded())
        {
            chargingJump = true;
            chargeUI.SetActive(true);
            jumpStartTime = Time.time;
        }
        if (Input.GetKeyUp(KeyCode.Space) && Grounded() && chargingJump)
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
                rb.AddTorque(-groundRotationSpeed * Time.deltaTime);
            }
            else if (horizontalInput < 0) // rotate left
            {
                rb.angularVelocity = 0;
                rb.AddTorque(groundRotationSpeed * Time.deltaTime);
            }
            else // rotate back to the center
            {
                if (transform.rotation.eulerAngles.z > 0 && transform.rotation.eulerAngles.z < 180)
                {
                    rb.AddTorque(correctionRotationSpeed * Time.deltaTime);
                }
                else
                {
                    rb.AddTorque(-correctionRotationSpeed * Time.deltaTime);
                }
            }
        }
        else // air rotation
        {
            var horizontalInput = Input.GetAxis("Horizontal");
            if (horizontalInput > 0) // rotate right
            {
                rb.angularVelocity = 0;
                rb.AddTorque(-airRotationSpeed * Time.deltaTime);
            }
            else if (horizontalInput < 0) // rotate left
            {
                rb.angularVelocity = 0;
                rb.AddTorque(airRotationSpeed * Time.deltaTime);
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
        if (dead || Time.timeScale == 0)
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
                if (!dead)
                    StartCoroutine(respawn());
            }
        }

        velocityCopy = rb.velocity;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (dead || Time.timeScale == 0)
            return;

        if (other.gameObject.CompareTag("Death")) // check for death
        {
            if (!dead)
                StartCoroutine(respawn());
        }
        if (other.gameObject.CompareTag("Ground")) // check for ground
        {
            if (Grounded())
            {
                rb.angularVelocity = 0; // reset rotations
                rb.velocity = Vector2.zero;

                if (Time.time > spawnTime + _timeOfLastSpawn)
                {
                    // save the last safe position
                    addSafePosition(lastSafePosition);
                    // set the new last safe position
                    lastSafePosition = transform.position;
                }
            }
            else
            {
                // reset the angular velocity to stop the player from spinning loads
                rb.angularVelocity = 0;

                // bounce
                float angle = Quaternion.Angle(transform.rotation, Quaternion.FromToRotation(Vector2.up, other.contacts[0].normal));
                if (angle >= 180) // hit the ceiling
                    rb.velocity = new Vector2(velocityCopy.x, -rb.velocity.y);
                else // hit a wall
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
                    if (!dead)
                        StartCoroutine(respawn());
                }
            }
            else if (angle > maxAngle)
            {
                if (!dead)
                    StartCoroutine(respawn());
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("End"))
        {
            // end the game
            Time.timeScale = 0f;
            endScreen.SetActive(true);
        }
        else if (other.CompareTag("Coin"))
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

    public void addSafePosition(Vector3 newPosition)
    {
        // move all items forward one position
        for (int i = previousPositions.Length - 2; i >= 0; i--)
        {
            previousPositions[i + 1] = previousPositions[i];
        }
        previousPositions[0] = newPosition;
    }
    public Vector3? getSafePosition()
    {
        if (previousPositions[0] == null || previousPositions[0] == Vector3.zero)
            return null;
        Vector3? firstItem = previousPositions[0];
        for (int i = 0; i < previousPositions.Length - 1; i++)
        {
            previousPositions[i] = previousPositions[i + 1];
        }
        previousPositions[previousPositions.Length - 1] = null;
        return firstItem;
    }

    public IEnumerator respawn()
    {
        // deaths text
        currentDeaths++;
        data.currentDeaths++;
        deathsText.text = currentDeaths.ToString();

        dead = true;
        // turn off the charge slider
        chargeUI.SetActive(false);
        chargeSlider.value = chargeSlider.minValue;
        chargingJump = false;

        // Spawn a particle effect

        for (int i = 0; i < renderers.Length; i++) { renderers[i].enabled = false; }
        // stop dead player from moving
        rb.angularVelocity = 0;
        rb.velocity = Vector2.zero;
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
        if (data.easyMode)
            transform.position = lastSafePosition;
        else
            transform.position = spawnPoint.transform.position;
        chargingJump = false;
        dead = false;
        _timeOfLastSpawn = Time.time;
    }
}
