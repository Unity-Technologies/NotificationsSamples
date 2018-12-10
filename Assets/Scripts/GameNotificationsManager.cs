using System;
using System.Collections.Generic;
using System.IO;
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
		// Default filename for notifications serializer
		private const string DefaultFilename = "notifications.bin";
		
		// Minimum amount of time that a notification should be into the future before it's queued when we background.
		private static readonly TimeSpan MinimumNotificationTime = new TimeSpan(0, 0, 2);

		[Flags]
		public enum OperatingMode
		{
			/// <summary>
			/// Do not perform any queueing at all. All notifications are scheduled with the operating system
			/// immediately.
			/// </summary>
			NoQueue = 0x00,

			/// <summary>
			/// <para>
			/// Queue messages that are scheduled with this manager.
			/// No messages will be sent to the operating system until the application is backgrounded.
			/// </para>
			/// </summary>
			Queue = 0x01,

			/// <summary>
			/// When the application is foregrounded, clear all pending notifications.
			/// </summary>
			ClearOnForegrounding = 0x02,

			/// <summary>
			/// After clearing events, will put future ones back into the queue if they are marked with <see cref="PendingNotification.Reschedule"/>.
			/// </summary>
			/// <remarks>
			/// Only valid if <see cref="ClearOnForegrounding"/> is also set.
			/// </remarks>
			RescheduleAfterClearing = 0x04,

			/// <summary>
			/// <para>
			/// Combines the behaviour of <see cref="Queue"/>, <see cref="ClearOnForegrounding"/> and
			/// <see cref="RescheduleAfterClearing"/>.
			/// </para>
			/// <para>
			/// Ensures that messages will never be displayed while the application is in the foreground.
			/// </para>
			/// </summary>
			QueueClearAndReschedule = Queue | ClearOnForegrounding | RescheduleAfterClearing
		}

		[SerializeField]
		private OperatingMode mode;

		/// <summary>
		/// Event fired when a scheduled local notification is delivered while the app is in the foreground.
		/// </summary>
		public event Action<PendingNotification> LocalNotificationDelivered;

		/// <summary>
		/// Gets the implementation of the notifications for the current platform;
		/// </summary>
		public IGameNotificationsPlatform Platform { get; private set; }

		/// <summary>
		/// Gets a collection of notifications that are scheduled or queued.
		/// </summary>
		public List<PendingNotification> PendingNotifications { get; private set; }

		/// <summary>
		/// Gets or sets the serializer to use to save pending notifications to disk if we're in
		/// <see cref="OperatingMode.RescheduleAfterClearing"/> mode.
		/// </summary>
		public IPendingNotificationsSerializer Serializer { get; set; }

		/// <summary>
		/// Gets whether this manager queues events internally.
		/// </summary>
		/// <remarks>
		/// When this is true, it will never schedule notifications immediately, only when the application goes
		/// into the background.
		/// </remarks>
		public OperatingMode Mode => mode;

		// Reused list for rescheduling
		private readonly List<IGameNotification> notificationsToReschedule = new List<IGameNotification>();

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

			if (Platform == null)
			{
				return;
			}

			PendingNotifications = new List<PendingNotification>();
			Platform.NotificationReceived += OnNotificationReceived;
		}

		/// <summary>
		/// Clean up platform object if necessary 
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (Platform == null)
			{
				return;
			}

			Platform.NotificationReceived -= OnNotificationReceived;
			if (Platform is IDisposable disposable)
			{
				disposable.Dispose();
			}
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
		public PendingNotification ScheduleNotification(IGameNotification notification)
		{
			if (notification == null || Platform == null)
			{
				return null;
			}

			// If we queue, don't schedule immediately.
			// Also immediately schedule non-time based deliveries (for iOS)
			if ((mode & OperatingMode.Queue) != OperatingMode.Queue ||
			    notification.DeliveryTime == null)
			{
				Platform.ScheduleNotification(notification);
			}
			else if (!notification.Id.HasValue)
			{
				// Generate an ID for items that don't have one (just so they can be identified later)
				int id = Math.Abs(DateTime.Now.ToString("yyMMddHHmmssffffff").GetHashCode());
				notification.Id = id;
			}

			// Register pending notification
			var result = new PendingNotification(notification);
			PendingNotifications.Add(result);

			return result;
		}

		/// <summary>
		/// Cancels a scheduled notification.
		/// </summary>
		/// <param name="notificationId">The ID of the notification to cancel.</param>
		public void CancelNotification(int notificationId)
		{
			if (Platform == null)
			{
				return;
			}

			Platform.CancelNotification(notificationId);

			// Remove the cancelled notification from scheduled list
			int index = PendingNotifications.FindIndex(scheduledNotification =>
				                                           scheduledNotification.Notification.Id == notificationId);

			if (index >= 0)
			{
				PendingNotifications.RemoveAt(index);
			}
		}

		/// <summary>
		/// Cancels all scheduled notifications.
		/// </summary>
		public void CancelAllNotifications()
		{
			if (Platform == null)
			{
				return;
			}

			Platform.CancelAllScheduledNotifications();

			PendingNotifications.Clear();
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
			Platform?.DismissAllDisplayedNotifications();
		}

		/// <summary>
		/// Respond to application foreground/background events.
		/// </summary>
		private void OnApplicationFocus(bool hasFocus)
		{
			if (Platform == null)
			{
				return;
			}
			
			// Check serializer
			if (Serializer == null)
			{
				Serializer = new DefaultSerializer(Path.Combine(Application.persistentDataPath, DefaultFilename));
			}

			// Backgrounding
			if (!hasFocus)
			{
				// Queue future dated notifications
				if ((mode & OperatingMode.Queue) == OperatingMode.Queue)
				{
					for (var i = PendingNotifications.Count - 1; i >= 0; i--)
					{
						PendingNotification pendingNotification = PendingNotifications[i];
						// Ignore already scheduled ones
						if (pendingNotification.Notification.Scheduled)
						{
							return;
						}

						// If a non-scheduled notification is in the past (or not within our threshold)
						// just remove it immediately
						if (pendingNotification.Notification.DeliveryTime != null &&
						    pendingNotification.Notification.DeliveryTime - DateTime.Now < MinimumNotificationTime)
						{
							PendingNotifications.RemoveAt(i);
							continue;
						}

						// Schedule it now
						Platform.ScheduleNotification(pendingNotification.Notification);
					}
				}
				
				// Save to disk
				Serializer.Serialize(PendingNotifications);
			}
			else
			{
				// Foregrounding
				if ((mode & OperatingMode.ClearOnForegrounding) == OperatingMode.ClearOnForegrounding)
				{
					// Gather list of notifications to reschedule
					bool doRescheduling = (mode & OperatingMode.RescheduleAfterClearing) ==
					                      OperatingMode.RescheduleAfterClearing;
					
					// Retrieve pending list back from file
					PendingNotifications.Clear();
					IList<IGameNotification> loaded = Serializer.Deserialize(Platform);
					
					if (doRescheduling && loaded != null)
					{
						notificationsToReschedule.Clear();
						foreach (IGameNotification pendingNotification in loaded)
						{
							if (pendingNotification.DeliveryTime > DateTime.Now)
							{
								notificationsToReschedule.Add(pendingNotification);
							}
						}
					}

					// Clear on foregrounding
					Platform.CancelAllScheduledNotifications();
					PendingNotifications.Clear();

					// Reschedule notifications
					if (doRescheduling)
					{
						foreach (IGameNotification notification in notificationsToReschedule)
						{
							PendingNotification pendingNotification = ScheduleNotification(notification);
							pendingNotification.Reschedule = true;
						}

						notificationsToReschedule.Clear();
					}
				}
			}
		}

		/// <summary>
		/// Event fired by <see cref="Platform"/> when a notification is received while we're in the foreground.
		/// </summary>
		protected void OnNotificationReceived(IGameNotification deliveredNotification)
		{
			// Find in pending list
			int deliveredIndex =
				PendingNotifications.FindIndex(scheduledNotification =>
					                               scheduledNotification.Notification.Id == deliveredNotification.Id);
			if (deliveredIndex >= 0)
			{
				LocalNotificationDelivered?.Invoke(PendingNotifications[deliveredIndex]);

				PendingNotifications.RemoveAt(deliveredIndex);
			}
		}
	}
}