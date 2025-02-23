using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public GameObject interactionText; // UI'da "Press E" yazýsýný gösteren nesne
    public float raycastDistance = 3f; // Etkileþim mesafesi
    public Transform holdParent;
    private GameObject currentObject;

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        interactionText.SetActive(false);
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                interactionText.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Eðer etkileþime girilen nesne bir sandalye ise, oturma fonksiyonunu çaðýr
                    if (hit.collider.GetComponent<InteractableSandalyeler>())
                    {
                        hit.collider.GetComponent<InteractableSandalyeler>().SitOrStand();
                    }
                    else if (hit.collider.GetComponent<Interactable>())
                    {
                        if (currentObject == null)
                        {
                            currentObject = hit.collider.gameObject;
                            hit.collider.GetComponent<Interactable>().PickUp(currentObject, holdParent);
                        }
                        else
                        {
                            currentObject.GetComponent<Interactable>().Drop(currentObject);
                            currentObject = null;
                        }
                    }
                }
            }
        }
        else
        {
            interactionText.SetActive(false);
        }
    }
}
