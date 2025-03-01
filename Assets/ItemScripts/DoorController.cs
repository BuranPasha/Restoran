using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform leftDoor;  // Sol kapý transformu
    public Transform rightDoor; // Sað kapý transformu
    public float openAngle = 80f; // Kapýlarýn açýlma açýsý
    public float smoothSpeed = 2f; // Açýlýp kapanma hýzý
    public Transform doorCenter;  // Kapýnýn merkezi transformu

    private bool isOpen = false; // Kapýlarýn açýk/kapalý durumu
    private Quaternion leftDoorClosedRotation; // Sol kapý kapalý rotasyonu
    private Quaternion rightDoorClosedRotation; // Sað kapý kapalý rotasyonu
    private Quaternion leftDoorOpenRotation; // Sol kapý açýk rotasyonu
    private Quaternion rightDoorOpenRotation; // Sað kapý açýk rotasyonu

    void Start()
    {
        // Kapýlarýn baþlangýç rotasyonlarýný kaydet
        leftDoorClosedRotation = leftDoor.rotation;
        rightDoorClosedRotation = rightDoor.rotation;

        // Kapýlarýn açýk rotasyonlarýný varsayýlan olarak ayarla (sonradan deðiþtirilecek)
        leftDoorOpenRotation = leftDoorClosedRotation;
        rightDoorOpenRotation = rightDoorClosedRotation;
    }

    void Update()
    {
        if (isOpen)
        {
            // Kapýlarý açýk pozisyona getir
            leftDoor.rotation = Quaternion.RotateTowards(leftDoor.rotation, leftDoorOpenRotation, smoothSpeed * Time.deltaTime);
            rightDoor.rotation = Quaternion.RotateTowards(rightDoor.rotation, rightDoorOpenRotation, smoothSpeed * Time.deltaTime);
        }
        else
        {
            // Kapýlarý kapalý pozisyona getir
            leftDoor.rotation = Quaternion.RotateTowards(leftDoor.rotation, leftDoorClosedRotation, smoothSpeed * Time.deltaTime);
            rightDoor.rotation = Quaternion.RotateTowards(rightDoor.rotation, rightDoorClosedRotation, smoothSpeed * Time.deltaTime);
        }
    }

    [PunRPC]
    public void SetDoorState(bool state, Vector3 playerPosition)
    {
        isOpen = state;

        // Oyuncunun kapýya olan yönünü belirle
        Vector3 direction = playerPosition - doorCenter.position;

        if (direction.x > 0) // Saðdan yaklaþýyor
        {
            // **Kapýyý oyuncudan UZAK aç**
            leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, -openAngle, 0); // Sol kapýyý öne aç
            rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, openAngle, 0); // Sað kapýyý geriye aç
        }
        else if (direction.x < 0) // Soldan yaklaþýyor
        {
            // **Kapýyý oyuncudan UZAK aç**
            leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, openAngle, 0); // Sol kapýyý geriye aç
            rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, -openAngle, 0); // Sað kapýyý öne aç
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Veriyi gönder
            stream.SendNext(isOpen);
        }
        else
        {
            // Veriyi al
            isOpen = (bool)stream.ReceiveNext();
        }
    }
}
