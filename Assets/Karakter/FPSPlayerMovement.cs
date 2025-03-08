using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public CharacterController characterController;
    public Camera playerCamera;
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float lookSpeed = 2f;

    public float sprintFov = 80f; // Koþarken FOV deðeri
    public float normalFov = 60f; // Normal FOV deðeri

    private float currentSpeed;
    private Vector3 velocity;
    private bool isCrouching = false;
    private float originalHeight;
    private float crouchHeight = 0.5f;

    private new PhotonView photonView;
    private float verticalRotation = 0f;

    public bool isSitting = false;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("CharacterController component is missing!");
        }

        currentSpeed = speed;
        originalHeight = characterController.height;

        if (!photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(false);
        }

        // Baþlangýçta fareyi gizle ve kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Kamera FOV baþlangýcý
        playerCamera.fieldOfView = normalFov; // Normal FOV deðeri ile baþla
    }

    void Update()
    {
        if (!photonView.IsMine || characterController == null) return;

        // Sandalyeye oturulmuþsa hareketi engelle, kamera hareketi serbest olsun
        if (!isSitting)
        {
            Move();
            ApplyGravity();
            HandleCrouch();
            Jump();
        }

        // Kamera kontrolü her zaman aktif olsun
        LookAround();

        // Sandalyeye oturma ve kalkma iþlemi
        if (Input.GetKeyDown(KeyCode.E) && photonView.IsMine)
        {
            if (isSitting)
            {
                StandUpFromChair();
            }
            else
            {
                TrySitOnChair();
            }
        }

        // Koþarken FOV'yi deðiþtir
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFov, Time.deltaTime * 5f); // FOV deðiþimi
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFov, Time.deltaTime * 5f); // Normal FOV'ye dönüþ
        }
    }

    void Move()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = playerCamera.transform.forward * verticalInput + playerCamera.transform.right * horizontalInput;
        moveDirection.y = 0;
        moveDirection.Normalize();

        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            currentSpeed = sprintSpeed;
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else
        {
            currentSpeed = speed;
        }

        // Hareket yönüne sadece X ve Z ekseninde hýz ekleyelim
        Vector3 moveVelocity = moveDirection * currentSpeed;

        // Anýnda durma: Sadece yatay hareketi sýfýrla, yerçekimi etkisini koru
        if (horizontalInput == 0 && verticalInput == 0)
        {
            moveVelocity.x = 0;
            moveVelocity.z = 0;
        }

        characterController.Move((moveVelocity + velocity) * Time.deltaTime);
    }

    void Jump()
    {
        if (characterController.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isCrouching)
            {
                Crouch();
            }
            else
            {
                StandUp();
            }
        }
    }

    void Crouch()
    {
        isCrouching = true;
        characterController.height = crouchHeight;
        characterController.center = new Vector3(0, crouchHeight / 2, 0);
        playerCamera.transform.localPosition = new Vector3(0, crouchHeight / 2, 0);
    }

    void StandUp()
    {
        isCrouching = false;
        characterController.height = originalHeight;
        characterController.center = new Vector3(0, originalHeight / 2, 0);
        playerCamera.transform.localPosition = new Vector3(0, originalHeight / 2, 0);
    }

    void TrySitOnChair()
    {
        if (!isSitting)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 3f)) // MESAFEYÝ 3F YAPTIK
            {
                Chair chair = hit.collider.GetComponent<Chair>();
                if (chair != null && chair.sitPoint != null)
                {
                    Debug.Log("Player is sitting on the chair.");
                    photonView.RPC("SyncSit", RpcTarget.All, chair.sitPoint.position, chair.sitPoint.rotation);
                }
                else
                {
                    
                }
            }
            else
            {
                
            }
        }
    }

    void StandUpFromChair()
    {
        Debug.Log("Player has stood up from the chair.");
        photonView.RPC("SyncStandUp", RpcTarget.All);
    }

    [PunRPC]
    void SyncSit(Vector3 sitPosition, Quaternion sitRotation)
    {
        isSitting = true;
        characterController.enabled = false; // Hareketi engelle

        transform.position = sitPosition;
        transform.rotation = sitRotation;

        // Fareyi her zaman gizle ve kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    [PunRPC]
    void SyncStandUp()
    {
        isSitting = false;
        characterController.enabled = true; // Hareketi aç

        // Fareyi her zaman gizle ve kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
