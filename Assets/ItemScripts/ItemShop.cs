using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;

public class ItemShop : MonoBehaviourPun
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public GameObject itemPrefab;
        public int price;
        public TMP_InputField quantityInput;
        public Button addToCartButton;
    }

    public List<ShopItem> shopItems = new List<ShopItem>();
    public Transform itemSpawnPoint;
    public TMP_Text balanceText;
    public TMP_Text cartText;
    public Button clearCartButton;

    private Dictionary<ShopItem, int> cart = new Dictionary<ShopItem, int>();

    void Start()
    {
        UpdateBalanceUI();
        UpdateCartUI();

        for (int i = 0; i < shopItems.Count; i++)
        {
            int index = i;
            shopItems[i].addToCartButton.onClick.AddListener(() => AddToCart(index));
        }

        if (clearCartButton != null)
        {
            clearCartButton.onClick.AddListener(ClearCart);
        }
    }

    public void AddToCart(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= shopItems.Count)
        {
            Debug.LogError("Hata: Geçersiz ürün indexi!");
            return;
        }

        ShopItem item = shopItems[itemIndex];

        if (item == null || item.quantityInput == null)
        {
            Debug.LogError("Hata: Ürün veya miktar alaný null!");
            return;
        }

        int quantity = 1;
        if (int.TryParse(item.quantityInput.text, out int parsedQuantity))
        {
            quantity = Mathf.Max(1, parsedQuantity);
        }

        if (cart.ContainsKey(item))
        {
            cart[item] += quantity;
        }
        else
        {
            cart[item] = quantity;
        }

        UpdateCartUI();
        Debug.Log(item.itemName + " sepete " + quantity + " adet eklendi.");
    }

    void UpdateCartUI()
    {
        cartText.text = "Sepet: \n";
        foreach (var item in cart)
        {
            cartText.text += item.Key.itemName + " x" + item.Value + "\n";
        }
    }

    public void ClearCart()
    {
        cart.Clear();
        UpdateCartUI();
        Debug.Log("Sepet temizlendi!");
    }

    public void PurchaseItems()
    {
        if (cart.Count == 0)
        {
            Debug.LogError("Sepet boþ! Satýn alma iþlemi yapýlamaz.");
            return;
        }

        if (BankingSystem.Instance == null)
        {
            Debug.LogError("Hata: BankingSystem.Instance null!");
            return;
        }

        int totalCost = 0;
        foreach (var item in cart)
        {
            if (item.Key == null)
            {
                Debug.LogError("Hata: Sepette geçersiz ürün var!");
                continue;
            }
            totalCost += item.Key.price * item.Value;
        }

        if (BankingSystem.Instance.SharedMoney < totalCost)
        {
            Debug.LogError("Yetersiz bakiye!");
            return;
        }

        // **Parayý düþ**
        BankingSystem.Instance.AddFunds(-totalCost);

        // **Tüm ürünleri anýnda spawnla**
        SpawnAllItems();

        // **Sepeti temizle**
        ClearCart();
        UpdateBalanceUI();
    }

    void SpawnAllItems()
    {
        Vector3 spawnOffset = Vector3.zero;

        foreach (var item in cart)
        {
            for (int i = 0; i < item.Value; i++)
            {
                photonView.RPC("SpawnItem", RpcTarget.All, item.Key.itemName, itemSpawnPoint.position + spawnOffset);
                spawnOffset += new Vector3(0.5f, 0, 0); // Yan yana dizilsin
            }
        }
    }

    [PunRPC]
    void SpawnItem(string itemName, Vector3 spawnPosition)
    {
        Debug.Log("SpawnItem RPC çaðrýldý: " + itemName);

        ShopItem shopItem = shopItems.Find(x => x.itemName == itemName);
        if (shopItem != null && shopItem.itemPrefab != null)
        {
            GameObject spawnedItem = PhotonNetwork.Instantiate(shopItem.itemPrefab.name, spawnPosition, Quaternion.identity);
            Debug.Log(itemName + " baþarýyla spawn oldu.");
        }
        else
        {
            Debug.LogError("SpawnItem Hatasý: " + itemName + " için prefab bulunamadý veya hatalý!");
        }
    }

    void UpdateBalanceUI()
    {
        if (balanceText != null)
        {
            balanceText.text = "Bakiye: $" + BankingSystem.Instance.SharedMoney.ToString();
        }
    }
}
