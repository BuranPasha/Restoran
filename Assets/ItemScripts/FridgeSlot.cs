using UnityEngine;

public class StorageSlot : MonoBehaviour
{
    public Transform itemParent;
    public GameObject slotVisual;
    private GameObject storedItem;

    public bool IsOccupied => storedItem != null;

    private void Start()
    {
        UpdateSlotVisual();
    }

    public bool CanStore(GameObject item)
    {
        if (item == null) return false;

        bool canStore = !IsOccupied && item.GetComponent<StorableItem>() != null;
        Debug.Log($"CanStore check - Occupied: {IsOccupied}, HasComponent: {item.GetComponent<StorableItem>() != null}, Result: {canStore}");
        return canStore;
    }

    public bool StoreItem(GameObject item)
    {
        if (!CanStore(item))
        {
            Debug.Log("Cannot store item in slot");
            return false;
        }

        storedItem = item;

        // Parent ve transform ayarlarý
        item.transform.SetParent(itemParent, true);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // Fizik özelliklerini devre dýþý býrak
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        UpdateSlotVisual();
        Debug.Log("Item stored successfully in slot: " + gameObject.name);
        return true;
    }

    public GameObject RetrieveItem()
    {
        if (!IsOccupied) return null;

        GameObject item = storedItem;
        storedItem = null;

        item.transform.SetParent(null);

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        UpdateSlotVisual();
        return item;
    }

    private void UpdateSlotVisual()
    {
        if (slotVisual != null)
        {
            bool shouldShow = !IsOccupied;
            Debug.Log($"Updating slot visual - Show: {shouldShow}");
            slotVisual.SetActive(shouldShow);
        }
    }
}