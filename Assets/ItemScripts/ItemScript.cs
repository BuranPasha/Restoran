using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public enum ItemType
    {
        Yemek,
        Tatlý,
        Salata,
        Ýçecek,
        Çatal,
        Kaþýk,
        Býçak
    }

    public ItemType itemType; // Bu item'ýn türünü belirler

    // Bu metodu, item'ý tepsiye yerleþtirirken çaðýrabilirsiniz
    public string GetItemSlotName()
    {
        return itemType.ToString(); // Item türünü (örneðin "Çatal", "Yemek") döndürür
    }
}
