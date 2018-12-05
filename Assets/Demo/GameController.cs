using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NotificationSamples.Demo
{
	/// <summary>
	/// Keeps track of the game currency and inventory items.
	/// </summary>
	public class GameController : MonoBehaviour
	{
		// Inventory item bought, and will be received at the specified delivery time.
		private class PendingInventoryItem
		{
			/// <summary>
			/// When the item will be delivered.
			/// </summary>
			public DateTime DeliveryTime;
			
			/// <summary>
			/// The item's data.
			/// </summary>
			public InventoryItemData ItemData;

			/// <summary>
			/// Time remaining until it is delivered.
			/// </summary>
			public float TimeRemaining
			{
				get
				{
					TimeSpan timeSpan = DeliveryTime - DateTime.Now;
					return (float)timeSpan.TotalSeconds;
				}
			}
		}
		
		[SerializeField, Tooltip("Reference to the notification console.")]
		protected NotificationConsole console;
		
		[SerializeField, Tooltip("Buy item prefab to clone.")]
		protected BuyInventoryItem buyItemPrefab;
		
		[SerializeField, Tooltip("Parent to hold the items to buy.")]
		protected Transform buyContentHolder;

		[SerializeField, Tooltip("Prefab of bought items.")]
		protected InventoryItem itemPrefab;
		
		[SerializeField, Tooltip("Parent to hold the bought items.")]
		protected Transform contentHolder;
		
		[SerializeField, Tooltip("Label to display the currency.")]
		protected TextMeshProUGUI currencyLabel;
		
		[SerializeField, Tooltip("Start the game with this currency.")]
		protected float initialCurrency = 100.0f;
		
		[SerializeField, Tooltip("Inventory items data to use in the game.")]
		protected InventoryItemData[] itemsData;

		// Current currency.
		private float currency;

		// Currency bonus per second.
		private float currencyBonus;

		// Inventory item bought, and will be received at the specified delivery time.
		private List<PendingInventoryItem> pendingItems = new List<PendingInventoryItem>();

		// Bought items displayed in the status area
		private Dictionary<InventoryItemData, InventoryItem> items = new Dictionary<InventoryItemData, InventoryItem>();
		
		private void Awake()
		{
			currency = initialCurrency;
			// Create the buy items
			for (int i = 0, len = itemsData.Length; i < len; i++)
			{
				InventoryItemData data = itemsData[i];
				BuyInventoryItem item = Instantiate(buyItemPrefab, buyContentHolder);
				item.Initialise(data, OnBuy);
			}

			UpdateControls();
		}

		// Called when an item's buy button is clicked.
		private void OnBuy(BuyInventoryItem item)
		{
			if (currency < item.Cost)
			{
				return;
			}

			DateTime deliveryTime = DateTime.Now + TimeSpan.FromMinutes(item.Minutes);
			console.SendNotification(item.Title, item.Description, deliveryTime, item.BadgeNumber);
			currency -= item.Cost;
			
			pendingItems.Add(new PendingInventoryItem
			{
				DeliveryTime = deliveryTime,
				ItemData = item.ItemData
			});
			
			item.Cost += item.ItemData.CostIncrease;
			item.Minutes += item.ItemData.CreationTimeIncrease;
			item.UpdateControls();
			
			UpdateControls();
		}

		// Update UI controls
		private void UpdateControls()
		{
			int currencyInt = (int)currency;
			currencyLabel.text = currencyInt.ToString("N0");
		}

		private void Update()
		{
			// Check if pending items must be received
			for (int i = pendingItems.Count - 1; i >= 0; i--)
			{
				PendingInventoryItem pendingItem = pendingItems[i];
				if (pendingItem == null || pendingItem.TimeRemaining > 0.0f)
				{
					continue;
				}
				currencyBonus += pendingItem.ItemData.CurrencyBonus;
				AddItemToStatus(pendingItem.ItemData);
				pendingItems.RemoveAt(i);
			}
			
			currency += currencyBonus * Time.deltaTime;
			UpdateControls();
		}

		// Add the item to the status area.
		private void AddItemToStatus(InventoryItemData itemData)
		{
			InventoryItem item;
			if (items.TryGetValue(itemData, out item))
			{
				// Update existing item of the same type
				item.OnReceivedItem();
			}
			else
			{
				item = Instantiate(itemPrefab, contentHolder);
				items.Add(itemData, item);
				item.Initialise(itemData);
			}
		}
	}
}
