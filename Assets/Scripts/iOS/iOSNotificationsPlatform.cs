#if UNITY_IOS
using System;
using System.Collections;
using Unity.Notifications;

namespace NotificationSamples.iOS
{
    /// <summary>
    /// iOS implementation of <see cref="IGameNotificationsPlatform"/>.
    /// </summary>
    public class iOSNotificationsPlatform : GameNotificationsPlatform, IDisposable
    {
        /// <inheritdoc />
        public override event Action<GameNotification> NotificationReceived;

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSNotificationsPlatform"/>.
        /// </summary>
        public iOSNotificationsPlatform()
        {
            NotificationCenter.OnNotificationReceived += OnLocalNotificationReceived;
        }

        /// <inheritdoc />
        public override void ScheduleNotification(GameNotification notification, DateTime deliveryTime)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            int id = NotificationCenter.ScheduleNotification(notification.InternalNotification, new NotificationDateTimeSchedule(deliveryTime));
            notification.Id = id;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="T:NotificationSamples.Android.AndroidNotification" />.
        /// </summary>
        public override GameNotification CreateNotification()
        {
            return new GameNotification();
        }

        /// <inheritdoc />
        public override GameNotification GetLastNotification()
        {
            var notification = NotificationCenter.LastRespondedNotification;

            if (notification.HasValue)
            {
                return new GameNotification(notification.Value);
            }

            return null;
        }

        /// <summary>
        /// Clears badge count.
        /// </summary>
        public override void OnForeground()
        {
            NotificationCenter.ClearBadge();
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
            NotificationCenter.OnNotificationReceived -= OnLocalNotificationReceived;
        }

        // Event handler for receiving local notifications.
        private void OnLocalNotificationReceived(Notification notification)
        {
            // Create a new AndroidGameNotification out of the delivered notification, but only
            // if the event is registered
            NotificationReceived?.Invoke(new GameNotification(notification));
        }
    }
}
#endif
