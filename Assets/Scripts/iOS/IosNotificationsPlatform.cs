#if UNITY_IOS
using System;
using Unity.Notifications.iOS;

namespace NotificationSamples.iOS
{
	/// <summary>
	/// iOS implementation of <see cref="IGameNotificationsPlatform"/>.
	/// </summary>
	public class IosNotificationsPlatform : IGameNotificationsPlatform<IosGameNotification>,
		IDisposable
	{
		/// <inheritdoc />
		public event Action<IGameNotification> NotificationReceived;

		/// <summary>
		/// Instantiate a new instance of <see cref="IosNotificationsPlatform"/>.
		/// </summary>
		public IosNotificationsPlatform()
		{
			iOSNotificationCenter.OnNotificationReceived += OnLocalNotificationReceived;
		}

		/// <inheritdoc />
		public void ScheduleNotification(IGameNotification gameNotification)
		{
			if (gameNotification == null)
			{
				throw new ArgumentNullException(nameof(gameNotification));
			}

			if (!(gameNotification is IosGameNotification iosGameNotification))
			{
				throw new InvalidOperationException(
					"Notification provided to ScheduleNotification isn't an IosGameNotification.");
			}

			ScheduleNotification(iosGameNotification);
		}

		/// <inheritdoc />
		public void ScheduleNotification(IosGameNotification notification)
		{
			if (notification == null)
			{
				throw new ArgumentNullException(nameof(notification));
			}

			iOSNotificationCenter.ScheduleNotification(notification.InternalNotification);
			notification.OnScheduled();
		}

		/// <inheritdoc />
		/// <summary>
		/// Create a new <see cref="T:NotificationSamples.Android.AndroidNotification" />.
		/// </summary>
		IGameNotification IGameNotificationsPlatform.CreateNotification()
		{
			return CreateNotification();
		}

		/// <inheritdoc />
		/// <summary>
		/// Create a new <see cref="T:NotificationSamples.Android.AndroidNotification" />.
		/// </summary>
		public IosGameNotification CreateNotification()
		{
			return new IosGameNotification();
		}

		/// <inheritdoc />
		public void CancelNotification(int notificationId)
		{
			iOSNotificationCenter.RemoveScheduledNotification(notificationId.ToString());
		}

		/// <inheritdoc />
		public void DismissNotification(int notificationId)
		{
			iOSNotificationCenter.RemoveDeliveredNotification(notificationId.ToString());
		}

		/// <inheritdoc />
		public void CancelAllScheduledNotifications()
		{
			iOSNotificationCenter.RemoveAllScheduledNotifications();
		}

		/// <inheritdoc />
		public void DismissAllDisplayedNotifications()
		{
			iOSNotificationCenter.RemoveAllDeliveredNotifications();
		}

		/// <summary>
		/// Clears badge count.
		/// </summary>
		public void OnForeground()
		{
			iOSNotificationCenter.ApplicationBadge = 0;
		}

		/// <summary>
		/// Does nothing on iOS.
		/// </summary>
		public void OnBackground() { }

		/// <summary>
		/// Unregister delegates.
		/// </summary>
		public void Dispose()
		{
			iOSNotificationCenter.OnNotificationReceived -= OnLocalNotificationReceived;
		}

		// Event handler for receiving local notifications.
		private void OnLocalNotificationReceived(iOSNotification notification)
		{
			// Create a new AndroidGameNotification out of the delivered notification, but only
			// if the event is registered
			NotificationReceived?.Invoke(new IosGameNotification(notification));
		}
	}
}
#endif