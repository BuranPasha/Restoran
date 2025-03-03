using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;
    public Transform holdPosition;
    public string leftPositionTag = "LeftPosition"; // Býrakma noktasýnýn etiketi

    private GameObject[] inventory = new GameObject[5]; // 5 Slotlu Envanter
    private int selectedSlot = 0; // Baþlangýçta 1. slot seçili

    private GameObject heldObject = null;
    private Rigidbody heldRigidbody;
    private PhotonView heldObjectPhotonView;

    private Stove stove; // Ocak referansý

    void Start()
    {
        stove = FindFirstObjectByType<Stove>(); // Ocak nesnesini bul
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Envanterdeki slotlarý deðiþtirme (1-5 tuþlarý)
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
                TryPlaceObject(); // E tuþuyla bir yere koyma
            }
        }

        // Nesne býrakma
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (heldObject != null)
            {
                TryDropObject(); // G tuþuyla yere býrakma
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

        // Eðer yeni slottaki bir eþya varsa, onu elde tut
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

    void TryPickUp()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            GameObject objectToPickUp = hit.collider.gameObject;

            if (!objectToPickUp.CompareTag("Pickable")) return;

            PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();
            if (objectPhotonView == null) return;

            if (!objectPhotonView.IsMine)
            {
                objectPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            photonView.RPC("RPC_PickUpObject", RpcTarget.AllBuffered, objectPhotonView.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
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

        // Eðer elde nesne varsa, envantere ekle
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

    // Nesneyi bir yere koyma (E tuþu)
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

        // Eðer piþirilebilir bir þeyse, piþirmeyi baþlat
        Item itemComponent = objectToPlace.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.ResumeCooking();
        }

        heldObject = null;
    }

    // Nesneyi yere býrakma (G tuþu)
    void TryDropObject()
    {
        if (inventory[selectedSlot] == null) return; // Eðer seçili slot boþsa çýk

        GameObject objectToDrop = inventory[selectedSlot];
        PhotonView objectPhotonView = objectToDrop.GetComponent<PhotonView>();

        if (objectPhotonView != null && objectPhotonView.IsMine)
        {
            // Sahipliði MasterClient'a ver (sahipsiz gibi davranmasý için)
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

        // Sahipliði serbest býrak
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

        Debug.Log("Nesne býrakýldý: " + droppedObject.name);
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
