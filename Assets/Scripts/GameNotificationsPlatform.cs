using System;
using System.Collections;
using Unity.Notifications;

namespace NotificationSamples
{

    public abstract class GameNotificationsPlatform : IGameNotificationsPlatform
    {
        public abstract event Action<IGameNotification> NotificationReceived;

        public IEnumerator RequestNotificationPermission()
        {
            return NotificationCenter.RequestPermission();
        }

        public abstract IGameNotification CreateNotification();

        public abstract void ScheduleNotification(IGameNotification gameNotification, DateTime deliveryTime);

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

        public abstract IGameNotification GetLastNotification();

        public abstract void OnForeground();

        public abstract void OnBackground();
    }

}