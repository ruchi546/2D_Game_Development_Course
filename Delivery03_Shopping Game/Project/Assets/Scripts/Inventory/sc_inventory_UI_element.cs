using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum InventoryLocation
{
    Null,
    Shop,
    Player
}

public class InventoryUIElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image Image;
    [SerializeField] private TextMeshProUGUI AmountText;
    [SerializeField] private TextMeshProUGUI InfoText;
    [SerializeField] private GameObject ToolTipGameObj;
    [SerializeField] private Image ImageText;
    [SerializeField] private GameObject SelectedImage;
    [SerializeField] private InventoryShopUI _shopInventoryUI;
    [SerializeField] private ShopInventory ShopInventory;
    [SerializeField] private PlayerInventory PlayerInventory;

    private Canvas _canvas;
    private GraphicRaycaster _raycaster;
    private Transform _parent;
    private BaseItem _item;
    private InventoryPlayerUI _playerInventoryUI;
    private bool _isMouseOver = false;
    private static InventoryUIElement _selectedItem = null;
    private bool MouseFollows = false;

    private void Start()
    {
        ShopInventory = FindObjectOfType<ShopInventory>();
        PlayerInventory = FindObjectOfType<PlayerInventory>();
    }

    private void Update()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (MouseFollows)
        {
            ToolTipGameObj.transform.position = new Vector3(worldPosition.x, worldPosition.y, ToolTipGameObj.transform.position.z);
        }
    }

    public void SetStuffPlayer(InventarySlot slot, InventoryPlayerUI inventory)
    {
        SetStuf(slot);
        _playerInventoryUI = inventory;
    }

    public void SetStuffShop(InventarySlot slot, InventoryShopUI inventory)
    {
        SetStuf(slot);
        _shopInventoryUI = inventory;
    }

    private void SetStuf(InventarySlot slot)
    {
        Image.sprite = slot.Item.ImageUI;
        AmountText.text = slot.Amount.ToString();
        AmountText.enabled = slot.Amount > 1;

        // Check if the item is a weapon or a consumable
        if (slot.Item is WeaponItem)
        {
            WeaponItem weapon = (WeaponItem)slot.Item;
            InfoText.text = slot.Item.Name + "\nDamage: " + weapon.damage.ToString() + " DPS"+ "\nValue: " + slot.Item.Cost + " Coins";
        }
        else if (slot.Item is ConsumableItem)
        {
            ConsumableItem consumable = (ConsumableItem)slot.Item;
            InfoText.text = slot.Item.Name + "\nLife Restore: " + (consumable.GetLifeRestore() * 100).ToString() + "%" + "\nValue: " + slot.Item.Cost + " Coins";
        }
        else
        {
            InfoText.text = slot.Item.Name + "\nValue: " + slot.Item.Cost + " Coins";
        }

        InfoText.enabled = false;
        ImageText.enabled = false;
        ToolTipGameObj.SetActive(false);

        _item = slot.Item;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InfoText.enabled = false;
        ImageText.enabled = false;
        ToolTipGameObj.SetActive(false);
        _parent = transform.parent;

        if (!_canvas)
        {
            _canvas = GetComponentInParent<Canvas>();
            _raycaster = _canvas.GetComponent<GraphicRaycaster>();
        }

        // Change parent of our item to the canvas
        transform.SetParent(_canvas.transform, true);

        // And set it as last child to be rendered on top of UI
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        InfoText.enabled = false;
        ImageText.enabled = false;
        ToolTipGameObj.SetActive(false);


        // Set the position of the dragged object to the mouse position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, Input.mousePosition, _canvas.worldCamera, out Vector2 localPoint);
        transform.localPosition = localPoint;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        InfoText.enabled = false;
        ImageText.enabled = false;
        ToolTipGameObj.SetActive(false);


        // Find objects within canvas
        var results = new List<RaycastResult>();
        _raycaster.Raycast(eventData, results);
        foreach (var hit in results)
        {
            if (hit.gameObject.name == "InventoryPlayer")
            {
                PlayerInventory.BuyItem();
                ShopInventory.SellItem();
            }
            else if (hit.gameObject.name == "InventoryShop")
            {
                ShopInventory.BuyItem();
                PlayerInventory.SellItem();
            }
        }

        // Find scene objects            
        RaycastHit2D hitData = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

        if (hitData)
        {
            var consumer = hitData.collider.GetComponent<IConsume>();
            bool consumable = _item is ConsumableItem;

            if ((consumer != null) && consumable)
            {
                (_item as ConsumableItem).Use(consumer);
                if (_playerInventoryUI)
                {
                    _playerInventoryUI.ItemUsed(_item);
                }
                else if (_shopInventoryUI)
                {
                    _shopInventoryUI.ItemUsed(_item);
                }
            }
        }

        // Changing parent back to slot
        transform.SetParent(_parent.transform);

        // And centering item position
        transform.localPosition = Vector3.zero;
    }

    // If the mouse is over the UI element and the mouse is not being dragged then the UI element enables name text

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isMouseOver)
        {
            _isMouseOver = true;
            InfoText.enabled = true;
            ImageText.enabled = true;
            ToolTipGameObj.SetActive(true);

            MouseFollows = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isMouseOver = false;
        InfoText.enabled = false;
        ImageText.enabled = false;
        ToolTipGameObj.SetActive(false);
        MouseFollows = false;
    }

    // If the UI element is clicked then the UI element is selected
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_selectedItem != null)
        {
            _selectedItem.SelectedImage.SetActive(false);
        }

        // Select this item
        _selectedItem = this;
        SelectedImage.SetActive(true);
    }

    public static void DeselectAllItems()
    {
        if (_selectedItem != null)
        {
            _selectedItem.SelectedImage.SetActive(false);
            _selectedItem = null;
        }
    }

    public static BaseItem GetSelectedItem()
    {
        if (_selectedItem != null)
        {
            return _selectedItem._item;
        }
        else
        {
            return null;
        }
    }

    public static InventoryLocation GetSlotLocation()
    {
        if (_selectedItem == null)
        {
            return InventoryLocation.Null;
        }
        else if (_selectedItem._playerInventoryUI)
        {
            return InventoryLocation.Player;
        }
        else if (_selectedItem._shopInventoryUI)
        {
            return InventoryLocation.Shop;
        }

        return InventoryLocation.Null;
    }
}