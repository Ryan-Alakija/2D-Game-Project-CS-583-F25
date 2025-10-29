using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Linked Portal")]
    public GameObject linkedPortal;
    private Portal linkedPortalScript;              //reference to the other portal
    public bool isActive = true;                    //can this portal be used? (only if both portals are active)

    private bool playerIsOverlapping;
    //private Collider playerInPortal;                //reference to player collider when in portal

    private const float EXIT_OFFSET = 2.0f;         //distance to offset player from exit portal upon teleporting

    [Header("Sound Effects")]
    [SerializeField] private AudioClip teleportSound;
    private AudioSource audioSource;

    //teleport cooldown
    //private HashSet<Collider> recentlyTeleported = new HashSet<Collider>();
    //private const float REENTRY_COOLDOWN = 0.2f;    //seconds after teleport during which player cannot re-enter portal

    public void Awake()
    {
        if (linkedPortal != null)
            linkedPortalScript = linkedPortal.GetComponent<Portal>();       //for isOverLapping and isActive checks

        audioSource = GetComponent<AudioSource>();
    }

    private void Teleport(Collider player)
    {
        //if (recentlyTeleported.Contains(player))
        //    return;     //player recently teleported, do not teleport again yet

        Debug.Log("Telporting player...");

        Rigidbody rb = player.GetComponent<Rigidbody>();

        //TESTING: get current position right when player enters portal
        Debug.Log("Enter position: " + rb.position);

        if (rb == null) return;

        // get entry and exit facing directions, and velocity of player upon entering portal
        Vector3 entryUp = transform.up;
        Vector3 exitUp = linkedPortal.transform.up;
        Vector3 entryVelocity = rb.velocity;

        Debug.Log("Entry Velocity: " + entryVelocity);

        //reorient velocity relative to portals
        Vector3 relativeVelocity = transform.InverseTransformDirection(entryVelocity);

        Debug.Log("Relative Velocity: " + relativeVelocity);

        Vector3 newVelocity = linkedPortal.transform.TransformDirection(relativeVelocity);
        Debug.Log("Relative velocity to exit portal: " + newVelocity);

        //check if exit portal is pointing horizontally or vertically,
        //and flip either the x or y component of the new direction accordingly;
        //note that only the x or y component of exitUp will be non-zero
        Debug.Log("exitUp: " + exitUp);
        //if portal is pointing horizontally
        if (exitUp.x != 0f)
        {
            newVelocity.x *= -1;

            //make sure that the player always gets pushed out of the portal based on exit direction
            //player should travel at a horizontal speed proportional to its total speed
            float PossibleHorizontalVelocity = exitUp.x * newVelocity.magnitude;
            if ((exitUp.x > 0f && newVelocity.x < PossibleHorizontalVelocity) || (exitUp.x < 0f && newVelocity.x > PossibleHorizontalVelocity))
            {
                Debug.Log("New Velocity before adjustment: " + newVelocity);
                newVelocity.x = exitUp.x * newVelocity.magnitude;
            }

            Debug.Log("Adjusted newVelocity.x: " + newVelocity.x);

            //set y velocity to zero for horizontal portals
            newVelocity.y = 0f;
        }
        //if portal is pointing vertically
        else
        {
            newVelocity.y *= -1;

            //make sure that the player always gets pushed out of the portal based on exit direction
            float PossibleVerticalVelocity = exitUp.y * newVelocity.magnitude;
            if ((exitUp.y > 0 && newVelocity.y < PossibleVerticalVelocity) || (exitUp.y < 0 && newVelocity.y > PossibleVerticalVelocity))
            {
                newVelocity.y = exitUp.y * newVelocity.magnitude;
            }

            //edge case: if exit portal is facing opposite direction from entry portal,
            //flip x velocity back to what the original x velocity was
            if (Vector3.Dot(entryUp, exitUp) == -1.0f)
            {
                newVelocity.x = entryVelocity.x;
            }
        }

        Debug.Log("New Velocity: " + newVelocity);

        //teleport player's position
        //player.transform.position = linkedPortal.transform.position + exitUp * 2.8f;
        rb.position = linkedPortal.transform.position + exitUp * EXIT_OFFSET;

        //TESTING: get position after teleporting
        Debug.Log("Exit position: " + rb.position);

        //apply reoriented velocity
        rb.velocity = newVelocity;

        Debug.Log("I'm goin this fast now: " + rb.velocity);

        player.GetComponent<PlayerMovement>().OnTeleported();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool levelComplete = other.GetComponent<PlayerMovement>().levelCompleted;

            //prevent immediate back-and-forth teleporting and make sure portal is linked
            if (!playerIsOverlapping && linkedPortal != null)
            {
                //if level is complete, do not play teleport sound
                if (!levelComplete)
                    audioSource.PlayOneShot(teleportSound);

                //even if level is complete, still teleport the player (just for the funnies)
                Teleport(other);
                linkedPortalScript.playerIsOverlapping = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsOverlapping = false;
        }
    }
}
