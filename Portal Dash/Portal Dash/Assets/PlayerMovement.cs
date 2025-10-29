using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 55f;
    public float jumpForce = 110f;

    private Rigidbody rb;
    private bool isGrounded;

    [Header("Teleport Settings")]
    private bool ignoreInputAfterTeleport = false;
    private float teleportCoolDown = 0.15f;
    private float teleportTimer = 0f;

    //Jump buffering
    private bool wantsToJump = false;

    [Header("Respawn Settings")]
    public float respawnDelay = 1.0f;
    public Transform respawnPoint;
    private bool isRespawning = false;

    [Header("Level Completion Settings")]
    public float levelCompleteDelay = 2.0f;
    public bool levelCompleted = false;

    //sprite renderer for child sprite object of player
    private SpriteRenderer playerSprite;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip keyGrabSound;
    private AudioSource audioSource;
    
    //custom volumes of different sound effects
    public float jumpSoundVolume = 0.6f;
    public float deathSoundVolume = 1.0f;
    public float keyGrabSoundVolume = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        if (playerSprite == null)
        {
            Debug.LogWarning("PlayerMovement: No SpriteRenderer found in child of player object.");
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //disable jump input during respawn or level completion
        if (isRespawning || levelCompleted)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            wantsToJump = true;
        }
    }

    // Called by the Portal script after teleporting
    public void OnTeleported()
    {
        ignoreInputAfterTeleport = true;
        teleportTimer = teleportCoolDown;
    }

    // FixedUpdate is called at a fixed interval (50 fps by default)
    // and is optimal for physics-related movement
    void FixedUpdate()
    {
        if (isRespawning)
        {
            //disable movement during respawn
            rb.velocity = Vector3.zero;
            return;
        }

        //disable any further movement upon level completion
        //but allow the player to fall if in the air
        if (levelCompleted)
        {
            //stop horizontal movement upon level completion
            Vector3 curr_vel = rb.velocity;
            curr_vel.x = 0f;
            rb.velocity = curr_vel;

            return;
        }

        if (ignoreInputAfterTeleport)
        {
            teleportTimer -= Time.fixedDeltaTime;
            if (teleportTimer <= 0f)
            {
                ignoreInputAfterTeleport = false;
            }
            else
            {
                return; //skip input forces during cooldown
            }
        }

        // Horizontal movement
        float moveX = Input.GetAxis("Horizontal");

        //forc-based movement
        Vector3 targetSpeed = new Vector3(moveX, 0, 0) * moveSpeed;
        Vector3 curr_x_vel = new Vector3(rb.velocity.x, 0, 0);
        Vector3 speedDiff = targetSpeed - curr_x_vel;

        //rb.AddForce(speedDiff, ForceMode.Force);
        rb.AddForce(speedDiff, ForceMode.VelocityChange);

        if (wantsToJump && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            
            audioSource.volume = jumpSoundVolume;
            audioSource.PlayOneShot(jumpSound);
        }
        wantsToJump = false;        //reset after physics step

        // Apply manual drag when grounded and not pressing movement
        if (isGrounded && Mathf.Abs(moveX) < 0.1f)
        {
            Vector3 dampenedVel = rb.velocity;
            dampenedVel.x *= 0.85f;
            rb.velocity = dampenedVel;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //check if we hit the ground
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if player touches a spike
        if (other.gameObject.CompareTag("Spike") && !isRespawning)
        {
            audioSource.volume = deathSoundVolume;
            audioSource.PlayOneShot(deathSound);
            StartCoroutine(RestartLevel());
        }

        //if player obtains the key
        if (other.CompareTag("Key") && !levelCompleted)
        {
            levelCompleted = true;              //prevent multiple triggers

            audioSource.volume = keyGrabSoundVolume;
            audioSource.PlayOneShot(keyGrabSound);

            //key dissapears upon being collected
            SpriteRenderer keySprite = other.GetComponent<SpriteRenderer>();
            if (keySprite != null)
            {
                keySprite.enabled = false;
            }

            StartCoroutine(NextLevel());
        }
    }

    private IEnumerator RestartLevel()
    {
        isRespawning = true;                //prevent multiple triggers during respawn
        playerSprite.enabled = false;       //player disappears upon death

        //reset position and velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = respawnPoint.position;

        //wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);

        playerSprite.enabled = true;        //show player again after respawn
        isRespawning = false;               //reset respawn flag back to false now that respawn is complete
    }
    
    private IEnumerator NextLevel()
    {
        //wait for level complete delay
        yield return new WaitForSeconds(levelCompleteDelay);

        //load next scene
        SceneLoader.Instance.LoadNextScene();

        levelCompleted = false;
    }
}
