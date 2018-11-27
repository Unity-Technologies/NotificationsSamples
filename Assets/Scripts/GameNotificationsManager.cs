using NotificationSamples.Android;
using UnityEngine;

namespace NotificationSamples
{
	/// <summary>
	/// Global notifications manager that serves as a wrapper for multiple platform's notification systems.
	/// </summary>
	public class GameNotificationsManager : MonoBehaviour
	{
		private IGameNotificationsPlatform platform;

		/// <summary>
		/// Initialize the correct platform
		/// </summary>
		protected virtual void Awake()
		{
#if UNITY_ANDROID
			platform = new AndroidNotificationsPlatform();
#endif
		}

		/// <summary>
		/// Creates a new notification object for the current platform.
		/// </summary>
		/// <returns>The new notification, ready to be scheduled, or null if there's no valid platform.</returns>
		public IGameNotification CreateNotification()
		{
			return platform?.CreateNotification();
		}

		/// <summary>
		/// Schedules a notification to be delivered.
		/// </summary>
		/// <param name="notification">The notification to deliver.</param>
		public void ScheduleNotification(IGameNotification notification)
		{
			if (notification != null)
			{
				platform?.ScheduleNotification(notification);
			}
		}
	}
}