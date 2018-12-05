using System;
using System.Collections;
using TMPro;
using UnityEngine;
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
		private const string ChannelId = "game_channel";

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
			// Initialize Android channel
			var c = new AndroidNotificationChannel()
			{
				Id = ChannelId,
				Name = "Default Game Channel",
				CanShowBadge = true,
				Importance = Importance.Default,
				Description = "Generic notifications",
			};
			AndroidNotificationCenter.RegisterNotificationChannel(c);

			if (manager.Platform is AndroidNotificationsPlatform androidPlatform)
			{
				androidPlatform.DefaultChannelId = ChannelId;
			}
#endif
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
		/// Queue a notification with the given parameters
		/// </summary>
		/// <param name="title">The title for the notification.</param>
		/// <param name="body">The body text for the notification.</param>
		/// <param name="deliveryTime">The time to deliver the notification.</param>
		/// <param name="badgeNumber">The optional badge number to display on the application icon.</param>
		public void SendNotification(string title, string body, DateTime deliveryTime, int? badgeNumber = null)
		{
			IGameNotification notification = manager.CreateNotification();

			if (notification == null)
			{
				return;
			}

			notification.Title = title;
			notification.Body = body;
			notification.Group = ChannelId;
			notification.DeliveryTime = deliveryTime;
			if (badgeNumber != null)
			{
				notification.BadgeNumber = badgeNumber;
			}

			manager.ScheduleNotification(notification);
			UpdatePendingNotifications();

			QueueEvent($"Queued event with ID \"{notification.Id}\" at time {deliveryTime:HH:mm}");
		}

		/// <summary>
		/// Cancel a given pending notification
		/// </summary>
		public void CancelPendingNotificationItem(PendingNotification itemToCancel)
		{
			manager.CancelNotification(itemToCancel.Id);

			UpdatePendingNotifications();
			
			QueueEvent($"Cancelled notification with ID \"{itemToCancel.Id}\"");
		}

		private void SendNotificationFromUi()
		{
			int? badgeNumber = int.TryParse(badgeField.text, out int parsedBadgeNumber) ? parsedBadgeNumber : (int?)null;

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

		private void OnDelivered(IGameNotification deliveredNotification)
		{
			// Schedule this to run on the next frame (can't create UI elements from a Java callback)
			StartCoroutine(ShowDeliveryNotificationCoroutine(deliveredNotification));
		}

		private IEnumerator ShowDeliveryNotificationCoroutine(IGameNotification deliveredNotification)
		{
			yield return null;
			
			QueueEvent($"Notification with ID \"{deliveredNotification.Id}\" shown in foreground.");
			
			UpdatePendingNotifications();
		}

		private void UpdatePendingNotifications()
		{
			Debug.Log("Updating pending list.");
				
			// Clear all existing ones
			foreach (object child in pendingNotificationsListParent.transform)
			{
				if (child is Transform childTransform)
				{
					Destroy(childTransform.gameObject);
				}
			}

			// Recreate based on currently pending list
			foreach (PendingNotification scheduledNotification in manager.ScheduledNotifications)
			{
				PendingNotificationItem newItem = Instantiate(pendingNotificationPrefab, pendingNotificationsListParent);
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
