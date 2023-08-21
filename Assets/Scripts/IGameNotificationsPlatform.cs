using System;
using System.Collections;

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
        event Action<GameNotification> NotificationReceived;

        IEnumerator RequestNotificationPermission();

        /// <summary>
        /// Create a new instance of a <see cref="IGameNotification"/> for this platform.
        /// </summary>
        /// <returns>A new platform-appropriate notification object.</returns>
        GameNotification CreateNotification();

        /// <summary>
        /// Schedules a notification to be delivered.
        /// </summary>
        /// <param name="gameNotification">The notification to deliver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameNotification"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="gameNotification"/> isn't of the correct type.</exception>
        void ScheduleNotification(GameNotification gameNotification, DateTime deliveryTime);

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
        /// Use this to retrieve the last local or remote notification received by the app.
        /// </summary>
        /// <remarks>
        /// On Android the last notification is not cleared until the application is explicitly quit.
        /// </remarks>
        /// <returns>
        /// Returns the last local or remote notification used to open the app or clicked on by the user. If no
        /// notification is available it returns null.
        /// </returns>
        GameNotification GetLastNotification();

        /// <summary>
        /// Performs any initialization or processing necessary on foregrounding the application.
        /// </summary>
        void OnForeground();

        /// <summary>
        /// Performs any processing necessary on backgrounding or closing the application.
        /// </summary>
        void OnBackground();
    }
}
