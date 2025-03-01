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

        // Kap�lar�n a��k rotasyonlar�n� varsay�lan olarak ayarla (sonradan de�i�tirilecek)
        leftDoorOpenRotation = leftDoorClosedRotation;
        rightDoorOpenRotation = rightDoorClosedRotation;
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

    [PunRPC]
    public void SetDoorState(bool state, Vector3 playerPosition)
    {
        isOpen = state;

        // Oyuncunun kap�ya olan y�n�n� belirle
        Vector3 direction = playerPosition - doorCenter.position;

        if (direction.x > 0) // Sa�dan yakla��yor
        {
            // **Kap�y� oyuncudan UZAK a�**
            leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, -openAngle, 0); // Sol kap�y� �ne a�
            rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, openAngle, 0); // Sa� kap�y� geriye a�
        }
        else if (direction.x < 0) // Soldan yakla��yor
        {
            // **Kap�y� oyuncudan UZAK a�**
            leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, openAngle, 0); // Sol kap�y� geriye a�
            rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, -openAngle, 0); // Sa� kap�y� �ne a�
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
