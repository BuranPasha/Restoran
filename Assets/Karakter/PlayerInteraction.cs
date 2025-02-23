using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 2f;
    public LayerMask interactableLayer;
    public Transform holdPoint; // Oyuncunun eli
    private GameObject heldItem = null;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem == null)
            {
                TryPickup();
            }
            else
            {
                DropItem();
            }
        }
    }

    void TryPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactionRange, interactableLayer))
        {
            if (hit.collider != null)
            {
                GameObject item = hit.collider.gameObject;

                // Malzemeyi eline al
                heldItem = item;

                // Eðer malzemede özel bir tutma noktasý varsa ona göre hizala
                Transform itemHoldPoint = heldItem.transform.Find("HoldPoint");

                if (itemHoldPoint != null)
                {
                    heldItem.transform.SetParent(holdPoint);
                    heldItem.transform.position = holdPoint.position;
                    // Elin dönüþü ile nesnenin dönüþünü uyumlu hale getir
                    heldItem.transform.rotation = holdPoint.rotation * Quaternion.Inverse(itemHoldPoint.localRotation);
                }
                else
                {
                    // Eðer özel bir HoldPoint yoksa, normal þekilde hizala
                    heldItem.transform.SetParent(holdPoint);
                    heldItem.transform.localPosition = Vector3.zero;
                    heldItem.transform.localRotation = Quaternion.identity;
                }

                // Fizik bileþenlerini kapat ki düþmesin
                Rigidbody rb = heldItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
            }
        }
    }

    void DropItem()
    {
        if (heldItem != null)
        {
            // Malzemeyi býrak
            heldItem.transform.SetParent(null);

            // Fizik bileþenini tekrar aç
            Rigidbody rb = heldItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(transform.forward * 2f, ForceMode.Impulse); // Hafifçe ileri fýrlat
            }

            heldItem = null;
        }
    }
}
