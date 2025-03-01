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
        // Kap� a��lma ve kapanma animasyonu
        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, doorRotationSpeed * Time.deltaTime);
        }
    }

    public void OpenDoor()
    {
        if (!IsOpen)  // Kap� zaten a��ksa tekrar a�ma
        {
            targetRotation = Quaternion.Euler(0, openAngle, 0);
            IsOpen = true;
        }
    }

    public void CloseDoor()
    {
        if (IsOpen)  // Kap� zaten kapal�ysa tekrar kapama
        {
            targetRotation = Quaternion.Euler(0, closeAngle, 0);
            IsOpen = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Oyuncu kap�ya yakla�t���nda
        {
            OpenDoor();  // Kap�y� a�
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))  // Oyuncu kap�dan uzakla�t���nda
        {
            CloseDoor();  // Kap�y� kapa
        }
    }
}
