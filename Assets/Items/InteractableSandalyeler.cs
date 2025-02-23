using UnityEngine;

public class InteractableSandalyeler : MonoBehaviour
{
    private bool isSitting = false;
    private Transform player;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public Transform sitPoint; // Sandalyede oturma noktas� (Unity'de bo� bir GameObject olarak ayarla)

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Oyuncuyu bul
    }

    public void SitOrStand()
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        if (!isSitting)
        {
            // Oyuncunun �nceki pozisyonunu ve rotasyonunu kaydet
            originalPosition = player.position;
            originalRotation = player.rotation;

            // �nce karakter kontrolc�s�n� devre d��� b�rak (hareketi kapat)
            controller.enabled = false;

            // Oturma noktas�na ta��madan �nce sandalyenin d�n���n� al
            Quaternion chairRotation = sitPoint.rotation;

            // Oyuncuyu oturma noktas�na ta�� ve y�n�n� sandalyenin d�n���ne g�re ayarla
            player.position = sitPoint.position;
            player.rotation = Quaternion.Euler(0, chairRotation.eulerAngles.y + 180, 0); // Sandalyenin d�n���ne g�re ayarla

            isSitting = true;
        }
        else
        {
            // Oyuncuyu eski pozisyonuna d�nd�r
            player.position = originalPosition;
            player.rotation = originalRotation;

            // Karakter kontrolc�s�n� tekrar a� (hareketi a�)
            controller.enabled = true;

            isSitting = false;
        }
    }
}
