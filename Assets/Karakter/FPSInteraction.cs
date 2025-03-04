using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;
    public Transform holdPosition;
    public string leftPositionTag = "LeftPosition"; // B�rakma noktas�n�n etiketi

    private GameObject[] inventory = new GameObject[5]; // 5 Slotlu Envanter
    private int selectedSlot = 0; // Ba�lang��ta 1. slot se�ili

    private GameObject heldObject = null;
    private Rigidbody heldRigidbody;
    private PhotonView heldObjectPhotonView;
    private Tray heldTray = null; // Oyuncunun ta��d��� tepsi


    private Stove stove; // Ocak referans�

    void Start()
    {
        stove = FindFirstObjectByType<Stove>(); // Ocak nesnesini bul
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Envanterdeki slotlar� de�i�tirme (1-5 tu�lar�)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);

        // Nesne alma
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                TryPickUp();
            }
            else
            {
                TryPlaceObject(); // E tu�uyla bir yere koyma
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldTray != null)
            {
                heldTray.TryPickUpItem(); // Tepsiye item eklemeye �al��
            }
            else if (heldObject == null)
            {
                TryPickUp();
            }
            else
            {
                TryPlaceObject();
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (heldTray != null)
            {
                heldTray.DropItem();
                if (heldTray.IsEmpty()) // E�er tepsi tamamen bo�ald�ysa, tepsiyi de b�rak
                {
                    DropTray();
                }
            }
            else if (heldObject != null)
            {
                TryDropObject();
            }
        }



        // Nesne b�rakma
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (heldObject != null)
            {
                TryDropObject(); // G tu�uyla yere b�rakma
            }
        }
    }

    void SelectSlot(int slotIndex)
    {
        if (heldObject != null)
        {
            // Mevcut eldeki nesneyi envantere koy
            inventory[selectedSlot] = heldObject;
            heldObject.SetActive(false); // Nesneyi sakla
        }

        selectedSlot = slotIndex;

        // E�er yeni slottaki bir e�ya varsa, onu elde tut
        if (inventory[selectedSlot] != null)
        {
            heldObject = inventory[selectedSlot];
            heldObject.SetActive(true);
            heldObject.transform.SetParent(holdPosition);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
        else
        {
            heldObject = null;
        }
    }

    void TryPickUpTray(GameObject trayObject)
    {
        PhotonView trayPhotonView = trayObject.GetComponent<PhotonView>();
        if (trayPhotonView == null) return;

        if (!trayPhotonView.IsMine)
        {
            trayPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        heldTray = trayObject.GetComponent<Tray>();
        if (heldTray != null)
        {
            photonView.RPC("RPC_PickUpTray", RpcTarget.AllBuffered, trayPhotonView.ViewID);
        }
    }

    [PunRPC]
    void RPC_PickUpTray(int trayViewID)
    {
        PhotonView trayView = PhotonView.Find(trayViewID);
        if (trayView == null) return;

        GameObject trayObject = trayView.gameObject;
        heldTray = trayObject.GetComponent<Tray>();

        trayObject.transform.SetParent(holdPosition);
        trayObject.transform.localPosition = Vector3.zero;
        trayObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = trayObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void DropTray()
    {
        if (heldTray == null) return;

        photonView.RPC("RPC_DropTray", RpcTarget.AllBuffered, heldTray.photonView.ViewID);
    }

    [PunRPC]
    void RPC_DropTray(int trayViewID)
    {
        PhotonView trayView = PhotonView.Find(trayViewID);
        if (trayView == null) return;

        GameObject trayObject = trayView.gameObject;
        trayObject.transform.SetParent(null);

        Rigidbody rb = trayObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        heldTray = null;
    }

    void TryPickUp()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            GameObject objectToPickUp = hit.collider.gameObject;

            if (objectToPickUp.CompareTag("Pickable"))
            {
                PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();
                if (objectPhotonView == null) return;

                if (!objectPhotonView.IsMine)
                {
                    objectPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                }

                photonView.RPC("RPC_PickUpObject", RpcTarget.AllBuffered, objectPhotonView.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else if (objectToPickUp.CompareTag("Tray")) // Tepsiyi al
            {
                TryPickUpTray(objectToPickUp);
            }
        }
    }


    [PunRPC]
    void RPC_PickUpObject(int objectViewID, int ownerActorNumber)
    {
        PhotonView foundView = PhotonView.Find(objectViewID);
        if (foundView == null) return;

        GameObject objectToPickUp = foundView.gameObject;
        if (!objectToPickUp.GetComponent<PhotonView>().IsMine)
        {
            objectToPickUp.GetComponent<PhotonView>().TransferOwnership(ownerActorNumber);
        }

        // E�er elde nesne varsa, envantere ekle
        if (heldObject != null)
        {
            inventory[selectedSlot] = heldObject;
            heldObject.SetActive(false);
        }

        heldObject = objectToPickUp;
        inventory[selectedSlot] = heldObject;
        heldObject.SetActive(true);

        heldRigidbody = heldObject.GetComponent<Rigidbody>();
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.freezeRotation = true;
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.Euler(0, 90, 0);

        Item itemComponent = heldObject.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.PauseCooking();
        }
    }

    // Nesneyi bir yere koyma (E tu�u)
    void TryPlaceObject()
    {
        if (heldObject == null) return;

        GameObject leftPosition = GameObject.FindGameObjectWithTag(leftPositionTag);
        if (leftPosition != null && Vector3.Distance(transform.position, leftPosition.transform.position) <= interactionDistance)
        {
            photonView.RPC("RPC_PlaceOnLeftPosition", RpcTarget.AllBuffered, heldObject.GetComponent<PhotonView>().ViewID, leftPosition.transform.position);
        }
    }

    [PunRPC]
    void RPC_PlaceOnLeftPosition(int objectViewID, Vector3 position)
    {
        PhotonView foundView = PhotonView.Find(objectViewID);
        if (foundView == null) return;

        GameObject objectToPlace = foundView.gameObject;
        objectToPlace.transform.SetParent(null);
        objectToPlace.transform.position = position;
        objectToPlace.transform.rotation = Quaternion.identity;

        Rigidbody rb = objectToPlace.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.freezeRotation = true;
        }

        heldObject = null;
        inventory[selectedSlot] = null;

        // E�er pi�irilebilir bir �eyse, pi�irmeyi ba�lat
        Item itemComponent = objectToPlace.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.ResumeCooking();
        }

        heldObject = null;
    }

    // Nesneyi yere b�rakma (G tu�u)
    void TryDropObject()
    {
        if (inventory[selectedSlot] == null) return; // E�er se�ili slot bo�sa ��k

        GameObject objectToDrop = inventory[selectedSlot];
        PhotonView objectPhotonView = objectToDrop.GetComponent<PhotonView>();

        if (objectPhotonView != null && objectPhotonView.IsMine)
        {
            // Sahipli�i MasterClient'a ver (sahipsiz gibi davranmas� i�in)
            objectPhotonView.TransferOwnership(PhotonNetwork.MasterClient);
        }

        photonView.RPC("RPC_DropObject", RpcTarget.AllBuffered, objectPhotonView.ViewID);
    }

    [PunRPC]
    void RPC_DropObject(int objectViewID)
    {
        PhotonView foundView = PhotonView.Find(objectViewID);
        if (foundView == null) return;

        GameObject droppedObject = foundView.gameObject;
        Rigidbody droppedRigidbody = droppedObject.GetComponent<Rigidbody>();

        if (droppedRigidbody != null)
        {
            droppedRigidbody.isKinematic = false;
            droppedRigidbody.useGravity = true;
        }

        droppedObject.transform.SetParent(null);
        droppedObject.transform.rotation = Quaternion.identity;

        // Sahipli�i serbest b�rak
        if (foundView.IsMine)
        {
            foundView.TransferOwnership(0);
        }

        // **Envanterden sil**
        inventory[selectedSlot] = null;

        // **Eldeki nesneyi temizle**
        if (heldObject == droppedObject)
        {
            heldObject = null;
        }

        Debug.Log("Nesne b�rak�ld�: " + droppedObject.name);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(heldObject != null ? heldObject.transform.position : Vector3.zero);
            stream.SendNext(heldObject != null ? heldObject.transform.rotation : Quaternion.identity);
        }
        else
        {
            Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
            Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();

            if (heldObject != null)
            {
                heldObject.transform.position = receivedPosition;
                heldObject.transform.rotation = receivedRotation;
            }
        }
    }
}
