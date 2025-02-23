using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public GameObject interactionText; // UI'da "Press E" yaz�s�n� g�steren nesne
    public float raycastDistance = 3f; // Etkile�im mesafesi
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
                    // E�er etkile�ime girilen nesne bir sandalye ise, oturma fonksiyonunu �a��r
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
