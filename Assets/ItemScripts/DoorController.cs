using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform leftDoor;  // Sol kap� transformu
    public Transform rightDoor; // Sa� kap� transformu
    public float openAngle = 80f; // Kap�lar�n a��lma a��s�
    public float smoothSpeed = 2f; // A��l�p kapanma h�z�
    public Transform doorCenter;  // Kap�n�n merkezi transformu

    private bool isOpen = false; // Kap�lar�n a��k/kapal� durumu
    private Quaternion leftDoorClosedRotation; // Sol kap� kapal� rotasyonu
    private Quaternion rightDoorClosedRotation; // Sa� kap� kapal� rotasyonu
    private Quaternion leftDoorOpenRotation; // Sol kap� a��k rotasyonu
    private Quaternion rightDoorOpenRotation; // Sa� kap� a��k rotasyonu

    void Start()
    {
        // Kap�lar�n ba�lang�� rotasyonlar�n� kaydet
        leftDoorClosedRotation = leftDoor.rotation;
        rightDoorClosedRotation = rightDoor.rotation;

        // Kap�lar�n a��k rotasyonlar�n� hesapla
        leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, -openAngle, 0);
        rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        if (isOpen)
        {
            // Kap�lar� a��k pozisyona getir
            leftDoor.rotation = Quaternion.RotateTowards(leftDoor.rotation, leftDoorOpenRotation, smoothSpeed * Time.deltaTime);
            rightDoor.rotation = Quaternion.RotateTowards(rightDoor.rotation, rightDoorOpenRotation, smoothSpeed * Time.deltaTime);
        }
        else
        {
            // Kap�lar� kapal� pozisyona getir
            leftDoor.rotation = Quaternion.RotateTowards(leftDoor.rotation, leftDoorClosedRotation, smoothSpeed * Time.deltaTime);
            rightDoor.rotation = Quaternion.RotateTowards(rightDoor.rotation, rightDoorClosedRotation, smoothSpeed * Time.deltaTime);
        }
    }

    // Kap� a��lma durumunu y�n belirleyerek ayarlama
    [PunRPC]
    public void SetDoorState(bool state, Vector3 playerPosition)
    {
        isOpen = state;

        // Oyuncunun kap�ya yakla�ma y�n�n� kontrol et
        Vector3 direction = playerPosition - doorCenter.position;

        if (direction.z > 0) // Kap�n�n arkas�ndan yakla��yor
        {
            // Kap�y� ters y�nde a�
            leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, openAngle, 0);
            rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, -openAngle, 0);
        }
        else // Kap�n�n �n�nden yakla��yor
        {
            // Kap�y� normal y�nde a�
            leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, -openAngle, 0);
            rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, openAngle, 0);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Veriyi g�nder
            stream.SendNext(isOpen);
        }
        else
        {
            // Veriyi al
            isOpen = (bool)stream.ReceiveNext();
        }
    }
}
