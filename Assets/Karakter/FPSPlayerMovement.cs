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

    // Sandalyeye oturma ve kalkma i�lemi i�in de�i�kenler
    private bool isSitting = false;
    private Transform chairPosition;  // Sandalyenin oturma pozisyonu
    private Vector3 originalPosition; // Sandalyeden kalkarken orijinal pozisyon
    private Quaternion originalRotation; // Sandalyeden kalkarken orijinal rotasyon
    private float originalCameraHeight; // Kameran�n orijinal y�ksekli�i

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

        // Sandalyenin oturma noktas� (Trigger olarak i�aretlendi�inden emin olun)
        chairPosition = transform.Find("SitPoint");  // "SitPoint" objesini do�ru olarak yerle�tirin
        if (chairPosition == null)
        {
            Debug.LogError("SitPoint not found. Make sure it is attached to the chair.");
        }
    }

    void Update()
    {
        if (!photonView.IsMine || characterController == null) return;

        // Sandalyede de�ilken normal hareketi kontrol et
        if (!isSitting)
        {
            Move();
            ApplyGravity();
            LookAround();
            HandleCrouch();
            Jump();
        }
    }

    void Move()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Kamera y�n�ne g�re hareket
        Vector3 moveDirection = playerCamera.transform.forward * verticalInput + playerCamera.transform.right * horizontalInput;
        moveDirection.y = 0;
        moveDirection.Normalize();

        // Sprint kontrol�
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

        // Di�er oyuncular�n hareketini senkronize et
        photonView.RPC("SyncMovement", RpcTarget.Others, transform.position, transform.rotation);
    }

    void Jump()
    {
        if (characterController.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            photonView.RPC("SyncJump", RpcTarget.Others, velocity.y);  // Z�plama h�z�n� di�er oyunculara ilet
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
                photonView.RPC("SyncCrouch", RpcTarget.Others, true);  // Di�er oyunculara crouch durumu g�nder
            }
            else
            {
                StandUp();
                photonView.RPC("SyncCrouch", RpcTarget.Others, false);  // Di�er oyunculara stand-up durumu g�nder
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

    // OnTriggerEnter ile sandalyeye otur
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chair") && !isSitting)  // Sandalyenin �zerine girdi�inde
        {
            SitOnChair();  // Sandalyeye otur
        }
    }

    // OnTriggerExit ile sandalyeden kalk
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chair") && isSitting)  // Sandalyeden ��kt���nda
        {
            StandUpFromChair();  // Sandalyeden kalk
        }
    }

    // Sandalyeye oturmak i�in metod
    void SitOnChair()
    {
        isSitting = true;

        // Oturulacak pozisyonu ayarla
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalCameraHeight = playerCamera.transform.localPosition.y;

        // Karakteri sandalyeye oturacak �ekilde yerle�tir
        transform.position = chairPosition.position;
        transform.rotation = chairPosition.rotation;

        // Kameray� sandalyeye uygun �ekilde yerle�tir
        playerCamera.transform.localPosition = new Vector3(0, chairPosition.position.y, 0);

        // Karakterin hareketini ge�ici olarak devre d��� b�rak
        characterController.enabled = false;

        photonView.RPC("SyncSit", RpcTarget.Others, true);
    }

    // Sandalyeden kalkmak i�in metod
    void StandUpFromChair()
    {
        isSitting = false;

        // Karakteri eski pozisyona geri getir
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Kameray� eski y�ksekli�ine geri getir
        playerCamera.transform.localPosition = new Vector3(0, originalCameraHeight, 0);

        // Karakteri hareket ettirmek i�in characterController'� tekrar etkinle�tir
        characterController.enabled = true;

        photonView.RPC("SyncSit", RpcTarget.Others, false);
    }

    // Di�er oyunculara senkronize edilen hareket bilgilerini g�nder
    [PunRPC]
    void SyncMovement(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    // Z�plama senkronizasyonu i�in RPC
    [PunRPC]
    void SyncJump(float jumpVelocity)
    {
        velocity.y = jumpVelocity;
    }

    // Crouch senkronizasyonu i�in RPC
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

    // Sandalyeye oturma durumu i�in senkronizasyon
    [PunRPC]
    void SyncSit(bool sitting)
    {
        if (sitting)
        {
            SitOnChair();
        }
        else
        {
            StandUpFromChair();
        }
    }
}
