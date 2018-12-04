using System;
using TMPro;
using UnityEngine;

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

		public void SendNotification()
		{
			IGameNotification notification = manager.CreateNotification();

			if (notification == null)
			{
				return;
			}

			notification.Title = titleField.text;
			notification.Body = bodyField.text;
			notification.Group = ChannelId;
			if (int.TryParse(badgeField.text, out int badgeNumber))
			{
				notification.BadgeNumber = badgeNumber;
			}

			if (float.TryParse(timeField.text, out float minutes))
			{
				notification.DeliveryTime = DateTime.Now + TimeSpan.FromMinutes(minutes);
			}
			else
			{
				notification.DeliveryTime = DateTime.Now + TimeSpan.FromMinutes(1);
			}

			manager.ScheduleNotification(notification);

			UpdatePendingNotifications();
		}

		public void SendNotification(CreateNotificationItem fromItem)
		{
			IGameNotification notification = manager.CreateNotification();
			if (notification == null)
			{
				return;
			}
			
			notification.Title = fromItem.Title;
			notification.Body = fromItem.Description;
			notification.Group = ChannelId;
			notification.BadgeNumber = fromItem.BadgeNumber;
			notification.DeliveryTime = DateTime.Now + TimeSpan.FromMinutes(fromItem.Minutes);

			manager.ScheduleNotification(notification);

			UpdatePendingNotifications();
		}

		/// <summary>
		/// Cancel a given pending notification
		/// </summary>
		public void CancelPendingNotificationItem(PendingNotification itemToCancel)
		{
			manager.CancelNotification(itemToCancel.Id);

			UpdatePendingNotifications();
		}

		// Update pending list
		private void UpdatePendingNotifications()
		{
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
	}
}
