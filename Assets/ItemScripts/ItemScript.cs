using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public enum ItemType
    {
        Yemek,
        Tatl�,
        Salata,
        ��ecek,
        �atal,
        Ka��k,
        B��ak
    }

    public ItemType itemType; // Bu item'�n t�r�n� belirler

    // Bu metodu, item'� tepsiye yerle�tirirken �a��rabilirsiniz
    public string GetItemSlotName()
    {
        return itemType.ToString(); // Item t�r�n� (�rne�in "�atal", "Yemek") d�nd�r�r
    }
}
