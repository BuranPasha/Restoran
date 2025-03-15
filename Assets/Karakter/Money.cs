using UnityEngine;

public class Money : MonoBehaviour
{
    public int moneyValue = 10; // Her para kaç birim artýracak?

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Oyuncu çarptýðýnda çalýþýr
        {
            MoneySystem.instance.AddMoney(moneyValue);
            Destroy(gameObject); // Parayý yok et
        }
    }
}
