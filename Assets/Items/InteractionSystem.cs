using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public GameObject interactionText;  // UI Text Referansý
    public float raycastDistance = 3f;  // Ray'in mesafesi
    public Transform holdParent;  // Nesneyi tutacak yer (örneðin, oyuncunun elleri)
    private GameObject currentObject;  // Þu an tutulan nesne

    private Camera playerCamera;  // Oyuncunun kamerasý

    void Start()
    {
        playerCamera = Camera.main;  // Oyuncunun kamerasýný al
        interactionText.SetActive(false);  // Baþlangýçta gizle
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);  // Kamera ekranýndan ray oluþtur

        if (Physics.Raycast(ray, out hit, raycastDistance))  // Ray ile etkileþimli nesneleri tespit et
        {
            if (hit.collider.CompareTag("Interactable"))  // Etkileþimli objeye çarptýk
            {
                interactionText.SetActive(true);  // UI yazýsýný göster
                if (Input.GetKeyDown(KeyCode.E))  // E tuþuna basýldýðýnda etkileþim
                {
                    if (currentObject == null)  // Eðer nesne elinizde deðilse, al
                    {
                        currentObject = hit.collider.gameObject;  // Nesneyi alýn
                        hit.collider.GetComponent<Interactable>().PickUp(currentObject, holdParent);  // Nesneyi tutma
                    }
                    else  // Eðer nesne elinizdeyse, býrak
                    {
                        currentObject.GetComponent<Interactable>().Drop(currentObject);
                        currentObject = null;  // Elinizdeki nesneyi býrakýn
                    }
                }
            }
        }
        else
        {
            interactionText.SetActive(false);  // Objeye çarpmadýðýnda yazýyý gizle
        }
    }
}
