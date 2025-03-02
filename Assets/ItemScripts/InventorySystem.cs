using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public int inventorySize = 5; // Maksimum 5 slot
    private List<GameObject> inventory = new List<GameObject>();

    public bool AddItem(GameObject item)
    {
        if (inventory.Count < inventorySize)
        {
            inventory.Add(item);
            item.SetActive(false); // Eþyayý sahneden kaldýr
            Debug.Log(item.name + " envantere eklendi.");
            return true;
        }
        else
        {
            Debug.Log("Envanter dolu!");
            return false;
        }
    }

    public void RemoveItem(int slotIndex, Transform dropPosition)
    {
        if (slotIndex >= 0 && slotIndex < inventory.Count)
        {
            GameObject item = inventory[slotIndex];
            inventory.RemoveAt(slotIndex);
            item.SetActive(true);
            item.transform.position = dropPosition.position;
            Debug.Log(item.name + " envanterden çýkarýldý.");
        }
        else
        {
            Debug.Log("Geçersiz slot numarasý!");
        }
    }

    public GameObject GetItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventory.Count)
        {
            return inventory[slotIndex];
        }
        return null;
    }

    public int GetItemCount()
    {
        return inventory.Count;
    }
}
