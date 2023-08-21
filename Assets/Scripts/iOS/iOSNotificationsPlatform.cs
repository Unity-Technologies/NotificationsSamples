#if UNITY_IOS
using System;
using System.Collections;
using Unity.Notifications.iOS;

namespace NotificationSamples.iOS
{
    /// <summary>
    /// iOS implementation of <see cref="IGameNotificationsPlatform"/>.
    /// </summary>
    public class iOSNotificationsPlatform : GameNotificationsPlatform, IGameNotificationsPlatform<iOSGameNotification>,
        IDisposable
    {
        /// <inheritdoc />
        public override event Action<IGameNotification> NotificationReceived;

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSNotificationsPlatform"/>.
        /// </summary>
        public iOSNotificationsPlatform()
        {
            iOSNotificationCenter.OnNotificationReceived += OnLocalNotificationReceived;
        }

        /// <inheritdoc />
        public override void ScheduleNotification(IGameNotification gameNotification, DateTime deliveryTime)
        {
            if (gameNotification == null)
            {
                throw new ArgumentNullException(nameof(gameNotification));
            }

            if (!(gameNotification is iOSGameNotification notification))
            {
                throw new InvalidOperationException(
                    "Notification provided to ScheduleNotification isn't an iOSGameNotification.");
            }

            ScheduleNotification(notification, deliveryTime);
        }

        /// <inheritdoc />
        public void ScheduleNotification(iOSGameNotification notification, DateTime deliveryTime)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            notification.DeliveryTime = deliveryTime;
            iOSNotificationCenter.ScheduleNotification(notification.InternalNotification);
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="T:NotificationSamples.Android.AndroidNotification" />.
        /// </summary>
        public override IGameNotification CreateNotification()
        {
            return CreateNotification();
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="T:NotificationSamples.Android.AndroidNotification" />.
        /// </summary>
        iOSGameNotification IGameNotificationsPlatform<iOSGameNotification>.CreateNotification()
        {
            return new iOSGameNotification();
        }

        /// <inheritdoc />
        public override IGameNotification GetLastNotification()
        {
            return GetLastNotification();
        }

        /// <inheritdoc />
        iOSGameNotification IGameNotificationsPlatform<iOSGameNotification>.GetLastNotification()
        {
            var notification = iOSNotificationCenter.GetLastRespondedNotification();

            if (notification != null)
            {
                return new iOSGameNotification(notification);
            }

            return null;
        }

        /// <summary>
        /// Clears badge count.
        /// </summary>
        public override void OnForeground()
        {
            iOSNotificationCenter.ApplicationBadge = 0;
        }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public override void OnBackground() {}

        /// <summary>
        /// Unregister delegates.
        /// </summary>
        public void Dispose()
        {
            iOSNotificationCenter.OnNotificationReceived -= OnLocalNotificationReceived;
        }

        // Event handler for receiving local notifications.
        private void OnLocalNotificationReceived(iOSNotification notification)
        {
            // Create a new AndroidGameNotification out of the delivered notification, but only
            // if the event is registered
            NotificationReceived?.Invoke(new iOSGameNotification(notification));
        }
    }
}
#endif
