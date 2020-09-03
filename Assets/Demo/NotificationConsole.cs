using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NotificationSamples.Demo
{
    /// <summary>
    /// Manages the console on screen that displays information about notifications,
    /// and allows you to schedule more.
    /// </summary>
    public class NotificationConsole : MonoBehaviour
    {
        // On iOS, this represents the notification's Category Identifier, and is optional
        // On Android, this represents the notification's channel, and is required (at least one).
        // Channels defined as global constants so can be referred to from GameController.cs script when setting/sending notification
        public const string ChannelId = "game_channel0";
        public const string ReminderChannelId = "reminder_channel1";
        public const string NewsChannelId = "news_channel2";

        [SerializeField]
        protected Button sendButton;

        [SerializeField]
        protected Button clearEventButton;

        [SerializeField]
        protected TMP_InputField titleField;

        [SerializeField]
        protected TMP_InputField bodyField;

        [SerializeField]
        protected TMP_InputField timeField;

        [SerializeField]
        protected TMP_InputField badgeField;

        [SerializeField]
        protected GameNotificationsManager manager;

        [SerializeField]
        protected Transform pendingNotificationsListParent;

        [SerializeField]
        protected PendingNotificationItem pendingNotificationPrefab;

        [SerializeField]
        protected Transform pendingEventParent;

        [SerializeField]
        protected NotificationEventItem eventPrefab;

        // Update pending notifications in the next update.
        private bool updatePendingNotifications;

        private void Awake()
        {
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendNotificationFromUi);
            }

            if (clearEventButton != null)
            {
                clearEventButton.onClick.AddListener(ClearEvents);
            }
        }

        private void Start()
        {
            // Set up channels (mostly for Android)
            // You need to have at least one of these
            var c1 = new GameNotificationChannel(ChannelId, "Default Game Channel", "Generic notifications");
            var c2 = new GameNotificationChannel(NewsChannelId, "News Channel", "News feed notifications");
            var c3 = new GameNotificationChannel(ReminderChannelId, "Reminder Channel", "Reminder notifications");

            manager.Initialize(c1, c2, c3);
        }

        private void OnDestroy()
        {
            if (sendButton != null)
            {
                sendButton.onClick.RemoveListener(SendNotificationFromUi);
            }

            if (clearEventButton != null)
            {
                clearEventButton.onClick.RemoveListener(ClearEvents);
            }
        }

        private void OnEnable()
        {
            if (manager != null)
            {
                manager.LocalNotificationDelivered += OnDelivered;
                manager.LocalNotificationExpired += OnExpired;
            }
        }

        private void OnDisable()
        {
            if (manager != null)
            {
                manager.LocalNotificationDelivered -= OnDelivered;
                manager.LocalNotificationExpired -= OnExpired;
            }
        }

        /// <summary>
        /// Queue a notification with the given parameters.
        /// </summary>
        /// <param name="title">The title for the notification.</param>
        /// <param name="body">The body text for the notification.</param>
        /// <param name="deliveryTime">The time to deliver the notification.</param>
        /// <param name="badgeNumber">The optional badge number to display on the application icon.</param>
        /// <param name="reschedule">
        /// Whether to reschedule the notification if foregrounding and the notification hasn't yet been shown.
        /// </param>
        /// <param name="channelId">Channel ID to use. If this is null/empty then it will use the default ID. For Android
        /// the channel must be registered in <see cref="GameNotificationsManager.Initialize"/>.</param>
        /// <param name="smallIcon">Notification small icon.</param>
        /// <param name="largeIcon">Notification large icon.</param>
        public void SendNotification(string title, string body, DateTime deliveryTime, int? badgeNumber = null,
            bool reschedule = false, string channelId = null,
            string smallIcon = null, string largeIcon = null)
        {
            IGameNotification notification = manager.CreateNotification();

            if (notification == null)
            {
                return;
            }

            notification.Title = title;
            notification.Body = body;
            notification.Group = !string.IsNullOrEmpty(channelId) ? channelId : ChannelId;
            notification.DeliveryTime = deliveryTime;
            notification.SmallIcon = smallIcon;
            notification.LargeIcon = largeIcon;
            if (badgeNumber != null)
            {
                notification.BadgeNumber = badgeNumber;
            }

            PendingNotification notificationToDisplay = manager.ScheduleNotification(notification);
            notificationToDisplay.Reschedule = reschedule;
            updatePendingNotifications = true;

            QueueEvent($"Queued event with ID \"{notification.Id}\" at time {deliveryTime:HH:mm}");
        }

        /// <summary>
        /// Cancel a given pending notification
        /// </summary>
        public void CancelPendingNotificationItem(PendingNotification itemToCancel)
        {
            manager.CancelNotification(itemToCancel.Notification.Id.Value);

            updatePendingNotifications = true;

            QueueEvent($"Cancelled notification with ID \"{itemToCancel.Notification.Id}\"");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                StartCoroutine(UpdatePendingNotificationsNextFrame());
            }
        }

        private void SendNotificationFromUi()
        {
            int? badgeNumber = int.TryParse(badgeField.text, out int parsedBadgeNumber)
                ? parsedBadgeNumber
                : (int?)null;

            DateTime deliveryTime;
            if (float.TryParse(timeField.text, out float minutes))
            {
                deliveryTime = DateTime.Now.ToLocalTime() + TimeSpan.FromMinutes(minutes);
            }
            else
            {
                deliveryTime = DateTime.Now.ToLocalTime() + TimeSpan.FromMinutes(1);
            }

            SendNotification(titleField.text, bodyField.text, deliveryTime, badgeNumber);
        }

        private void OnDelivered(PendingNotification deliveredNotification)
        {
            // Schedule this to run on the next frame (can't create UI elements from a Java callback)
            StartCoroutine(ShowDeliveryNotificationCoroutine(deliveredNotification.Notification));
        }

        private void OnExpired(PendingNotification obj)
        {
            QueueEvent($"Notification with title \"{obj.Notification.Title}\" expired and was not displayed.");
        }

        private IEnumerator ShowDeliveryNotificationCoroutine(IGameNotification deliveredNotification)
        {
            yield return null;

            QueueEvent($"Notification with ID \"{deliveredNotification.Id}\" shown in foreground.");

            updatePendingNotifications = true;
        }

        private IEnumerator UpdatePendingNotificationsNextFrame()
        {
            yield return null;
            updatePendingNotifications = true;
        }

        private void Update()
        {
            if (updatePendingNotifications)
            {
                UpdatePendingNotifications();
            }
        }

        private void UpdatePendingNotifications()
        {
            Debug.Log("Updating pending list.");

            updatePendingNotifications = false;

            // Clear all existing ones
            foreach (object child in pendingNotificationsListParent.transform)
            {
                if (child is Transform childTransform)
                {
                    Destroy(childTransform.gameObject);
                }
            }

            if (manager?.PendingNotifications == null)
                return;

            // Recreate based on currently pending list
            // Note: Using ToArray because the list can change during the loop.
            foreach (PendingNotification scheduledNotification in manager.PendingNotifications.ToArray())
            {
                PendingNotificationItem newItem =
                    Instantiate(pendingNotificationPrefab, pendingNotificationsListParent);
                newItem.Show(scheduledNotification, this);
            }
        }

        private void QueueEvent(string eventText)
        {
            Debug.Log($"Queueing event with text \"{eventText}\"");

            NotificationEventItem newItem = Instantiate(eventPrefab, pendingEventParent);
            newItem.Show(eventText);
        }

        private void ClearEvents()
        {
            foreach (object child in pendingEventParent.transform)
            {
                if (child is Transform childTransform)
                {
                    Destroy(childTransform.gameObject);
                }
            }
        }
    }
}
