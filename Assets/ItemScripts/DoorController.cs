using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform leftDoor;  // Sol kapý transformu
    public Transform rightDoor; // Sað kapý transformu
    public float openAngle = 80f; // Kapýlarýn açýlma açýsý
    public float smoothSpeed = 2f; // Açýlýp kapanma hýzý

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

        // Kapýlarýn açýk rotasyonlarýný hesapla
        leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, -openAngle, 0);
        rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        if (isOpen)
        {
            // Kapýlarý açýk pozisyona getir
            leftDoor.rotation = Quaternion.Slerp(leftDoor.rotation, leftDoorOpenRotation, Time.deltaTime * smoothSpeed);
            rightDoor.rotation = Quaternion.Slerp(rightDoor.rotation, rightDoorOpenRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // Kapýlarý kapalý pozisyona getir
            leftDoor.rotation = Quaternion.Slerp(leftDoor.rotation, leftDoorClosedRotation, Time.deltaTime * smoothSpeed);
            rightDoor.rotation = Quaternion.Slerp(rightDoor.rotation, rightDoorClosedRotation, Time.deltaTime * smoothSpeed);
        }
    }

    [PunRPC]
    public void SetDoorState(bool state)
    {
        isOpen = state;
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