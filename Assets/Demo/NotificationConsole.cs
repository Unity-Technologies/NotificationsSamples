using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
#if UNITY_ANDROID
using NotificationSamples.Android;
using Unity.Notifications.Android;
#endif

namespace NotificationSamples.Demo
{
	/// <summary>
	/// Manages the console on screen that displays information about notifications,
	/// and allows you to schedule more.
	/// </summary>
	public class NotificationConsole : MonoBehaviour
	{
		// Default channel ID.
		private const string ChannelId = "game_channel";
		
		// News channel ID.
		public const string NewsChannelId = "news_channel";

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
#if UNITY_ANDROID
			RegisterAndroidChannels();
#endif
		}
		
#if UNITY_ANDROID
		private void RegisterAndroidChannels()
		{
			// Initialize Android channels
			var c1 = new AndroidNotificationChannel()
			{
				Id = ChannelId,
				Name = "Default Game Channel",
				CanShowBadge = true,
				Importance = Importance.High,
				Description = "Generic notifications",
			};
			AndroidNotificationCenter.RegisterNotificationChannel(c1);
	
			var c2 = new AndroidNotificationChannel()
			{
				Id = NewsChannelId,
				Name = "News Channel",
				CanShowBadge = true,
				Importance = Importance.High,
				Description = "News feed notifications",
			};
			AndroidNotificationCenter.RegisterNotificationChannel(c2);

			if (manager.Platform is AndroidNotificationsPlatform androidPlatform)
			{
				androidPlatform.DefaultChannelId = ChannelId;
			}
		}
#endif

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
			}
		}

		private void OnDisable()
		{
			if (manager != null)
			{
				manager.LocalNotificationDelivered -= OnDelivered;
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
		/// the channel must be registered in <see cref="RegisterAndroidChannels"/>.</param>
		public void SendNotification(string title, string body, DateTime deliveryTime, int? badgeNumber = null,
		                             bool reschedule = false, string channelId = null)
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
			if (!itemToCancel.Notification.Scheduled)
			{
				return;
			}

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
				deliveryTime = DateTime.Now + TimeSpan.FromMinutes(minutes);
			}
			else
			{
				deliveryTime = DateTime.Now + TimeSpan.FromMinutes(1);
			}

			SendNotification(titleField.text, bodyField.text, deliveryTime, badgeNumber);
		}

		private void OnDelivered(PendingNotification deliveredNotification)
		{
			// Schedule this to run on the next frame (can't create UI elements from a Java callback)
			StartCoroutine(ShowDeliveryNotificationCoroutine(deliveredNotification.Notification));
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
