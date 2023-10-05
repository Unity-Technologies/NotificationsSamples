using System;

namespace NotificationSamples
{
    /// <summary>
    /// Represents a notification that was scheduled with <see cref="GameNotificationsManager.ScheduleNotification"/>.
    /// </summary>
    public class PendingNotification
    {
        /// <summary>
        /// Whether to reschedule this event if it hasn't displayed once the app is foregrounded again.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only valid if the <see cref="GameNotificationsManager"/>'s <see cref="GameNotificationsManager.Mode"/>
        /// flag is set to <see cref="GameNotificationsManager.OperatingMode.RescheduleAfterClearing"/>.
        /// </para>
        /// <para>
        /// Will not function for any notifications that are using a delivery scheduling method that isn't time
        /// based, such as iOS location notifications.
        /// </para>
        /// </remarks>
        public bool Reschedule;

        /// <summary>
        /// Whether notification is scheduled.
        /// </summary>
        public bool Scheduled { get; private set; }

        /// <summary>
        /// The scheduled notification.
        /// </summary>
        public readonly GameNotification Notification;

        public readonly DateTime DeliveryTime;

        /// <summary>
        /// Instantiate a new instance of <see cref="PendingNotification"/> from a <see cref="IGameNotification"/>.
        /// </summary>
        /// <param name="notification">The notification to create from.</param>
        public PendingNotification(GameNotification notification, DateTime deliveryTime, bool scheduled = false)
        {
            Notification = notification ?? throw new ArgumentNullException(nameof(notification));
            DeliveryTime = deliveryTime;
            Scheduled = scheduled;
        }

        public void Schedule()
        {
            Scheduled = true;
        }
    }
}
