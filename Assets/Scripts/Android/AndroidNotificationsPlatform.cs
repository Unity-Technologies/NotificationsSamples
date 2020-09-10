#if UNITY_ANDROID
using System;
using Unity.Notifications.Android;

namespace NotificationSamples.Android
{
    /// <summary>
    /// Android implementation of <see cref="IGameNotificationsPlatform"/>.
    /// </summary>
    public class AndroidNotificationsPlatform : IGameNotificationsPlatform<AndroidGameNotification>,
        IDisposable
    {
        /// <inheritdoc />
        public event Action<IGameNotification> NotificationReceived;

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
            AndroidNotificationCenter.OnNotificationReceived += OnLocalNotificationReceived;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Will set the <see cref="AndroidGameNotification.Id"/> field of <paramref name="gameNotification"/>.
        /// </remarks>
        public void ScheduleNotification(AndroidGameNotification gameNotification)
        {
            if (gameNotification == null)
            {
                throw new ArgumentNullException(nameof(gameNotification));
            }

            if (gameNotification.Id.HasValue)
            {
                AndroidNotificationCenter.SendNotificationWithExplicitID(gameNotification.InternalNotification,
                    gameNotification.DeliveredChannel,
                    gameNotification.Id.Value);
            }
            else
            {
                int notificationId = AndroidNotificationCenter.SendNotification(gameNotification.InternalNotification,
                    gameNotification.DeliveredChannel);
                gameNotification.Id = notificationId;
            }

            gameNotification.OnScheduled();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Will set the <see cref="AndroidGameNotification.Id"/> field of <paramref name="gameNotification"/>.
        /// </remarks>
        public void ScheduleNotification(IGameNotification gameNotification)
        {
            if (gameNotification == null)
            {
                throw new ArgumentNullException(nameof(gameNotification));
            }

            if (!(gameNotification is AndroidGameNotification androidNotification))
            {
                throw new InvalidOperationException(
                    "Notification provided to ScheduleNotification isn't an AndroidGameNotification.");
            }

            ScheduleNotification(androidNotification);
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="AndroidGameNotification" />.
        /// </summary>
        public AndroidGameNotification CreateNotification()
        {
            var notification = new AndroidGameNotification()
            {
                DeliveredChannel = DefaultChannelId
            };

            return notification;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a new <see cref="AndroidGameNotification" />.
        /// </summary>
        IGameNotification IGameNotificationsPlatform.CreateNotification()
        {
            return CreateNotification();
        }

        /// <inheritdoc />
        public void CancelNotification(int notificationId)
        {
            AndroidNotificationCenter.CancelScheduledNotification(notificationId);
        }

        /// <inheritdoc />
        /// <summary>
        /// Not currently implemented on Android
        /// </summary>
        public void DismissNotification(int notificationId)
        {
            AndroidNotificationCenter.CancelDisplayedNotification(notificationId);
        }

        /// <inheritdoc />
        public void CancelAllScheduledNotifications()
        {
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }

        /// <inheritdoc />
        public void DismissAllDisplayedNotifications()
        {
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
        }

        /// <inheritdoc />
        IGameNotification IGameNotificationsPlatform.GetLastNotification()
        {
            return GetLastNotification();
        }

        /// <inheritdoc />
        public AndroidGameNotification GetLastNotification()
        {
            var data = AndroidNotificationCenter.GetLastNotificationIntent();

            if (data != null)
            {
                return new AndroidGameNotification(data.Notification, data.Id, data.Channel);
            }

            return null;
        }

        /// <summary>
        /// Does nothing on Android.
        /// </summary>
        public void OnForeground() {}

        /// <summary>
        /// Does nothing on Android.
        /// </summary>
        public void OnBackground() {}

        /// <summary>
        /// Unregister delegates.
        /// </summary>
        public void Dispose()
        {
            AndroidNotificationCenter.OnNotificationReceived -= OnLocalNotificationReceived;
        }

        // Event handler for receiving local notifications.
        private void OnLocalNotificationReceived(AndroidNotificationIntentData data)
        {
            // Create a new AndroidGameNotification out of the delivered notification, but only
            // if the event is registered
            NotificationReceived?.Invoke(new AndroidGameNotification(data.Notification, data.Id, data.Channel));
        }
    }
}
#endif
