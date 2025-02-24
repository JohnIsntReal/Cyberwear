using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f; // Degrees per second
    public float jumpForce = 5f;
    private bool isGrounded;
    

    [Header("Grab System")]
    public float grabDistance = 10f;
    public LayerMask grabbableLayer;
    public float ropeSpringForce = 100f;
    public float ropeDamper = 5f;
    
    private LineRenderer ropeRenderer;
    private SpringJoint ropeJoint;
    private GameObject grabbedObject;
    private bool isGrabbing = false;

    private Camera mainCamera;

    public UnityEvent OnInteract = new UnityEvent();

    private GameObject minigame;

    public List<GameObject> minigames = new List<GameObject>();


    public int score = 0;

    public TextMeshProUGUI textComponent;

    

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Setup rope renderer
        ropeRenderer = gameObject.AddComponent<LineRenderer>();
        ropeRenderer.startWidth = 0.1f;
        ropeRenderer.endWidth = 0.1f;
        ropeRenderer.positionCount = 2;
        ropeRenderer.enabled = false;

        mainCamera = Camera.main;
    }

    void Update()
    {
        minigame = minigames[Random.Range(0, minigames.Count)];
        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Handle grabbing
        if (Input.GetMouseButtonDown(0)) // Left click to grab
        {
            if (!isGrabbing)
                TryGrab();
            else
                Release();
        }

        // Update rope visual
        if (isGrabbing)
        {
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, grabbedObject.transform.position);
        }
        textComponent.text = score.ToString();
    }

    void FixedUpdate()
    {
        // Get input axes
        float moveVertical = Input.GetAxisRaw("Vertical");  // W/S keys
        float rotation = Input.GetAxisRaw("Horizontal");    // A/D keys

        // Handle rotation
        Quaternion deltaRotation = Quaternion.Euler(0f, rotation * rotationSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);

        // Handle forward/backward movement
        Vector3 movement = transform.forward * moveVertical * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    void OnCollisionStay(Collision collision)
    {
        // Check if player is grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Check if player left the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }



    void TryGrab()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableLayer))
        {
            grabbedObject = hit.collider.gameObject;

            
            
            // Setup spring joint
            ropeJoint = grabbedObject.AddComponent<SpringJoint>();
            ropeJoint.connectedBody = rb;
            ropeJoint.spring = ropeSpringForce;
            ropeJoint.damper = ropeDamper;
            ropeJoint.autoConfigureConnectedAnchor = false;
            ropeJoint.connectedAnchor = Vector3.zero;
            
            // Make sure object has rigidbody and set it to be frictionless
            Rigidbody objRb = grabbedObject.GetComponent<Rigidbody>();
            if (objRb == null)
            {
                objRb = grabbedObject.AddComponent<Rigidbody>();
                objRb.mass = 1f;
            }
            
            // Store the original physics material if it exists
            Collider objCollider = grabbedObject.GetComponent<Collider>();
            if (objCollider != null)
            {
                // Create and apply frictionless physics material
                PhysicMaterial frictionlessMaterial = new PhysicMaterial("Frictionless");
                frictionlessMaterial.dynamicFriction = 0f;
                frictionlessMaterial.staticFriction = 0f;
                frictionlessMaterial.bounciness = 0f;
                frictionlessMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
                frictionlessMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
                objCollider.material = frictionlessMaterial;
            }
            
            // Enable rope visual
            ropeRenderer.enabled = true;
            isGrabbing = true;


            //checks if the grabbed object has a minigame associated with it
            if(grabbedObject.tag == "interactable")
            {
                if(grabbedObject.gameObject.name == "computer")
                {
                    PlayMinigame();    
                    score++;
                }
                if(grabbedObject.gameObject.name == "tv")
                {
                    print("tv interact");
                    GameObject child1 = grabbedObject.transform.GetChild(0).gameObject;
                    child1.SetActive(!child1.activeSelf);
                }
                Release();
            }
        }
    }

    void Release()
    {
        if (grabbedObject != null)
        {
            // Reset physics material to default
            Collider objCollider = grabbedObject.GetComponent<Collider>();
            if (objCollider != null)
            {
                objCollider.material = null; // This will use Unity's default physics material
            }

            Destroy(ropeJoint);
            ropeRenderer.enabled = false;
            isGrabbing = false;
            grabbedObject = null;
        }
    }

    public void PlayMinigame()
    {
        print("play minigame for" + grabbedObject.gameObject.name);
        if (grabbedObject.gameObject.name == "computer")
        {
            Instantiate(minigame);
        }
    }


}
