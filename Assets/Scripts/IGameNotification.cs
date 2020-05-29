using System;

namespace NotificationSamples
{
    /// <summary>
    /// Represents a notification that will be delivered for this application.
    /// </summary>
    public interface IGameNotification
    {
        /// <summary>
        /// Gets or sets a unique identifier for this notification.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If null, will be generated automatically once the notification is delivered, and then
        /// can be retrieved afterwards.
        /// </para>
        /// <para>On some platforms, this might be converted to a string identifier internally.</para>
        /// </remarks>
        /// <value>A unique integer identifier for this notification, or null (on some platforms) if not explicitly set.</value>
        int? Id { get; set; }

        /// <summary>
        /// Gets or sets the notification's title.
        /// </summary>
        /// <value>The title message for the notification.</value>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the body text of the notification.
        /// </summary>
        /// <value>The body message for the notification.</value>
        string Body { get; set; }

        /// <summary>
        /// Gets or sets a subtitle for the notification.
        /// </summary>
        /// <value>The subtitle message for the notification.</value>
        string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets group to which this notification belongs.
        /// </summary>
        /// <value>A platform specific string identifier for the notification's group.</value>
        string Group { get; set; }

        /// <summary>
        /// Gets or sets the badge number for this notification. No badge number will be shown if null.
        /// </summary>
        /// <value>The number displayed on the app badge.</value>
        int? BadgeNumber { get; set; }

        /// <summary>
        /// Gets or sets if this notification will be dismissed automatically when the user taps it.
        /// Only available on Android.
        /// </summary>
        bool ShouldAutoCancel { get; set; }

        /// <summary>
        /// Gets or sets time to deliver the notification.
        /// </summary>
        /// <value>The time of delivery in local time.</value>
        DateTime? DeliveryTime { get; set; }

        /// <summary>
        /// Gets whether this notification has been scheduled.
        /// </summary>
        /// <value>True if the notification has been scheduled with the underlying operating system.</value>
        bool Scheduled { get; }

        /// <summary>
        /// Notification small icon.
        /// </summary>
        string SmallIcon { get; set; }

        /// <summary>
        /// Notification large icon.
        /// </summary>
        string LargeIcon { get; set; }
    }
}
