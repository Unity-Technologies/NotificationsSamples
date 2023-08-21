#if UNITY_ANDROID
using System;
using Unity.Notifications;

namespace NotificationSamples.Android
{
    /// <summary>
    /// Android specific implementation of <see cref="IGameNotification"/>.
    /// </summary>
    public class AndroidGameNotification : IGameNotification
    {
        private Notification internalNotification;

        /// <summary>
        /// Gets the internal notification object used by the mobile notifications system.
        /// </summary>
        public Notification InternalNotification => internalNotification;

        /// <inheritdoc />
        /// <summary>
        /// On Android, if the ID isn't explicitly set, it will be generated after it has been scheduled.
        /// </summary>
        public int? Id { get; set; }

        /// <inheritdoc />
        public string Title { get => InternalNotification.Title; set => internalNotification.Title = value; }

        /// <inheritdoc />
        public string Body { get => InternalNotification.Text; set => internalNotification.Text = value; }

        /// <inheritdoc />
        public string Data { get => InternalNotification.Data; set => internalNotification.Data = value; }

        /// <inheritdoc />
        public int BadgeNumber
        {
            get => internalNotification.Badge;
            set => internalNotification.Badge = value;
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="AndroidGameNotification"/>.
        /// </summary>
        public AndroidGameNotification()
        {
            internalNotification = new Notification();
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="AndroidGameNotification"/> from a delivered notification
        /// </summary>
        /// <param name="deliveredNotification">The notification that has been delivered.</param>
        /// <param name="deliveredId">The ID of the delivered notification.</param>
        internal AndroidGameNotification(Notification deliveredNotification)
        {
            internalNotification = deliveredNotification;
        }
    }
}
#endif
