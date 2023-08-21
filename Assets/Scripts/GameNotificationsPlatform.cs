using System;
using System.Collections;

namespace NotificationSamples
{

    public abstract class GameNotificationsPlatform : IGameNotificationsPlatform
    {
        public abstract event Action<IGameNotification> NotificationReceived;

        public abstract IEnumerator RequestNotificationPermission();

        public abstract IGameNotification CreateNotification();

        public abstract void ScheduleNotification(IGameNotification gameNotification, DateTime deliveryTime);

        public abstract void CancelNotification(int notificationId);

        public abstract void DismissNotification(int notificationId);

        public abstract void CancelAllScheduledNotifications();

        public abstract void DismissAllDisplayedNotifications();

        public abstract IGameNotification GetLastNotification();

        public abstract void OnForeground();

        public abstract void OnBackground();
    }

}