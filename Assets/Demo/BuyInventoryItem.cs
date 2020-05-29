using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NotificationSamples.Demo
{
    /// <summary>
    /// UI list item for buying an inventory item.
    /// </summary>
    public class BuyInventoryItem : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI titleLabel;
        [SerializeField]
        protected TextMeshProUGUI timeLabel;
        [SerializeField]
        protected TextMeshProUGUI costLabel;
        [SerializeField]
        protected Image icon;
        [SerializeField]
        protected Button buyButton;
        [SerializeField]
        protected Image progressImage;

        // Fired when the buy button is pressed.
        private Action<BuyInventoryItem> bought;

        // When the last bought item will be delivered.
        private DateTime? deliveryTime;

        // Initial time remaining for the delivery of the last bought item.
        private float initialTimeRemaining;

        // The last currency received from the game controller.
        private int currency;

        /// <summary>
        /// Title to show in the notification.
        /// </summary>
        public string Title => titleLabel.text;

        /// <summary>
        /// Description to show in the notification.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Fire notification after this amount of minutes.
        /// </summary>
        public float Minutes { get; set; }

        /// <summary>
        /// The item's cost.
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// The item data linked to this UI item.
        /// </summary>
        public InventoryItemData ItemData { get; private set; }

        /// <summary>
        /// Initialise the item with the specified settings.
        /// </summary>
        public void Initialise(InventoryItemData itemData, Action<BuyInventoryItem> buyAction)
        {
            ItemData = itemData;
            titleLabel.text = itemData.Title;
            Minutes = itemData.InitialCreationTime;
            Description = itemData.Description;
            Cost = itemData.InitialCost;
            icon.sprite = itemData.Icon;
            bought = buyAction;
            deliveryTime = null;
            progressImage.fillAmount = 0.0f;
            UpdateControls();
        }

        /// <summary>
        /// Called when the buy button is clicked.
        /// </summary>
        public void OnBuy()
        {
            bought?.Invoke(this);
        }

        /// <summary>
        /// Called when buying an item was successful.
        /// </summary>
        public void OnBuySuccess(DateTime boughtDeliveryTime)
        {
            deliveryTime = boughtDeliveryTime;
            initialTimeRemaining = GetTimeRemaining();
            buyButton.interactable = false;
            UpdateControls();
        }

        /// <summary>
        /// Update UI controls.
        /// </summary>
        public void UpdateControls()
        {
            if (deliveryTime != null)
            {
                // Do not update the labels (i.e. show the old values until the item is delivered).
                return;
            }
            costLabel.text = Cost.ToString("N0");
            timeLabel.text = $"Takes {Minutes:F1} minute(s)";
        }

        /// <summary>
        /// Called when the game's currency total changed.
        /// </summary>
        public void OnCurrencyChanged(int newCurrency)
        {
            currency = newCurrency;
            UpdateBuyButton();
        }

        /// <summary>
        /// Called when an item was delivered.
        /// </summary>
        public void OnDeliveredItem(InventoryItemData itemData)
        {
            if (itemData == ItemData)
            {
                UpdateBuyButton();
            }
        }

        // Time remaining until the last bought item is delivered.
        private float GetTimeRemaining()
        {
            if (deliveryTime == null)
            {
                return 0.0f;
            }
            TimeSpan timeSpan = deliveryTime.Value - DateTime.Now;
            return Mathf.Max((float)timeSpan.TotalSeconds, 0.0f);
        }

        private void Update()
        {
            UpdateProgress();
        }

        // Determine if the buy button should be interactable.
        private void UpdateBuyButton()
        {
            buyButton.interactable = currency >= Cost && GetTimeRemaining() <= 0.0f;
        }

        // Update an item's delivery progress, and update the progress bar.
        private void UpdateProgress()
        {
            if (deliveryTime == null)
            {
                return;
            }
            float timeRemaining = GetTimeRemaining();
            if (timeRemaining > 0.0f)
            {
                progressImage.fillAmount = 1.0f - Mathf.Clamp01(timeRemaining / initialTimeRemaining);
                return;
            }
            progressImage.fillAmount = 0.0f;
            deliveryTime = null;
            UpdateControls();
        }
    }
}
