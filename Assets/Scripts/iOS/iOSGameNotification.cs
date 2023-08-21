#if UNITY_IOS
using System;
using Unity.Notifications;

namespace NotificationSamples.iOS
{
    /// <summary>
    /// iOS implementation of <see cref="IGameNotification"/>.
    /// </summary>
    public class iOSGameNotification : IGameNotification
    {
        private Notification internalNotification;

        /// <summary>
        /// Gets the internal notification object used by the mobile notifications system.
        /// </summary>
        public Notification InternalNotification => internalNotification;

        /// <inheritdoc />
        /// <remarks>
        /// Internally stored as a string. Gets parsed to an integer when retrieving.
        /// </remarks>
        /// <value>The identifier as an integer, or null if the identifier couldn't be parsed as a number.</value>
        public int? Id { get => internalNotification.Identifier; set => internalNotification.Identifier = value; }

        /// <inheritdoc />
        public string Title { get => internalNotification.Title; set => internalNotification.Title = value; }

        /// <inheritdoc />
        public string Body { get => internalNotification.Text; set => internalNotification.Text = value; }

        /// <inheritdoc />
        public string Data { get => internalNotification.Data; set => internalNotification.Data = value; }

        /// <inheritdoc />
        public int BadgeNumber
        {
            get => internalNotification.Badge;
            set => internalNotification.Badge = value;
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSGameNotification"/>.
        /// </summary>
        public iOSGameNotification()
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
        internal iOSGameNotification(Notification internalNotification)
        {
            this.internalNotification = internalNotification;
        }
    }
}
#endif
