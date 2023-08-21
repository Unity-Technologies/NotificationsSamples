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

        /// <inheritdoc />
        public string Data { get => InternalNotification.IntentData; set => internalNotification.IntentData = value; }

        /// <inheritdoc />
        public int BadgeNumber
        {
            get => internalNotification.Number;
            set => internalNotification.Number = value;
        }

        /// <inheritdoc />
        public DateTime DeliveryTime
        {
            get => InternalNotification.FireTime;
            set => internalNotification.FireTime = value;
        }

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
        internal AndroidGameNotification(AndroidNotification deliveredNotification, int deliveredId)
        {
            internalNotification = deliveredNotification;
            Id = deliveredId;
        }
    }
}
#endif
