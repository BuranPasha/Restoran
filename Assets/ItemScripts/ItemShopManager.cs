using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int price;
    }

    public List<ShopItem> shopItems = new List<ShopItem>(); // Maðazadaki ürün listesi
    public Transform itemButtonContainer; // Bunu Content objesine baðla
    public GameObject itemButtonPrefab; // Item button prefabý

    public TMP_Text selectedItemNameText; // Seçilen ürün ismi (QuantityPanel içindeki)
    public TMP_Text selectedItemPriceText; // Seçilen ürün fiyatý (QuantityPanel içindeki)
    public GameObject quantityPanel; // Adet girmek için açýlacak panel (QuantityPanel)

    private ShopItem selectedItem;

    private void Start()
    {
        PopulateShop();
    }

    void PopulateShop()
    {
        foreach (ShopItem item in shopItems)
        {
            GameObject newItemButton = Instantiate(itemButtonPrefab, itemButtonContainer); // Prefab oluþtur

            TMP_Text nameText = newItemButton.transform.Find("ItemNameText").GetComponent<TMP_Text>();
            TMP_Text priceText = newItemButton.transform.Find("ItemPriceText").GetComponent<TMP_Text>();
            Button button = newItemButton.GetComponent<Button>();

            nameText.text = item.itemName;
            priceText.text = "$" + item.price.ToString();

            // Butona týklayýnca QuantityPanel'i açacak
            button.onClick.AddListener(() => OpenQuantityPanel(item));
        }
    }

    void OpenQuantityPanel(ShopItem item)
    {
        selectedItem = item;
        selectedItemNameText.text = item.itemName;
        selectedItemPriceText.text = "$" + item.price;
        quantityPanel.SetActive(true);
    }
}
