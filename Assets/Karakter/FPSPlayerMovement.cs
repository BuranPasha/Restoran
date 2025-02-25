using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController characterController;
    public Camera playerCamera;
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float lookSpeed = 2f;

    private float currentSpeed;
    private Vector3 velocity;
    private bool isCrouching = false;
    private float originalHeight;
    private float crouchHeight = 0.5f;

    private PhotonView photonView;
    private float verticalRotation = 0f;

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
    }

    void Update()
    {
        if (!photonView.IsMine || characterController == null) return;

        Move();
        ApplyGravity();
        LookAround();
        HandleCrouch();
        Jump();
    }

    void Move()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Kamera yönüne göre hareket
        Vector3 moveDirection = playerCamera.transform.forward * verticalInput + playerCamera.transform.right * horizontalInput;
        moveDirection.y = 0;
        moveDirection.Normalize();

        // Sprint kontrolü
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

        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Diðer oyuncularýn hareketini senkronize et
        photonView.RPC("SyncMovement", RpcTarget.Others, transform.position, transform.rotation);
    }

    void Jump()
    {
        if (characterController.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            photonView.RPC("SyncJump", RpcTarget.Others, velocity.y);  // Zýplama hýzýný diðer oyunculara ilet
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
                photonView.RPC("SyncCrouch", RpcTarget.Others, true);  // Diðer oyunculara crouch durumu gönder
            }
            else
            {
                StandUp();
                photonView.RPC("SyncCrouch", RpcTarget.Others, false);  // Diðer oyunculara stand-up durumu gönder
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

    // Diðer oyunculara senkronize edilen hareket bilgilerini gönder
    [PunRPC]
    void SyncMovement(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    // Zýplama senkronizasyonu için RPC
    [PunRPC]
    void SyncJump(float jumpVelocity)
    {
        velocity.y = jumpVelocity;
    }

    // Crouch senkronizasyonu için RPC
    [PunRPC]
    void SyncCrouch(bool crouching)
    {
        if (crouching)
        {
            Crouch();
        }
        else
        {
            StandUp();
        }
    }
}
