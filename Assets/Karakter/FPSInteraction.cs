using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

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
        if (heldObject != null && currentStackCount > 0) return;

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

            // Ensure correct scale when retrieving
            StackableItem stackable = heldObject.GetComponent<StackableItem>();
            if (stackable != null)
            {
                heldObject.transform.localScale = stackable.originalScale;
            }

            currentStackCount = CountItemsInStack(heldObject.transform);
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
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            TryScatterDrop();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            photonView.RPC("RPC_DropStackedObjectsNormally", RpcTarget.AllBuffered);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                // Önce slotlardan almaya çalýþ
                TryPickUpFromSlot();

                // Slotlardan alamazsa normal pickup iþlemi
                if (heldObject == null)
                {
                    TryPickUp();
                }
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

    void TryScatterDrop()
    {
        if (heldObject == null) return;

        List<Transform> itemsToDrop = new List<Transform>();
        Transform current = heldObject.transform;

        while (current != null)
        {
            itemsToDrop.Add(current);
            current = current.childCount > 0 ? current.GetChild(0) : null;
        }

        bool shouldScatter = CalculateScatterChance(itemsToDrop.Count);

        if (shouldScatter)
        {
            photonView.RPC("RPC_ScatterDrop", RpcTarget.AllBuffered);
        }
        else
        {
            Debug.Log("Scatter chance failed - items remain in hand");
        }
    }

    bool CalculateScatterChance(int itemCount)
    {
        // Scatter chances:
        // 1-3 items: 0%
        // 4 items: 30%
        // 5 items: 50%
        // 6 items: 70%
        // 7 items: 80%
        // 8 items: 90%
        // 9+ items: 100%
        float[] scatterChances = { 0f, 0f, 0f, 0f, 0.3f, 0.5f, 0.7f, 0.8f, 0.9f, 1f };
        int index = Mathf.Clamp(itemCount, 0, scatterChances.Length - 1);
        return Random.value < scatterChances[index];
    }

    [PunRPC]
    void RPC_ScatterDrop()
    {
        if (heldObject == null) return;

        List<Transform> itemsToDrop = new List<Transform>();
        Transform current = heldObject.transform;

        while (current != null)
        {
            itemsToDrop.Add(current);
            current = current.childCount > 0 ? current.GetChild(0) : null;
        }

        foreach (Transform item in itemsToDrop)
        {
            DropSingleObject(item.gameObject, true);
        }

        inventory[selectedSlot] = null;
        heldObject = null;
        currentStackCount = 0;
    }

    [PunRPC]
    void RPC_DropStackedObjectsNormally()
    {
        if (heldObject == null) return;

        List<Transform> itemsToDrop = new List<Transform>();
        Transform current = heldObject.transform;

        while (current != null)
        {
            itemsToDrop.Add(current);
            current = current.childCount > 0 ? current.GetChild(0) : null;
        }

        foreach (Transform item in itemsToDrop)
        {
            DropSingleObject(item.gameObject, false);
        }

        inventory[selectedSlot] = null;
        heldObject = null;
        currentStackCount = 0;
    }

    void DropSingleObject(GameObject obj, bool scatter)
    {
        obj.transform.SetParent(null);

        StackableItem stackable = obj.GetComponent<StackableItem>();
        if (stackable != null)
        {
            stackable.isStacked = false;
            obj.transform.localScale = stackable.originalScale;

            if (stackable.photonView != null)
            {
                stackable.photonView.RPC("SetStacked", RpcTarget.AllBuffered, false);
            }
        }

        ResetObjectPhysics(obj, scatter);

        PhotonView pv = obj.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            pv.TransferOwnership(0);
        }
    }

    void ResetObjectPhysics(GameObject obj, bool scatter)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (scatter)
            {
                Vector3 scatterDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.1f, 0.3f), // Reduced upward force
                    Random.Range(-1f, 1f)
                ).normalized;

                rb.AddForce(scatterDirection * dropForce, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(transform.forward * dropForce * 0.3f, ForceMode.Impulse);
            }
        }

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }
    void TryPickUp()
    {
        RaycastHit[] hits = Physics.RaycastAll(
            Camera.main.ScreenPointToRay(Input.mousePosition),
            interactionDistance,
            interactableLayers,
            QueryTriggerInteraction.Collide); // Trigger collider'larý da kontrol et

        foreach (RaycastHit hit in hits)
        {
            // Slotlardan item alma
            if (hit.collider.CompareTag(leftPositionTag))
            {
                StorageSlot slot = hit.collider.GetComponent<StorageSlot>();
                if (slot != null && slot.IsOccupied)
                {
                    GameObject item = slot.RetrieveItem();
                    if (item != null)
                    {
                        photonView.RPC("RPC_PickUpFromSlot", RpcTarget.AllBuffered,
                            item.GetComponent<PhotonView>().ViewID);
                    }
                    return;
                }
            }
            // Yerdeki item'larý alma (orijinal kod)
            if (hit.collider.CompareTag("Pickable"))
            {
                PhotonView objPV = hit.collider.GetComponent<PhotonView>();
                if (objPV != null)
                {
                    if (!objPV.IsMine)
                    {
                        objPV.TransferOwnership(PhotonNetwork.LocalPlayer);
                    }
                    photonView.RPC("RPC_PickUpObject", RpcTarget.AllBuffered, objPV.ViewID, selectedSlot);
                    return;
                }
            }
        }
    }
    void TryPickUpFromSlot()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag(leftPositionTag))
            {
                StorageSlot slot = hit.collider.GetComponent<StorageSlot>();
                if (slot != null && slot.IsOccupied)
                {
                    GameObject item = slot.RetrieveItem();
                    if (item != null)
                    {
                        // Doðrudan eline alma mantýðý
                        heldObject = item;
                        heldObject.transform.SetParent(holdPosition);
                        heldObject.transform.localPosition = Vector3.zero;
                        heldObject.transform.localRotation = Quaternion.identity;

                        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = true;
                            rb.detectCollisions = false;
                        }

                        Debug.Log("Item taken from slot successfully");
                    }
                }
            }
        }
    }
    [PunRPC]
    void RPC_PickUpFromSlot(int itemViewID)
    {
        PhotonView objPV = PhotonView.Find(itemViewID);
        if (objPV == null) return;

        GameObject item = objPV.gameObject;

        if (photonView.IsMine)
        {
            if (heldObject != null)
            {
                StoreCurrentItem();
            }

            heldObject = item;
            inventory[selectedSlot] = item;

            // Item'ý el pozisyonuna taþý
            item.transform.SetParent(holdPosition);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            // Fizik özelliklerini ayarla
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            Collider col = item.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            currentStackCount = CountItemsInStack(heldObject.transform);
        }
    }
    [PunRPC]
    void RPC_PickUpObject(int objectViewID, int slotIndex)
    {
        PhotonView objPV = PhotonView.Find(objectViewID);
        if (objPV == null) return;

        GameObject objToPick = objPV.gameObject;

        if (!objPV.IsMine)
        {
            objPV.TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        if (slotIndex != selectedSlot && heldObject != null)
        {
            StoreCurrentItem();
        }
        selectedSlot = slotIndex;
        heldObject = objToPick;
        inventory[selectedSlot] = heldObject;
        // Reset scale when picking up
        StackableItem stackable = heldObject.GetComponent<StackableItem>();
        if (stackable != null)
        {
            heldObject.transform.localScale = stackable.originalScale;
        }
        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider col = heldObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        currentStackCount = CountItemsInStack(heldObject.transform);
    }
    bool TryAddToStack()
    {
        if (heldObject == null || currentStackCount >= maxStackCount) return false;

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), interactionDistance, interactableLayers);
        foreach (RaycastHit hit in hits)
        {
            GameObject targetObj = hit.collider.gameObject;
            if (targetObj == heldObject) continue;
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
        if (itemPV == null || heldObject == null || currentStackCount >= maxStackCount) return;

        GameObject objToStack = itemPV.gameObject;
        if (objToStack == heldObject) return;

        StackableItem stackable = objToStack.GetComponent<StackableItem>();
        if (stackable == null || stackable.isStacked) return;

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

        // Ölçek ve görünürlük ayarlarý
        objToStack.transform.localScale = Vector3.one; // Ölçeði sýfýrlama
        objToStack.SetActive(true); // Görünürlüðü garanti et

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

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionDistance, interactableLayers))
        {
            Debug.Log("Hit object: " + hit.collider.name + " | Tag: " + hit.collider.tag);

            // LeftPosition tag'ine ve StorageSlot componentine sahip mi kontrol et
            if (hit.collider.CompareTag(leftPositionTag))
            {
                StorageSlot storageSlot = hit.collider.GetComponent<StorageSlot>();
                if (storageSlot != null)
                {
                    Debug.Log("Valid slot found: " + storageSlot.name);

                    if (storageSlot.CanStore(heldObject))
                    {
                        Debug.Log("Can store item in slot");

                        int itemViewId = heldObject.GetComponent<PhotonView>().ViewID;
                        int slotIndex = GetSlotIndex(storageSlot);

                        photonView.RPC("RPC_PlaceObjectInSlot", RpcTarget.AllBuffered, itemViewId, slotIndex);
                    }
                    else
                    {
                        Debug.Log("Slot cannot store this item (maybe already occupied)");
                    }
                }
                else
                {
                    Debug.LogWarning("LeftPosition tag'li objede StorageSlot componenti yok!");
                }
            }
        }
        else
        {
            Debug.Log("No object hit with raycast");
        }
    }
    int GetSlotIndex(StorageSlot slot)
    {
        GameObject[] slotObjects = GameObject.FindGameObjectsWithTag(leftPositionTag);
        for (int i = 0; i < slotObjects.Length; i++)
        {
            if (slotObjects[i].GetComponent<StorageSlot>() == slot)
            {
                return i;
            }
        }
        return -1;
    }
    [PunRPC]
    void RPC_PlaceObjectInSlot(int objectViewID, int slotIndex)
    {
        PhotonView objPV = PhotonView.Find(objectViewID);
        if (objPV == null)
        {
            Debug.LogError("Object PhotonView not found!");
            return;
        }

        GameObject[] slotObjects = GameObject.FindGameObjectsWithTag(leftPositionTag);
        if (slotIndex < 0 || slotIndex >= slotObjects.Length)
        {
            Debug.LogError("Invalid slot index: " + slotIndex);
            return;
        }

        StorageSlot targetSlot = slotObjects[slotIndex].GetComponent<StorageSlot>();
        if (targetSlot == null)
        {
            Debug.LogError("Slot component not found at index: " + slotIndex);
            return;
        }

        GameObject objToPlace = objPV.gameObject;
        Debug.Log("Attempting to place object: " + objToPlace.name + " in slot: " + targetSlot.name);

        if (targetSlot.StoreItem(objToPlace))
        {
            Debug.Log("Item placed successfully in slot");

            if (photonView.IsMine && heldObject == objToPlace)
            {
                inventory[selectedSlot] = null;
                heldObject = null;
                currentStackCount = 0;
            }

            if (objPV.IsMine)
            {
                objPV.TransferOwnership(PhotonNetwork.MasterClient);
            }
        }
        else
        {
            Debug.Log("Failed to place item in slot");
        }
    }
    [PunRPC]
    void RPC_PlaceObject(int objectViewID, Vector3 position)
    {
        PhotonView objPV = PhotonView.Find(objectViewID);
        if (objPV == null) return;

        GameObject objToPlace = objPV.gameObject;

        // Nesne, 'StorageSlot' bileþenine yerleþtiriliyor
        StorageSlot storageSlot = objToPlace.GetComponentInParent<StorageSlot>();
        if (storageSlot != null)
        {
            // Nesnenin yerleþtirilebilmesi için gerekli iþlemler
            storageSlot.StoreItem(objToPlace);
        }

        objToPlace.transform.position = position;
        objToPlace.transform.rotation = Quaternion.identity;

        // Çocuk nesneleri varsa, bunlarý yerinden çýkarmalýyýz
        List<Transform> children = new List<Transform>();
        foreach (Transform child in objToPlace.transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            child.SetParent(null);
            ResetObjectPhysics(child.gameObject, false);
        }

        ResetObjectPhysics(objToPlace, false);

        inventory[selectedSlot] = null;
        heldObject = null;
        currentStackCount = 0;
    }
}