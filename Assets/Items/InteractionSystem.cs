using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public GameObject interactionText;  // UI Text Referans�
    public float raycastDistance = 3f;  // Ray'in mesafesi
    public Transform holdParent;  // Nesneyi tutacak yer (�rne�in, oyuncunun elleri)
    private GameObject currentObject;  // �u an tutulan nesne

    private Camera playerCamera;  // Oyuncunun kameras�

    void Start()
    {
        playerCamera = Camera.main;  // Oyuncunun kameras�n� al
        interactionText.SetActive(false);  // Ba�lang��ta gizle
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);  // Kamera ekran�ndan ray olu�tur

        if (Physics.Raycast(ray, out hit, raycastDistance))  // Ray ile etkile�imli nesneleri tespit et
        {
            if (hit.collider.CompareTag("Interactable"))  // Etkile�imli objeye �arpt�k
            {
                interactionText.SetActive(true);  // UI yaz�s�n� g�ster
                if (Input.GetKeyDown(KeyCode.E))  // E tu�una bas�ld���nda etkile�im
                {
                    if (currentObject == null)  // E�er nesne elinizde de�ilse, al
                    {
                        currentObject = hit.collider.gameObject;  // Nesneyi al�n
                        hit.collider.GetComponent<Interactable>().PickUp(currentObject, holdParent);  // Nesneyi tutma
                    }
                    else  // E�er nesne elinizdeyse, b�rak
                    {
                        currentObject.GetComponent<Interactable>().Drop(currentObject);
                        currentObject = null;  // Elinizdeki nesneyi b�rak�n
                    }
                }
            }
        }
        else
        {
            interactionText.SetActive(false);  // Objeye �arpmad���nda yaz�y� gizle
        }
    }
}
