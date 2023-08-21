using System;
using System.Collections;
using Unity.Notifications;

namespace NotificationSamples
{

    public abstract class GameNotificationsPlatform : IGameNotificationsPlatform
    {
        public abstract event Action<GameNotification> NotificationReceived;

        public IEnumerator RequestNotificationPermission()
        {
            return NotificationCenter.RequestPermission();
        }

        public abstract GameNotification CreateNotification();

        public abstract void ScheduleNotification(GameNotification gameNotification, DateTime deliveryTime);

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

        public abstract GameNotification GetLastNotification();

        public abstract void OnForeground();

        public abstract void OnBackground();
    }

}