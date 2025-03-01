using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public bool IsOpen { get; private set; }
    public float doorRotationSpeed = 3f;
    public float openAngle = 90f;
    public float closeAngle = 0f;

    private Quaternion targetRotation;

    private void Start()
    {
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        // Kapý açýlma ve kapanma animasyonu
        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, doorRotationSpeed * Time.deltaTime);
        }
    }

    public void OpenDoor()
    {
        if (!IsOpen)  // Kapý zaten açýksa tekrar açma
        {
            targetRotation = Quaternion.Euler(0, openAngle, 0);
            IsOpen = true;
        }
    }

    public void CloseDoor()
    {
        if (IsOpen)  // Kapý zaten kapalýysa tekrar kapama
        {
            targetRotation = Quaternion.Euler(0, closeAngle, 0);
            IsOpen = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Oyuncu kapýya yaklaþtýðýnda
        {
            OpenDoor();  // Kapýyý aç
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))  // Oyuncu kapýdan uzaklaþtýðýnda
        {
            CloseDoor();  // Kapýyý kapa
        }
    }
}
