#if UNITY_ANDROID
using System;
using Unity.Notifications.Android;
using UnityEngine.Assertions;

namespace NotificationSamples.Android
{
    /// <summary>
    /// Android specific implementation of <see cref="IGameNotification"/>.
    /// </summary>
    public class AndroidGameNotification : IGameNotification
    {
        private AndroidNotification internalNotification;

        /// <summary>
        /// Gets the internal notification object used by the mobile notifications system.
        /// </summary>
        public AndroidNotification InternalNotification => internalNotification;

        /// <inheritdoc />
        /// <summary>
        /// On Android, if the ID isn't explicitly set, it will be generated after it has been scheduled.
        /// </summary>
        public int? Id { get; set; }

        /// <inheritdoc />
        public string Title { get => InternalNotification.Title; set => internalNotification.Title = value; }

        /// <inheritdoc />
        public string Body { get => InternalNotification.Text; set => internalNotification.Text = value; }

        /// <summary>
        /// Does nothing on Android.
        /// </summary>
        public string Subtitle { get => null; set {} }

        /// <inheritdoc />
        public string Data { get => InternalNotification.IntentData; set => internalNotification.IntentData = value; }

        /// <inheritdoc />
        /// <remarks>
        /// On Android, this represents the notification's channel, and is required. Will be configured automatically by
        /// <see cref="AndroidNotificationsPlatform"/> if <see cref="AndroidNotificationsPlatform.DefaultChannelId"/> is set
        /// </remarks>
        /// <value>The value of <see cref="DeliveredChannel"/>.</value>
        public string Group { get => DeliveredChannel; set => DeliveredChannel = value; }

        /// <inheritdoc />
        public int? BadgeNumber
        {
            get => internalNotification.Number != -1 ? internalNotification.Number : (int?)null;
            set => internalNotification.Number = value ?? -1;
        }

        /// <inheritdoc />
        public bool ShouldAutoCancel
        {
            get => InternalNotification.ShouldAutoCancel;
            set => internalNotification.ShouldAutoCancel = value;
        }

        /// <inheritdoc />
        public DateTime? DeliveryTime
        {
            get => InternalNotification.FireTime;
            set => internalNotification.FireTime = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the channel for this notification.
        /// </summary>
        public string DeliveredChannel { get; set; }

        /// <inheritdoc />
        public bool Scheduled { get; private set; }

        /// <inheritdoc />
        public string SmallIcon { get => InternalNotification.SmallIcon; set => internalNotification.SmallIcon = value; }

        /// <inheritdoc />
        public string LargeIcon { get => InternalNotification.LargeIcon; set => internalNotification.LargeIcon = value; }

        /// <summary>
        /// Instantiate a new instance of <see cref="AndroidGameNotification"/>.
        /// </summary>
        public AndroidGameNotification()
        {
            internalNotification = new AndroidNotification();
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="AndroidGameNotification"/> from a delivered notification
        /// </summary>
        /// <param name="deliveredNotification">The notification that has been delivered.</param>
        /// <param name="deliveredId">The ID of the delivered notification.</param>
        /// <param name="deliveredChannel">The channel the notification was delivered to.</param>
        internal AndroidGameNotification(AndroidNotification deliveredNotification, int deliveredId,
                                         string deliveredChannel)
        {
            internalNotification = deliveredNotification;
            Id = deliveredId;
            DeliveredChannel = deliveredChannel;
        }

        /// <summary>
        /// Set the scheduled flag.
        /// </summary>
        internal void OnScheduled()
        {
            Assert.IsFalse(Scheduled);
            Scheduled = true;
        }
    }
}
#endif
