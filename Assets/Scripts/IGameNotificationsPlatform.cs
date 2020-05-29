using System;

namespace NotificationSamples
{
    /// <summary>
    /// Any type that handles notifications for a specific game platform
    /// </summary>
    public interface IGameNotificationsPlatform
    {
        /// <summary>
        /// Fired when a notification is received.
        /// </summary>
        event Action<IGameNotification> NotificationReceived;

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

        /// <summary>
        /// Cancels a scheduled notification.
        /// </summary>
        /// <param name="notificationId">The ID of a previously scheduled notification.</param>
        void CancelNotification(int notificationId);

        /// <summary>
        /// Dismiss a displayed notification.
        /// </summary>
        /// <param name="notificationId">The ID of a previously scheduled notification that is being displayed to the user.</param>
        void DismissNotification(int notificationId);

        /// <summary>
        /// Cancels all scheduled notifications.
        /// </summary>
        void CancelAllScheduledNotifications();

        /// <summary>
        /// Dismisses all displayed notifications.
        /// </summary>
        void DismissAllDisplayedNotifications();

        /// <summary>
        /// Performs any initialization or processing necessary on foregrounding the application.
        /// </summary>
        void OnForeground();

        /// <summary>
        /// Performs any processing necessary on backgrounding or closing the application.
        /// </summary>
        void OnBackground();
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
