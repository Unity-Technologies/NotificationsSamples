using Unity.Notifications;

namespace NotificationSamples
{
    /// <summary>
    /// Represents a notification that will be delivered for this application.
    /// </summary>
    public class GameNotification
    {
        private Notification internalNotification;

        /// <summary>
        /// Gets the internal notification object used by the mobile notifications system.
        /// </summary>
        public Notification InternalNotification => internalNotification;

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
        public int? Id { get => internalNotification.Identifier; set => internalNotification.Identifier = value; }

        /// <summary>
        /// Gets or sets the notification's title.
        /// </summary>
        /// <value>The title message for the notification.</value>
        public string Title { get => internalNotification.Title; set => internalNotification.Title = value; }

        /// <summary>
        /// Gets or sets the body text of the notification.
        /// </summary>
        /// <value>The body message for the notification.</value>
        public string Body { get => internalNotification.Text; set => internalNotification.Text = value; }

        /// <summary>
        /// Gets or sets optional arbitrary data for the notification.
        /// </summary>
        public string Data { get => internalNotification.Data; set => internalNotification.Data = value; }

        /// <summary>
        /// Gets or sets the badge number for this notification. No badge number will be shown if null.
        /// </summary>
        /// <value>The number displayed on the app badge.</value>
        public int BadgeNumber
        {
            get => internalNotification.Badge;
            set => internalNotification.Badge = value;
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSGameNotification"/>.
        /// </summary>
        public GameNotification()
        {
            internalNotification = new Notification
            {
                ShowInForeground = true // Deliver in foreground by default
            };
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSGameNotification"/> from a delivered notification.
        /// </summary>
        /// <param name="internalNotification">The delivered notification.</param>
        internal GameNotification(Notification notification)
        {
            this.internalNotification = notification;
        }
    }
}
