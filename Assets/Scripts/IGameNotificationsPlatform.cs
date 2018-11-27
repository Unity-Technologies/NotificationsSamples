using System;

namespace NotificationSamples
{
	/// <summary>
	/// Any type that handles notifications for a specific game platform
	/// </summary>
	public interface IGameNotificationsPlatform
	{
		/// <summary>
		/// Create a new instance of a <see cref="IGameNotification"/> for this platform.
		/// </summary>
		/// <returns>A new platform-appropriate notification object.</returns>
		IGameNotification CreateNotification();

		/// <summary>
		/// Schedules a notification to be delivered.
		/// </summary>
		/// <param name="gameNotification">The notification to deliver.</param>
		/// <exception cref="ArgumentNullException"><paramref name="gameNotification"/> is null.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="gameNotification"/> isn't of the correct type.</exception>
		void ScheduleNotification(IGameNotification gameNotification);
	}

	/// <summary>
	/// Any type that handles notifications for a specific game platform.
	/// </summary>
	/// <remarks>Has a concrete notification type</remarks>
	/// <typeparam name="TNotificationType">The type of notification returned by this platform.</typeparam>
	public interface IGameNotificationsPlatform<TNotificationType> : IGameNotificationsPlatform
		where TNotificationType : IGameNotification
	{
		/// <summary>
		/// Create an instance of <typeparamref name="TNotificationType"/>.
		/// </summary>
		/// <returns>A new platform-appropriate notification object.</returns>
		new TNotificationType CreateNotification();

		/// <summary>
		/// Schedule a notification to be delivered.
		/// </summary>
		/// <param name="notification">The notification to deliver.</param>
		/// <exception cref="ArgumentNullException"><paramref name="notification"/> is null.</exception>
		void ScheduleNotification(TNotificationType notification);
	}
}
