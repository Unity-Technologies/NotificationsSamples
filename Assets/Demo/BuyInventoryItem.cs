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
		
		// Fired when the buy button is pressed.
		private Action<BuyInventoryItem> bought;

		/// <summary>
		/// Title to show in the notification.
		/// </summary>
		public string Title => titleLabel.text;

		/// <summary>
		/// Description to show in the notification.
		/// </summary>
		public string Description { get; private set; }
		
		/// <summary>
		/// Show this number on the badge.
		/// </summary>
		public int BadgeNumber { get; private set; }
		
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
			BadgeNumber = 1;
			bought = buyAction;
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
		/// Update UI controls.
		/// </summary>
		public void UpdateControls()
		{
			costLabel.text = Cost.ToString("N0");
			timeLabel.text = $"Takes {Minutes} minute(s)";
		}
	}
}
