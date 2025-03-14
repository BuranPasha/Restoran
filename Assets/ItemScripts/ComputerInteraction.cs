using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class ComputerInteraction : MonoBehaviour
{
    public GameObject computerUI; // Bilgisayar UI'sý
    public Transform sitPosition; // Bilgisayarýn önünde oturulacak pozisyon
    public Transform computerScreenView; // Kameranýn bilgisayar ekranýna bakacaðý pozisyon

    public GameObject sosyalMedyaPanel; // Sosyal Medya paneli
    public GameObject finansPanel; // Finans paneli
    public GameObject yemekSipariþiPanel; // Yemek Sipariþi paneli

    private GameObject player; // Oyuncu objesi
    private Camera playerCamera; // Oyuncunun kamerasý
    private bool isUsingComputer = false; // Bilgisayar kullanýlýyor mu?
    private Vector3 originalPlayerPosition; // Oyuncunun baþlangýç pozisyonu
    private Quaternion originalPlayerRotation; // Oyuncunun baþlangýç rotasý
    private Quaternion originalCameraRotation; // Kameranýn baþlangýç rotasý

    // Oyuncunun eski boyutunu saklayacak deðiþken
    private Vector3 originalPlayerScale;

    void Start()
    {
        Debug.Log("Bilgisayar Script'i baþlatýldý!");

        // Bilgisayar UI baþta kapalý olmalý
        if (computerUI != null)
        {
            computerUI.SetActive(false);
        }
        else
        {
            Debug.LogError("computerUI atanmadý! Inspector'dan atayýn.");
        }

        // Panellerin baþlangýçta kapalý olmasý gerektiðini unutmayýn
        if (sosyalMedyaPanel != null)
        {
            sosyalMedyaPanel.SetActive(false);
        }
        if (finansPanel != null)
        {
            finansPanel.SetActive(false);
        }
        if (yemekSipariþiPanel != null)
        {
            yemekSipariþiPanel.SetActive(false);
        }

        // Photon ile karakter spawn olduktan sonra player ve kamerayý bul
        InvokeRepeating("FindPlayerAndCamera", 1f, 1f);
    }

    void FindPlayerAndCamera()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log("Player bulundu: " + player.name);
            }
        }

        if (playerCamera == null)
        {
            GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (cameraObj != null)
            {
                playerCamera = cameraObj.GetComponent<Camera>();
                Debug.Log("Player Camera bulundu: " + playerCamera.name);
            }
        }

        if (player != null && playerCamera != null)
        {
            CancelInvoke("FindPlayerAndCamera"); // Artýk aramaya gerek yok
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isUsingComputer)
        {
            Debug.Log("E tuþuna basýldý, bilgisayara oturuluyor...");
            UseComputer();
        }

        if (isUsingComputer && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC tuþuna basýldý, bilgisayardan çýkýlýyor...");
            ExitComputer();
        }
    }

    void UseComputer()
    {
        if (player == null || playerCamera == null || sitPosition == null || computerScreenView == null)
        {
            Debug.LogError("UseComputer() çaðrýldý ama bazý bileþenler eksik! Player veya Camera null olabilir.");
            return;
        }

        isUsingComputer = true;

        // Oyuncunun þu anki pozisyonunu kaydet
        originalPlayerPosition = player.transform.position;
        originalPlayerRotation = player.transform.rotation;

        // Oyuncuyu bilgisayarýn önüne al
        player.transform.position = sitPosition.position;
        player.transform.rotation = sitPosition.rotation;

        // Kameranýn pozisyonunu deðiþtirmiyoruz, sadece dönüþü koruyoruz
        originalCameraRotation = playerCamera.transform.rotation;

        // Bilgisayar UI aç
        if (computerUI != null)
        {
            computerUI.SetActive(true);
        }

        // Fareyi göster ve serbest býrak
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Oyuncu bilgisayara oturdu ve kamera bilgisayara bakýyor.");
    }

    void ExitComputer()
    {
        if (player == null || playerCamera == null)
        {
            Debug.LogError("ExitComputer() çaðrýldý ama player veya playerCamera null! Tekrar bulmaya çalýþýlýyor...");
            FindPlayerAndCamera();
            if (player == null || playerCamera == null)
            {
                Debug.LogError("Otomatik düzeltme baþarýsýz! Player veya Camera eksik.");
                return;
            }
        }

        isUsingComputer = false;

        // Oyuncuyu eski konumuna döndür
        player.transform.position = originalPlayerPosition;
        player.transform.rotation = originalPlayerRotation;

        // Kamerayý eski rotasýna döndür
        playerCamera.transform.rotation = originalCameraRotation;

        // Bilgisayar UI kapat
        if (computerUI != null)
        {
            computerUI.SetActive(false);
        }

        // Fareyi gizle ve kýsýtla
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Tüm panelleri kapat
        CloseSosyalMedya();
        CloseFinansUygulamasi();
        CloseYemekSipariþi();

        Debug.Log("Oyuncu eski konumuna döndü.");
    }


    // Uygulama açma ve kapama iþlevleri
    public void OpenSosyalMedya()
    {
        Debug.Log("Sosyal Medya Uygulamasý açýldý.");

        // Sosyal Medya Panelini aç
        if (sosyalMedyaPanel != null)
        {
            sosyalMedyaPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Sosyal Medya Paneli atanmadý!");
        }
    }

    public void CloseSosyalMedya()
    {
        // Sosyal Medya Panelini kapat
        if (sosyalMedyaPanel != null)
        {
            sosyalMedyaPanel.SetActive(false);
        }
    }

    public void OpenFinansUygulamasi()
    {
        Debug.Log("Finans Uygulamasý açýldý.");

        // Finans Panelini aç
        if (finansPanel != null)
        {
            finansPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Finans Paneli atanmadý!");
        }
    }

    public void CloseFinansUygulamasi()
    {
        // Finans Panelini kapat
        if (finansPanel != null)
        {
            finansPanel.SetActive(false);
        }
    }

    public void OpenYemekSipariþi()
    {
        Debug.Log("Yemek Sipariþi Uygulamasý açýldý.");

        // Yemek Sipariþi Panelini aç
        if (yemekSipariþiPanel != null)
        {
            yemekSipariþiPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Yemek Sipariþi Paneli atanmadý!");
        }
    }

    public void CloseYemekSipariþi()
    {
        // Yemek Sipariþi Panelini kapat
        if (yemekSipariþiPanel != null)
        {
            yemekSipariþiPanel.SetActive(false);
        }
    }
}
