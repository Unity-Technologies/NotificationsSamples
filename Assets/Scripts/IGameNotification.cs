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
        /// Gets or sets optional arbitrary data for the notification.
        /// </summary>
        string Data { get; set; }

        /// <summary>
        /// Gets or sets the badge number for this notification. No badge number will be shown if null.
        /// </summary>
        /// <value>The number displayed on the app badge.</value>
        int BadgeNumber { get; set; }
    }
}
