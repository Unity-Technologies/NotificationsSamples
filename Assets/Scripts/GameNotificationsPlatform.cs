using System;
using System.Collections;
using Unity.Notifications;

namespace NotificationSamples
{

    public class GameNotificationsPlatform : IGameNotificationsPlatform, IDisposable
    {
        public event Action<GameNotification> NotificationReceived;

        public GameNotificationsPlatform(NotificationCenterArgs args)
        {
            NotificationCenter.Initialize(args);
            NotificationCenter.OnNotificationReceived += OnLocalNotificationReceived;
        }

        public IEnumerator RequestNotificationPermission()
        {
            return NotificationCenter.RequestPermission();
        }

        public GameNotification CreateNotification()
        {
            return new GameNotification();
        }

        public void ScheduleNotification(GameNotification gameNotification, DateTime deliveryTime)
        {
            if (gameNotification == null)
            {
                throw new ArgumentNullException(nameof(gameNotification));
            }

            int notificationId = NotificationCenter.ScheduleNotification(gameNotification.InternalNotification, new NotificationDateTimeSchedule(deliveryTime));
            gameNotification.Id = notificationId;
        }

        public void CancelNotification(int notificationId)
        {
            NotificationCenter.CancelScheduledNotification(notificationId);
        }

        public void DismissNotification(int notificationId)
        {
            NotificationCenter.CancelDeliveredNotification(notificationId);
        }

        public void CancelAllScheduledNotifications()
        {
            NotificationCenter.CancelAllScheduledNotifications();
        }

        public void DismissAllDisplayedNotifications()
        {
            NotificationCenter.CancelAllDeliveredNotifications();
        }

        public GameNotification GetLastNotification()
        {
            var notification = NotificationCenter.LastRespondedNotification;

            if (notification.HasValue)
            {
                return new GameNotification(notification.Value);
            }

            return null;
        }

        public void OnForeground()
        {
            NotificationCenter.ClearBadge();
        }

        public void OnBackground() {}

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