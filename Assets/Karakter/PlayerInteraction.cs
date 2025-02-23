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

                // E�er malzemede �zel bir tutma noktas� varsa ona g�re hizala
                Transform itemHoldPoint = heldItem.transform.Find("HoldPoint");

                if (itemHoldPoint != null)
                {
                    heldItem.transform.SetParent(holdPoint);
                    heldItem.transform.position = holdPoint.position;
                    // Elin d�n��� ile nesnenin d�n���n� uyumlu hale getir
                    heldItem.transform.rotation = holdPoint.rotation * Quaternion.Inverse(itemHoldPoint.localRotation);
                }
                else
                {
                    // E�er �zel bir HoldPoint yoksa, normal �ekilde hizala
                    heldItem.transform.SetParent(holdPoint);
                    heldItem.transform.localPosition = Vector3.zero;
                    heldItem.transform.localRotation = Quaternion.identity;
                }

                // Fizik bile�enlerini kapat ki d��mesin
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
            // Malzemeyi b�rak
            heldItem.transform.SetParent(null);

            // Fizik bile�enini tekrar a�
            Rigidbody rb = heldItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(transform.forward * 2f, ForceMode.Impulse); // Hafif�e ileri f�rlat
            }

            heldItem = null;
        }
    }
}
