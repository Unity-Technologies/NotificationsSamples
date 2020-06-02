using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NotificationSamples.Demo
{
    /// <summary>
    /// UI list item for purchased inventory items.
    /// </summary>
    public class InventoryItem : MonoBehaviour
    {
        [SerializeField, Tooltip("Label to store the quantity bought.")]
        protected TextMeshProUGUI quantityLabel;
        [SerializeField]
        protected Image icon;

        // Quantity of items bought.
        private int quantity;

        /// <summary>
        /// The item data linked to this UI item.
        /// </summary>
        public InventoryItemData ItemData { get; private set; }

        /// <summary>
        /// Initialise the item with the specified settings.
        /// </summary>
        public void Initialise(InventoryItemData itemData)
        {
            ItemData = itemData;
            icon.sprite = itemData.Icon;
            quantity = 1;
            UpdateControls();
        }

        /// <summary>
        /// Called when another item of the same type was received after buying.
        /// </summary>
        public void OnReceivedItem()
        {
            quantity++;
            UpdateControls();
        }

        // Update UI controls.
        private void UpdateControls()
        {
            quantityLabel.text = quantity.ToString();
        }
    }
}
