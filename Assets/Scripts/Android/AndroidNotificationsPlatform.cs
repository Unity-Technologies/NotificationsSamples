#if UNITY_ANDROID
using System;
using System.Collections;
using Unity.Notifications;

namespace NotificationSamples.Android
{
    /// <summary>
    /// Android implementation of <see cref="IGameNotificationsPlatform"/>.
    /// </summary>
    public class AndroidNotificationsPlatform : GameNotificationsPlatform, IGameNotificationsPlatform<GameNotification>,
        IDisposable
    {
        /// <inheritdoc />
        public override event Action<IGameNotification> NotificationReceived;

        /// <summary>
        /// Gets or sets the default channel ID for notifications.
        /// </summary>
        /// <value>The default channel ID for new notifications, or null.</value>
        public string DefaultChannelId { get; set; }

        /// <summary>
        /// Instantiate a new instance of <see cref="AndroidNotificationsPlatform"/>.
        /// </summary>
        public AndroidNotificationsPlatform()
        {
            NotificationCenter.OnNotificationReceived += OnLocalNotificationReceived;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Will set the <see cref="AndroidGameNotification.Id"/> field of <paramref name="gameNotification"/>.
        /// </remarks>
        public void ScheduleNotification(GameNotification gameNotification, DateTime deliveryTime)
        {
            if (gameNotification == null)
            {
                throw new ArgumentNullException(nameof(gameNotification));
            }

            int notificationId = NotificationCenter.ScheduleNotification(gameNotification.InternalNotification, new NotificationDateTimeSchedule(deliveryTime));
            gameNotification.Id = notificationId;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Will set the <see cref="AndroidGameNotification.Id"/> field of <paramref name="gameNotification"/>.
        /// </remarks>
        public override void ScheduleNotification(IGameNotification gameNotification, DateTime deliveryTime)
        {
            if (gameNotification == null)
            {
                throw new ArgumentNullException(nameof(gameNotification));
            }

            if (!(gameNotification is GameNotification androidNotification))
            {
                throw new InvalidOperationException(
                    "Notification provided to ScheduleNotification isn't an AndroidGameNotification.");
            }

            ScheduleNotification(androidNotification, deliveryTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="AndroidGameNotification" />.
        /// </summary>
        GameNotification IGameNotificationsPlatform<GameNotification>.CreateNotification()
        {
            return new GameNotification();
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="AndroidGameNotification" />.
        /// </summary>
        public override IGameNotification CreateNotification()
        {
            return CreateNotification();
        }

        /// <inheritdoc />
        public override IGameNotification GetLastNotification()
        {
            return GetLastNotification();
        }

        /// <inheritdoc />
        GameNotification IGameNotificationsPlatform<GameNotification>.GetLastNotification()
        {
            var notification = NotificationCenter.LastRespondedNotification;

            if (notification.HasValue)
            {
                return new GameNotification(notification.Value);
            }

            return null;
        }

        /// <summary>
        /// Does nothing on Android.
        /// </summary>
        public override void OnForeground() {}

        /// <summary>
        /// Does nothing on Android.
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
