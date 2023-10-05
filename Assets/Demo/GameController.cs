using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NotificationSamples.Demo
{
    /// <summary>
    /// Keeps track of the game currency and inventory items.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        // Space (pixels) to use in the inspector between groups of fields.
        private const float DefaultInspectorSpace = 10.0f;

        // Max length of the news feed title.
        private const int MaxNewsFeedTitleLength = 50;

        // Max length of the news feed summary.
        private const int MaxNewsFeedSummaryLength = 150;

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

        [SerializeField, Tooltip("Reference to the notification manager.")]
        protected GameNotificationsManager notificationsManager;

        [SerializeField, Tooltip("For reading the news feed items.")]
        protected NewsFeedReader newsFeedReader;

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

        [SerializeField, Tooltip("Label to display log messages.")]
        protected TextMeshProUGUI logLabel;

        [SerializeField, Tooltip("Loading icon to show the news feed is being loaded.")]
        protected GameObject newsFeedLoadingIcon;

        [SerializeField, Tooltip("Turn speed of the news feed loading icon.")]
        protected float newsFeedLoadingIconTurnSpeed = 100.0f;

        [SerializeField, Tooltip("News feed button.")]
        protected Button newsFeedButton;

        [Space(DefaultInspectorSpace)]
        [SerializeField, Tooltip("Start the game with this currency.")]
        protected float initialCurrency = 100.0f;

        [SerializeField, Tooltip("Get the news feed (RSS) from this url.")]
        protected string newsFeedUrl = "https://unity3d.com/news.rss";

        [SerializeField, Tooltip("Schedule news feed notifications this time in the future (minutes).")]
        protected float newsNotificationTime = 5.0f;

        [SerializeField, Tooltip("Schedule a reminder to play the game at this hour (e.g. 6:00, 13:00, etc.).")]
        protected int playReminderHour = 6;

        [Space(DefaultInspectorSpace)]
        [SerializeField, Tooltip("Inventory items data to use in the game.")]
        protected InventoryItemData[] itemsData;

        // Current currency.
        private float currency;

        // Currency bonus per second.
        private float currencyBonus;

        // DateTime when the application was paused.
        private DateTime? applicationPausedTime;

        // Inventory items bought, and will be received at the specified delivery times.
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
            ShowNewsFeedLoadingIcon(false);
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

        private void Update()
        {
            float dt = Time.deltaTime;
            UpdatePendingItems();
            AwardCurrencyBonus(dt);
            UpdateControls();
            UpdateNewsFeedLoadingIcon(dt);
        }

        /// <summary>
        /// Called when the news feed button is pressed.
        /// </summary>
        public void OnNewsFeed()
        {
            ShowNewsFeedLoadingIcon(true);
            newsFeedReader.GetFirstItem(newsFeedUrl, OnGetFirstItem);
        }

        /// <summary>
        /// Called when the play reminder button is pressed.
        /// </summary>
        public void OnPlayReminder()
        {
            // Schedule a reminder to play the game. Schedule it for the next day.
            DateTime deliveryTime = DateTime.Now.ToLocalTime().AddDays(1);
            deliveryTime = new DateTime(deliveryTime.Year, deliveryTime.Month, deliveryTime.Day, playReminderHour, 0, 0,
                DateTimeKind.Local);

            console.SendNotification("Cookie Reminder", "Remember to make more cookies!", deliveryTime);
        }

        /// <summary>
        /// Called when the display last notification button is pressed.
        /// </summary>
        public void DisplayLastNotification()
        {
            var lastNotification = notificationsManager.GetLastNotification();
            if (lastNotification == null)
            {
                logLabel.text = "No notification has been opened yet";
                return;
            }

            logLabel.text = $"id: \"{lastNotification.Id}\"{Environment.NewLine}" +
                            $"title: \"{lastNotification.Title}\"{Environment.NewLine}" +
                            $"body: \"{lastNotification.Body}\"{Environment.NewLine}";
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

            DateTime deliveryTime = DateTime.Now.ToLocalTime() + TimeSpan.FromMinutes(item.Minutes);
            console.SendNotification(item.Title, item.Description, deliveryTime, reschedule: true);

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

        private void UpdateNewsFeedLoadingIcon(float dt)
        {
            if (newsFeedLoadingIcon == null || !newsFeedLoadingIcon.activeInHierarchy)
            {
                return;
            }
            newsFeedLoadingIcon.transform.Rotate(0.0f, 0.0f, -newsFeedLoadingIconTurnSpeed * dt);
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

        // Show/hide the news feed loading icon. It also makes the news button non-interactable when the icon is visible.
        private void ShowNewsFeedLoadingIcon(bool visible)
        {
            newsFeedLoadingIcon.SetActive(visible);
            newsFeedButton.interactable = !visible;
        }

        // Create a notification from the news feed item.
        private void OnGetFirstItem(NewsFeedReader.NewsItem newsItem)
        {
            ShowNewsFeedLoadingIcon(false);
            if (newsItem == null)
            {
                Debug.LogError("Could not get the news feed item.");
                return;
            }

            string title = newsItem.Title;
            string body = newsItem.Description;
            if (!string.IsNullOrEmpty(title) && title.Length > MaxNewsFeedTitleLength)
            {
                title = title.Substring(0, MaxNewsFeedTitleLength);
            }
            if (!string.IsNullOrEmpty(body) && body.Length > MaxNewsFeedSummaryLength)
            {
                body = body.Substring(0, MaxNewsFeedSummaryLength);
            }

            DateTime deliveryTime = DateTime.Now.ToLocalTime() + TimeSpan.FromMinutes(newsNotificationTime);
            console.SendNotification(title, body, deliveryTime);
        }
    }
}
