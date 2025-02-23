using UnityEngine;

public class InteractableSandalyeler : MonoBehaviour
{
    private bool isSitting = false;
    private Transform player;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public Transform sitPoint; // Sandalyede oturma noktasý (Unity'de boþ bir GameObject olarak ayarla)

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Oyuncuyu bul
    }

    public void SitOrStand()
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        if (!isSitting)
        {
            // Oyuncunun önceki pozisyonunu ve rotasyonunu kaydet
            originalPosition = player.position;
            originalRotation = player.rotation;

            // Önce karakter kontrolcüsünü devre dýþý býrak (hareketi kapat)
            controller.enabled = false;

            // Oyuncuyu oturma noktasýna taþý ve yönünü düzelt
            player.position = sitPoint.position;
            player.rotation = Quaternion.Euler(0, sitPoint.rotation.eulerAngles.y + 180, 0);

            isSitting = true;
        }
        else
        {
            // Oyuncuyu eski pozisyonuna döndür
            player.position = originalPosition;
            player.rotation = originalRotation;

            // Karakter kontrolcüsünü tekrar aç (hareketi aç)
            controller.enabled = true;

            isSitting = false;
        }
    }
}
