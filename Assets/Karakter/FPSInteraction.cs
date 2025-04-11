using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class FPSInteraction : MonoBehaviourPunCallbacks
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public Transform holdPosition;
    public string leftPositionTag = "LeftPosition";
    public float dropForce = 5f;
    public LayerMask interactableLayers;

    [Header("Stack Settings")]
    public float stackSpacing = 0.3f;
    public float tiltAmount = 15f;
    public float tiltSpeed = 5f;
    public int maxStackCount = 10;
    public float stackMergeDistance = 0.5f;

    [Header("Inventory")]
    public int inventorySize = 5;

    private GameObject[] inventory;
    private int selectedSlot = 0;
    private GameObject heldObject = null;
    private int currentStackCount = 0;
    private Vector3 lastMoveDirection;
    private Stove stove;

    void Awake()
    {
        inventory = new GameObject[inventorySize];
        stove = FindFirstObjectByType<Stove>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleMovementTilt();
        HandleSlotSelection();
        HandleInteractionInput();
    }

    void HandleMovementTilt()
    {
        // Eðer item taþýnýyorsa, tüm hareketi engelle
        if (heldObject != null && currentStackCount > 0)
        {
            return; // Hiçbir hareket iþlemi yapýlmasýn
        }

        // Yalnýzca normal hareket
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (moveDirection.magnitude > 0.1f)
        {
            lastMoveDirection = moveDirection;
            ApplyTiltEffect();
        }
        else
        {
            ResetTiltEffect();
        }
    }



    void ApplyTiltEffect()
    {
        if (heldObject == null || currentStackCount == 0) return;

        float targetTilt = lastMoveDirection.x * tiltAmount;
        Quaternion targetRotation = Quaternion.Euler(0, 0, -targetTilt);

        Transform current = heldObject.transform;
        float tiltFactor = 1f;

        while (current != null)
        {
            current.localRotation = Quaternion.Lerp(
                current.localRotation,
                targetRotation * Quaternion.Euler(0, 0, tiltFactor * 5f),
                tiltSpeed * Time.deltaTime);

            if (current.childCount > 0)
            {
                current = current.GetChild(0);
                tiltFactor += 0.5f;
            }
            else
            {
                current = null;
            }
        }
    }

    void ResetTiltEffect()
    {
        if (heldObject == null) return;

        Transform current = heldObject.transform;
        while (current != null)
        {
            current.localRotation = Quaternion.Lerp(
                current.localRotation,
                Quaternion.identity,
                tiltSpeed * Time.deltaTime);

            current = current.childCount > 0 ? current.GetChild(0) : null;
        }
    }

    void HandleSlotSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize) return;

        if (heldObject != null)
        {
            StoreCurrentItem();
        }

        selectedSlot = slotIndex;
        RetrieveItemFromSlot();
    }

    void StoreCurrentItem()
    {
        inventory[selectedSlot] = heldObject;
        heldObject.SetActive(false);
        heldObject = null;
        currentStackCount = 0;
    }

    void RetrieveItemFromSlot()
    {
        if (inventory[selectedSlot] != null)
        {
            heldObject = inventory[selectedSlot];
            heldObject.SetActive(true);
            heldObject.transform.SetParent(holdPosition);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;

            currentStackCount = CountItemsInStack(heldObject.transform);
            Debug.Log($"Retrieved item from slot {selectedSlot}, stack count: {currentStackCount}");
        }
    }

    int CountItemsInStack(Transform root)
    {
        int count = 0;
        Transform current = root;
        while (current != null)
        {
            count++;
            current = current.childCount > 0 ? current.GetChild(0) : null;
        }
        return count;
    }

    void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) // Shift ile tüm stack'i býrak
        {
            photonView.RPC("RPC_DropAllStackedObjects", RpcTarget.AllBuffered);
        }
        else if (Input.GetKeyDown(KeyCode.G)) // G ile tüm stack'i býrak (Shift gibi)
        {
            photonView.RPC("RPC_DropAllStackedObjects", RpcTarget.AllBuffered);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                TryPickUp();
            }
            else
            {
                if (!TryAddToStack())
                {
                    TryPlaceObject();
                }
            }
        }
    }

    void TryPickUp()
    {
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), interactionDistance, interactableLayers);
        foreach (RaycastHit hit in hits)
        {
            GameObject objToPick = hit.collider.gameObject;
            if (!objToPick.CompareTag("Pickable")) continue;

            PhotonView objPV = objToPick.GetComponent<PhotonView>();
            if (objPV == null) continue;

            Debug.Log($"Trying to pick up: {objToPick.name}");

            if (!objPV.IsMine)
            {
                objPV.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            photonView.RPC("RPC_PickUpObject", RpcTarget.AllBuffered, objPV.ViewID, selectedSlot);
            break; // Sadece ilk geçerli objeyi al
        }
    }

    [PunRPC]
    void RPC_PickUpObject(int objectViewID, int slotIndex)
    {
        PhotonView objPV = PhotonView.Find(objectViewID);
        if (objPV == null)
        {
            Debug.LogError("Object PhotonView not found!");
            return;
        }

        GameObject objToPick = objPV.gameObject;
        Debug.Log($"Picking up object: {objToPick.name}");

        if (!objPV.IsMine)
        {
            objPV.TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        // Eðer baþka bir slot seçilmiþse, önceki itemý sakla
        if (slotIndex != selectedSlot && heldObject != null)
        {
            StoreCurrentItem();
        }

        selectedSlot = slotIndex;
        heldObject = objToPick;
        inventory[selectedSlot] = heldObject;

        // Stack kontrolü
        StackableItem stackable = heldObject.GetComponent<StackableItem>();
        if (stackable != null && stackable.isStacked)
        {
            // Eðer zaten stacklenmiþ bir item alýyorsak, tüm stacki al
            currentStackCount = CountItemsInStack(heldObject.transform);
        }
        else
        {
            currentStackCount = 1;
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        // Fiziksel özellikleri devre dýþý býrak
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Kinematik yapmak, itemin fiziksel etkilerden baðýmsýz olmasýný saðlar
            rb.detectCollisions = false;
        }

        Collider col = heldObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Boyutlarý deðiþtirmeden taþýnmasýný saðla
        // localScale deðerini deðiþtirme, itemin doðal boyutlarýný koru

        Debug.Log($"Picked up: {heldObject.name}, Stack count: {currentStackCount}");
    }

    bool TryAddToStack()
    {
        if (heldObject == null || currentStackCount >= maxStackCount)
        {
            Debug.Log("Cannot stack: No held object or stack limit reached");
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), interactionDistance, interactableLayers);
        foreach (RaycastHit hit in hits)
        {
            GameObject targetObj = hit.collider.gameObject;
            if (targetObj == heldObject) continue;

            Debug.Log($"Trying to stack with: {targetObj.name}");

            StackableItem targetStack = targetObj.GetComponent<StackableItem>();
            StackableItem heldStack = heldObject.GetComponent<StackableItem>();

            if (targetObj.CompareTag("Pickable") &&
                targetStack != null &&
                heldStack != null &&
                targetStack.itemType == heldStack.itemType)
            {
                PhotonView targetPV = targetObj.GetComponent<PhotonView>();
                if (targetPV != null)
                {
                    Debug.Log($"Stacking with: {targetObj.name} (ID: {targetPV.ViewID})");
                    photonView.RPC("RPC_AddToStack", RpcTarget.AllBuffered, targetPV.ViewID);
                    return true;
                }
            }
        }
        return false;
    }

    [PunRPC]
    void RPC_AddToStack(int itemViewID)
    {
        PhotonView itemPV = PhotonView.Find(itemViewID);
        if (itemPV == null)
        {
            Debug.LogError("Item PhotonView not found!");
            return;
        }

        if (heldObject == null)
        {
            Debug.LogError("No held object!");
            return;
        }

        if (currentStackCount >= maxStackCount)
        {
            Debug.Log("Stack limit reached!");
            return;
        }

        GameObject objToStack = itemPV.gameObject;
        if (objToStack == heldObject)
        {
            Debug.LogError("Cannot stack with itself!");
            return;
        }

        StackableItem stackable = objToStack.GetComponent<StackableItem>();
        if (stackable == null)
        {
            Debug.LogError("No StackableItem component!");
            return;
        }

        // Eðer zaten baþka bir stack'e baðlýysa
        if (stackable.isStacked)
        {
            Debug.LogError("Item already stacked!");
            return;
        }

        // Fizik özelliklerini devre dýþý býrak
        Rigidbody rb = objToStack.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider col = objToStack.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Stack pozisyonunu belirle
        Transform attachPoint = GetTopStackPoint();
        objToStack.transform.SetParent(attachPoint);
        objToStack.transform.localPosition = new Vector3(0, stackSpacing, 0);
        objToStack.transform.localRotation = Quaternion.identity;

        // StackableItem durumunu güncelle
        stackable.isStacked = true;
        if (stackable.photonView != null)
        {
            stackable.photonView.RPC("SetStacked", RpcTarget.AllBuffered, true);
        }

        currentStackCount++;
        Debug.Log($"Stack successful! Current count: {currentStackCount}");
    }

    Transform GetTopStackPoint()
    {
        if (currentStackCount == 0) return heldObject.transform;

        Transform top = heldObject.transform;
        while (top.childCount > 0)
        {
            top = top.GetChild(0).transform;
        }
        return top;
    }

    void TryPlaceObject()
    {
        if (heldObject == null) return;

        GameObject leftPos = GameObject.FindGameObjectWithTag(leftPositionTag);
        if (leftPos != null && Vector3.Distance(transform.position, leftPos.transform.position) <= interactionDistance)
        {
            photonView.RPC("RPC_PlaceObject", RpcTarget.AllBuffered, heldObject.GetComponent<PhotonView>().ViewID, leftPos.transform.position);
        }
    }

    [PunRPC]
    void RPC_PlaceObject(int objectViewID, Vector3 position)
    {
        PhotonView objPV = PhotonView.Find(objectViewID);
        if (objPV == null) return;

        GameObject objToPlace = objPV.gameObject;
        objToPlace.transform.SetParent(null);
        objToPlace.transform.position = position;
        objToPlace.transform.rotation = Quaternion.identity;

        // Tüm stack'i býrak
        List<Transform> children = new List<Transform>();
        foreach (Transform child in objToPlace.transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            child.SetParent(null);
            ResetObjectPhysics(child.gameObject);
        }

        ResetObjectPhysics(objToPlace);

        inventory[selectedSlot] = null;
        heldObject = null;
        currentStackCount = 0;

        Item item = objToPlace.GetComponent<Item>();
        if (item != null) item.ResumeCooking();
    }

    [PunRPC]
    void RPC_DropAllStackedObjects()
    {
        if (heldObject == null) return;

        Debug.Log("Dropping all stacked objects");

        // Tüm stack'i býrak
        List<Transform> itemsToDrop = new List<Transform>();
        Transform current = heldObject.transform;

        while (current != null)
        {
            itemsToDrop.Add(current);
            current = current.childCount > 0 ? current.GetChild(0) : null;
        }

        foreach (Transform item in itemsToDrop)
        {
            DropSingleObject(item.gameObject);
        }

        inventory[selectedSlot] = null;
        heldObject = null;
        currentStackCount = 0;
    }

    void DropSingleObject(GameObject obj)
    {
        Debug.Log($"Dropping object: {obj.name}");

        // Parent iliþkisini kes
        obj.transform.SetParent(null);

        // StackableItem durumunu güncelle
        StackableItem stackable = obj.GetComponent<StackableItem>();
        if (stackable != null)
        {
            stackable.isStacked = false;
            if (stackable.photonView != null)
            {
                stackable.photonView.RPC("SetStacked", RpcTarget.AllBuffered, false);
            }
        }

        ResetObjectPhysics(obj);

        // Photon Ownership'i serbest býrak
        PhotonView pv = obj.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            pv.TransferOwnership(0); // Master Client'a devret
        }
    }

    void ResetObjectPhysics(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Fiziksel etkileþimleri tekrar aktif yap
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)) * dropForce);
        }

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(heldObject != null ? heldObject.GetComponent<PhotonView>().ViewID : -1);
            stream.SendNext(currentStackCount);
        }
        else
        {
            int viewID = (int)stream.ReceiveNext();
            currentStackCount = (int)stream.ReceiveNext();
            
            if (viewID != -1)
            {
                PhotonView pv = PhotonView.Find(viewID);
                if (pv != null)
                {
                    heldObject = pv.gameObject;
                }
            }
        }
    }
}