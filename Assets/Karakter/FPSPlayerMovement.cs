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

    // Sandalyeye oturma ve kalkma iþlemi için deðiþkenler
    private bool isSitting = false;
    private Transform chairPosition;  // Sandalyenin oturma pozisyonu
    private Vector3 originalPosition; // Sandalyeden kalkarken orijinal pozisyon
    private Quaternion originalRotation; // Sandalyeden kalkarken orijinal rotasyon
    private float originalCameraHeight; // Kameranýn orijinal yüksekliði

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

        // Sandalyenin oturma noktasý (Trigger olarak iþaretlendiðinden emin olun)
        chairPosition = transform.Find("SitPoint");  // "SitPoint" objesini doðru olarak yerleþtirin
        if (chairPosition == null)
        {
            Debug.LogError("SitPoint not found. Make sure it is attached to the chair.");
        }
    }

    void Update()
    {
        if (!photonView.IsMine || characterController == null) return;

        // Sandalyede deðilken normal hareketi kontrol et
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

    // OnTriggerEnter ile sandalyeye otur
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chair") && !isSitting)  // Sandalyenin üzerine girdiðinde
        {
            SitOnChair();  // Sandalyeye otur
        }
    }

    // OnTriggerExit ile sandalyeden kalk
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chair") && isSitting)  // Sandalyeden çýktýðýnda
        {
            StandUpFromChair();  // Sandalyeden kalk
        }
    }

    // Sandalyeye oturmak için metod
    void SitOnChair()
    {
        isSitting = true;

        // Oturulacak pozisyonu ayarla
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalCameraHeight = playerCamera.transform.localPosition.y;

        // Karakteri sandalyeye oturacak þekilde yerleþtir
        transform.position = chairPosition.position;
        transform.rotation = chairPosition.rotation;

        // Kamerayý sandalyeye uygun þekilde yerleþtir
        playerCamera.transform.localPosition = new Vector3(0, chairPosition.position.y, 0);

        // Karakterin hareketini geçici olarak devre dýþý býrak
        characterController.enabled = false;

        photonView.RPC("SyncSit", RpcTarget.Others, true);
    }

    // Sandalyeden kalkmak için metod
    void StandUpFromChair()
    {
        isSitting = false;

        // Karakteri eski pozisyona geri getir
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Kamerayý eski yüksekliðine geri getir
        playerCamera.transform.localPosition = new Vector3(0, originalCameraHeight, 0);

        // Karakteri hareket ettirmek için characterController'ý tekrar etkinleþtir
        characterController.enabled = true;

        photonView.RPC("SyncSit", RpcTarget.Others, false);
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

    // Sandalyeye oturma durumu için senkronizasyon
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
