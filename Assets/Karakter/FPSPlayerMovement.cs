using Photon.Pun;
using UnityEngine;

public class FPSPlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    public float moveSpeed = 5f;  // Normal hareket h�z�
    public float sprintSpeed = 8f;  // Ko�ma h�z�
    public float crouchSpeed = 3f;  // E�ilme h�z�
    public float lookSpeed = 2f;
    public float jumpHeight = 2f;  // Z�plama y�ksekli�i
    public Camera playerCamera;
    private CharacterController characterController;

    private float verticalInput;
    private float horizontalInput;
    private float mouseX;
    private float mouseY;

    public float gravity = -9.81f;  // Yer�ekimi
    private Vector3 velocity;

    private bool isCrouching = false;  // E�ilme durumu
    private float originalHeight;
    private float crouchHeight = 0.5f;  // E�ilme y�ksekli�i

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        originalHeight = characterController.height;

        if (!photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            // Yatay ve dikey hareket
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            // Kameran�n y�nlendirilmesi
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            // Ko�ma (Shift tu�u)
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);

            // Z�plama (Space tu�u)
            if (characterController.isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // E�ilme (Ctrl tu�u)
            if (Input.GetKeyDown(KeyCode.LeftControl) && !isCrouching)
            {
                Crouch();
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl) && isCrouching)
            {
                StandUp();
            }

            // Hareket ve h�z ayarlar�
            float currentSpeed = isSprinting ? sprintSpeed : (isCrouching ? crouchSpeed : moveSpeed);
            Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

            // Y�n de�i�tirme
            transform.Rotate(Vector3.up * mouseX * lookSpeed);
            playerCamera.transform.Rotate(Vector3.left * mouseY * lookSpeed);

            // Yer�ekimi
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    private void Crouch()
    {
        isCrouching = true;
        characterController.height = crouchHeight;  // E�ilme y�ksekli�i
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, crouchHeight / 2, playerCamera.transform.localPosition.z);
    }

    private void StandUp()
    {
        isCrouching = false;
        characterController.height = originalHeight;  // Orijinal y�kseklik
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, originalHeight / 2, playerCamera.transform.localPosition.z);
    }

    // Photon ile veri senkronizasyonu
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Yer payla��m� (position) ve d�n�� payla��m� (rotation)
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Di�er oyunculardan gelen verilerle g�ncelleme
            Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
            Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();

            transform.position = receivedPosition;
            transform.rotation = receivedRotation;
        }
    }
}
