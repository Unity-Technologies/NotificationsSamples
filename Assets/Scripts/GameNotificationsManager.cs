using System.Collections.Generic;
#if UNITY_ANDROID
using NotificationSamples.Android;
#elif UNITY_IOS
using NotificationSamples.iOS;
#endif
using UnityEngine;

namespace NotificationSamples
{
	/// <summary>
	/// Global notifications manager that serves as a wrapper for multiple platforms' notification systems.
	/// </summary>
	public class GameNotificationsManager : MonoBehaviour
	{
		/// <summary>
		/// Gets the implementation of the notifications for the current platform;
		/// </summary>
		public IGameNotificationsPlatform Platform { get; private set; }
		
		/// <summary>
		/// Gets a collection of notifications that are scheduled.
		/// </summary>
		public List<PendingNotification> ScheduledNotifications { get; private set; } 

		/// <summary>
		/// Initialize the correct platform
		/// </summary>
		protected virtual void Awake()
		{
#if UNITY_ANDROID
			Platform = new AndroidNotificationsPlatform();
#elif UNITY_IOS
			Platform = new IosNotificationsPlatform();
#endif

			ScheduledNotifications = new List<PendingNotification>();
		}

		/// <summary>
		/// Creates a new notification object for the current platform.
		/// </summary>
		/// <returns>The new notification, ready to be scheduled, or null if there's no valid platform.</returns>
		public IGameNotification CreateNotification()
		{
			return Platform?.CreateNotification();
		}

		/// <summary>
		/// Schedules a notification to be delivered.
		/// </summary>
		/// <param name="notification">The notification to deliver.</param>
		public void ScheduleNotification(IGameNotification notification)
		{
			if (notification == null)
			{
				return;
			}

			Platform?.ScheduleNotification(notification);

			// Register pending notification
			ScheduledNotifications.Add(new PendingNotification(notification));
		}

		/// <summary>
		/// Cancels a scheduled notification.
		/// </summary>
		/// <param name="notificationId">The ID of the notification to cancel.</param>
		public void CancelNotification(int notificationId)
		{
			Platform?.CancelNotification(notificationId);
			
			// Remove the cancelled notification from scheduled list
			int index = ScheduledNotifications.FindIndex(notification => notification.Id == notificationId);

			if (index >= 0)
			{
				ScheduledNotifications.RemoveAt(index);
			}
		}

		/// <summary>
		/// Cancels all scheduled notifications.
		/// </summary>
		public void CancelAllNotifications()
		{
			Platform?.CancelAllScheduledNotifications();
			
			ScheduledNotifications.Clear();
		}

		/// <summary>
		/// Dismisses a displayed notification.
		/// </summary>
		/// <param name="notificationId">The ID of the notification to dismiss.</param>
		public void DismissNotification(int notificationId)
		{
			Platform?.DismissNotification(notificationId);
		}

		/// <summary>
		/// Dismisses all displayed notifications.
		/// </summary>
		public void DismissAllNotifications()
		{
			Platform.DismissAllDisplayedNotifications();
		}
	}
}