using Photon.Pun;
using UnityEngine;

public class FPSPlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    public float moveSpeed = 5f;  // Normal hareket hýzý
    public float sprintSpeed = 8f;  // Koþma hýzý
    public float crouchSpeed = 3f;  // Eðilme hýzý
    public float lookSpeed = 2f;
    public float jumpHeight = 2f;  // Zýplama yüksekliði
    public Camera playerCamera;
    private CharacterController characterController;

    private float verticalInput;
    private float horizontalInput;
    private float mouseX;
    private float mouseY;

    public float gravity = -9.81f;  // Yerçekimi
    private Vector3 velocity;

    private bool isCrouching = false;  // Eðilme durumu
    private float originalHeight;
    private float crouchHeight = 0.5f;  // Eðilme yüksekliði

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

            // Kameranýn yönlendirilmesi
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            // Koþma (Shift tuþu)
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);

            // Zýplama (Space tuþu)
            if (characterController.isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Eðilme (Ctrl tuþu)
            if (Input.GetKeyDown(KeyCode.LeftControl) && !isCrouching)
            {
                Crouch();
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl) && isCrouching)
            {
                StandUp();
            }

            // Hareket ve hýz ayarlarý
            float currentSpeed = isSprinting ? sprintSpeed : (isCrouching ? crouchSpeed : moveSpeed);
            Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

            // Yön deðiþtirme
            transform.Rotate(Vector3.up * mouseX * lookSpeed);
            playerCamera.transform.Rotate(Vector3.left * mouseY * lookSpeed);

            // Yerçekimi
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    private void Crouch()
    {
        isCrouching = true;
        characterController.height = crouchHeight;  // Eðilme yüksekliði
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, crouchHeight / 2, playerCamera.transform.localPosition.z);
    }

    private void StandUp()
    {
        isCrouching = false;
        characterController.height = originalHeight;  // Orijinal yükseklik
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, originalHeight / 2, playerCamera.transform.localPosition.z);
    }

    // Photon ile veri senkronizasyonu
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Yer paylaþýmý (position) ve dönüþ paylaþýmý (rotation)
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Diðer oyunculardan gelen verilerle güncelleme
            Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
            Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();

            transform.position = receivedPosition;
            transform.rotation = receivedRotation;
        }
    }
}
