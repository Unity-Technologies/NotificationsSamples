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
					return Mathf.Max((float)timeSpan.TotalSeconds, 0.0f);
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
		
		[SerializeField, Tooltip("Label to display the currency bonus.")]
		protected TextMeshProUGUI bonusLabel;
		
		[SerializeField, Tooltip("Label to display the current time.")]
		protected TextMeshProUGUI timeLabel;
		
		[SerializeField, Tooltip("Start the game with this currency.")]
		protected float initialCurrency = 100.0f;
		
		[SerializeField, Tooltip("Inventory items data to use in the game.")]
		protected InventoryItemData[] itemsData;

		// Current currency.
		private float currency;

		// Currency bonus per second.
		private float currencyBonus;

		// DateTime when the application was paused.
		private DateTime? applicationPausedTime;

		// Inventory item bought, and will be received at the specified delivery time.
		private readonly List<PendingInventoryItem> pendingItems = new List<PendingInventoryItem>();

		// Bought items displayed in the status area
		private readonly Dictionary<InventoryItemData, InventoryItem> items = new Dictionary<InventoryItemData, InventoryItem>();
		
		// Keep track of the items to buy
		private readonly List<BuyInventoryItem> buyItems = new List<BuyInventoryItem>();
		
		private void Awake()
		{
			// Create the buy items
			for (int i = 0, len = itemsData.Length; i < len; i++)
			{
				InventoryItemData data = itemsData[i];
				BuyInventoryItem item = Instantiate(buyItemPrefab, buyContentHolder);
				item.Initialise(data, OnBuy);
				buyItems.Add(item);
			}

			SetCurrency(initialCurrency);
			UpdateControls();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				applicationPausedTime = DateTime.Now;
			}
			else if (applicationPausedTime != null)
			{
				// Award currency bonus which accumulated while the app was paused
				TimeSpan timeSpan = DateTime.Now - applicationPausedTime.Value;
				float pausedTime = Mathf.Max((float)timeSpan.TotalSeconds, 0.0f);
				applicationPausedTime = null;
				AwardCurrencyBonus(pausedTime);
			}
		}

		// Increase the currency by (currency bonus * elapsed time).
		private void AwardCurrencyBonus(float elapsedTime)
		{
			SetCurrency(currency + currencyBonus * elapsedTime);
		}

		// Called when an item's buy button is clicked.
		private void OnBuy(BuyInventoryItem item)
		{
			if (currency < item.Cost)
			{
				return;
			}

			DateTime deliveryTime = DateTime.Now + TimeSpan.FromMinutes(item.Minutes);
			console.SendNotification(item.Title, item.Description, deliveryTime, item.BadgeNumber, true);
			
			pendingItems.Add(new PendingInventoryItem
			{
				DeliveryTime = deliveryTime,
				ItemData = item.ItemData
			});
			
			// Store the item's cost before deducting it, so item can be updated in SetCurrency based on its new cost
			int cost = item.Cost;
			item.Cost += item.ItemData.CostIncrease;
			item.Minutes += item.ItemData.CreationTimeIncrease;
			item.OnBuySuccess(deliveryTime);
			
			SetCurrency(currency - cost);
			UpdateControls();
		}

		// Update UI controls
		private void UpdateControls()
		{
			int currencyInt = (int)currency;
			currencyLabel.text = currencyInt.ToString("N0");
			bonusLabel.text = $"(+{currencyBonus:f1}/s)";
			timeLabel.text = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
		}

		private void Update()
		{
			UpdatePendingItems();
			AwardCurrencyBonus(Time.deltaTime);
			UpdateControls();
		}

		// Check if pending items must be received
		private void UpdatePendingItems()
		{
			for (int i = pendingItems.Count - 1; i >= 0; i--)
			{
				PendingInventoryItem pendingItem = pendingItems[i];
				if (pendingItem == null || pendingItem.TimeRemaining > 0.0f)
				{
					continue;
				}
				currencyBonus += pendingItem.ItemData.CurrencyBonus;
				OnDeliveredItem(pendingItem.ItemData);
				pendingItems.RemoveAt(i);
			}
		}

		// Called when an item was delivered.
		private void OnDeliveredItem(InventoryItemData itemData)
		{
			// Add the item to the stats area.
			if (items.TryGetValue(itemData, out InventoryItem item))
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
			
			// Let buy items know an item was delivered
			for (int i = 0, len = buyItems.Count; i < len; i++)
			{
				BuyInventoryItem buyItem = buyItems[i];
				if (buyItem == null)
				{
					continue;
				}
				buyItem.OnDeliveredItem(itemData);
			}
		}

		private void SetCurrency(float newCurrency)
		{
			int oldCurrency = (int)currency;
			currency = newCurrency;
			if (oldCurrency == (int)currency || buyItems.Count <= 0)
			{
				return;
			}
			
			// Let buy items know the currency changed
			int changedCurrency = (int)currency;
			for (int i = 0, len = buyItems.Count; i < len; i++)
			{
				BuyInventoryItem item = buyItems[i];
				if (item == null)
				{
					continue;
				}
				item.OnCurrencyChanged(changedCurrency);
			}
		}
	}
}
