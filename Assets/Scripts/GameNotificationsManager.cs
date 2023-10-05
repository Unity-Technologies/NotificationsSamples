using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Notifications;

namespace NotificationSamples
{
    /// <summary>
    /// Global notifications manager that serves as a wrapper for multiple platforms' notification systems.
    /// </summary>
    public class GameNotificationsManager : MonoBehaviour
    {
        // Default filename for notifications serializer
        private const string DefaultFilename = "notifications.bin";

        // Minimum amount of time that a notification should be into the future before it's queued when we background.
        private static readonly TimeSpan MinimumNotificationTime = new TimeSpan(0, 0, 2);

        [Flags]
        public enum OperatingMode
        {
            /// <summary>
            /// Do not perform any queueing at all. All notifications are scheduled with the operating system
            /// immediately.
            /// </summary>
            NoQueue = 0x00,

            /// <summary>
            /// <para>
            /// Queue messages that are scheduled with this manager.
            /// No messages will be sent to the operating system until the application is backgrounded.
            /// </para>
            /// <para>
            /// If badge numbers are not set, will automatically increment them. This will only happen if NO badge numbers
            /// for pending notifications are ever set.
            /// </para>
            /// </summary>
            Queue = 0x01,

            /// <summary>
            /// When the application is foregrounded, clear all pending notifications.
            /// </summary>
            ClearOnForegrounding = 0x02,

            /// <summary>
            /// After clearing events, will put future ones back into the queue if they are marked with <see cref="PendingNotification.Reschedule"/>.
            /// </summary>
            /// <remarks>
            /// Only valid if <see cref="ClearOnForegrounding"/> is also set.
            /// </remarks>
            RescheduleAfterClearing = 0x04,

            /// <summary>
            /// Combines the behaviour of <see cref="Queue"/> and <see cref="ClearOnForegrounding"/>.
            /// </summary>
            QueueAndClear = Queue | ClearOnForegrounding,

            /// <summary>
            /// <para>
            /// Combines the behaviour of <see cref="Queue"/>, <see cref="ClearOnForegrounding"/> and
            /// <see cref="RescheduleAfterClearing"/>.
            /// </para>
            /// <para>
            /// Ensures that messages will never be displayed while the application is in the foreground.
            /// </para>
            /// </summary>
            QueueClearAndReschedule = Queue | ClearOnForegrounding | RescheduleAfterClearing,
        }

        [SerializeField, Tooltip("The operating mode for the notifications manager.")]
        private OperatingMode mode = OperatingMode.QueueClearAndReschedule;

        [SerializeField, Tooltip(
            "Check to make the notifications manager automatically set badge numbers so that they increment.\n" +
            "Schedule notifications with no numbers manually set to make use of this feature.")]
        private bool autoBadging = true;

        /// <summary>
        /// Event fired when a scheduled local notification is delivered while the app is in the foreground.
        /// </summary>
        public event Action<PendingNotification> LocalNotificationDelivered;

        /// <summary>
        /// Event fired when a queued local notification is cancelled because the application is in the foreground
        /// when it was meant to be displayed.
        /// </summary>
        /// <seealso cref="OperatingMode.Queue"/>
        public event Action<PendingNotification> LocalNotificationExpired;

        /// <summary>
        /// Gets the implementation of the notifications for the current platform;
        /// </summary>
        public GameNotificationsPlatform Platform { get; private set; }

        /// <summary>
        /// Gets a collection of notifications that are scheduled or queued.
        /// </summary>
        public List<PendingNotification> PendingNotifications { get; private set; }

        /// <summary>
        /// Gets or sets the serializer to use to save pending notifications to disk if we're in
        /// <see cref="OperatingMode.RescheduleAfterClearing"/> mode.
        /// </summary>
        public IPendingNotificationsSerializer Serializer { get; set; }

        /// <summary>
        /// Gets the operating mode for this manager.
        /// </summary>
        /// <seealso cref="OperatingMode"/>
        public OperatingMode Mode => mode;

        /// <summary>
        /// Gets whether this manager automatically increments badge numbers.
        /// </summary>
        public bool AutoBadging => autoBadging;

        /// <summary>
        /// Gets whether this manager has been initialized.
        /// </summary>
        public bool Initialized { get; private set; }

        // Flag set when we're in the foreground
        private bool inForeground = true;

        /// <summary>
        /// Clean up platform object if necessary
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (Platform == null)
            {
                return;
            }

            Platform.NotificationReceived -= OnNotificationReceived;
            if (Platform is IDisposable disposable)
            {
                disposable.Dispose();
            }

            inForeground = false;
        }

        /// <summary>
        /// Check pending list for expired notifications, when in queue mode.
        /// </summary>
        protected virtual void Update()
        {
            if (PendingNotifications == null || !PendingNotifications.Any()
                || (mode & OperatingMode.Queue) != OperatingMode.Queue)
            {
                return;
            }

            // Check each pending notification for expiry, then remove it
            for (int i = PendingNotifications.Count - 1; i >= 0; --i)
            {
                PendingNotification queuedNotification = PendingNotifications[i];
                DateTime time = queuedNotification.DeliveryTime;
                if (time < DateTime.Now)
                {
                    PendingNotifications.RemoveAt(i);
                    LocalNotificationExpired?.Invoke(queuedNotification);
                }
            }
        }

        /// <summary>
        /// Respond to application foreground/background events.
        /// </summary>
        protected void OnApplicationFocus(bool hasFocus)
        {
            if (Platform == null || !Initialized)
            {
                return;
            }

            inForeground = hasFocus;

            if (hasFocus)
            {
                OnForegrounding();

                return;
            }

            Platform.OnBackground();

            // Backgrounding
            // Queue future dated notifications
            if ((mode & OperatingMode.Queue) == OperatingMode.Queue)
            {
                // Filter out past events
                for (var i = PendingNotifications.Count - 1; i >= 0; i--)
                {
                    PendingNotification pendingNotification = PendingNotifications[i];
                    // Ignore already scheduled ones
                    if (pendingNotification.Scheduled)
                    {
                        continue;
                    }

                    // If a non-scheduled notification is in the past (or not within our threshold)
                    // just remove it immediately
                    if (pendingNotification.DeliveryTime - DateTime.Now < MinimumNotificationTime)
                    {
                        PendingNotifications.RemoveAt(i);
                    }
                }

                // Sort notifications by delivery time, if no notifications have a badge number set
                bool noBadgeNumbersSet =
                    PendingNotifications.All(notification => notification.Notification.BadgeNumber == null);

                if (noBadgeNumbersSet && AutoBadging)
                {
                    PendingNotifications.Sort((a, b) =>
                    {
                        return a.DeliveryTime.CompareTo(b.DeliveryTime);
                    });

                    // Set badge numbers incrementally
                    var badgeNum = 1;
                    foreach (PendingNotification pendingNotification in PendingNotifications)
                    {
                        if (!pendingNotification.Scheduled)
                        {
                            pendingNotification.Notification.BadgeNumber = badgeNum++;
                        }
                    }
                }

                for (int i = PendingNotifications.Count - 1; i >= 0; i--)
                {
                    PendingNotification pendingNotification = PendingNotifications[i];
                    // Ignore already scheduled ones
                    if (pendingNotification.Scheduled)
                    {
                        continue;
                    }

                    // Schedule it now
                    Platform.ScheduleNotification(pendingNotification.Notification, pendingNotification.DeliveryTime);
                    pendingNotification.Schedule();
                }

                // Clear badge numbers again (for saving)
                if (noBadgeNumbersSet && AutoBadging)
                {
                    foreach (PendingNotification pendingNotification in PendingNotifications)
                    {
                        pendingNotification.Notification.BadgeNumber = 0;
                    }
                }
            }

            // Calculate notifications to save
            var notificationsToSave = new List<PendingNotification>(PendingNotifications.Count);
            foreach (PendingNotification pendingNotification in PendingNotifications)
            {
                // If we're in clear mode, add nothing unless we're in rescheduling mode
                // Otherwise add everything
                if ((mode & OperatingMode.ClearOnForegrounding) == OperatingMode.ClearOnForegrounding)
                {
                    if ((mode & OperatingMode.RescheduleAfterClearing) != OperatingMode.RescheduleAfterClearing)
                    {
                        continue;
                    }

                    // In reschedule mode, add ones that have been scheduled, are marked for
                    // rescheduling, and that have a time
                    if (pendingNotification.Reschedule &&
                        pendingNotification.Scheduled)
                    {
                        notificationsToSave.Add(pendingNotification);
                    }
                }
                else
                {
                    // In non-clear mode, just add all scheduled notifications
                    if (pendingNotification.Scheduled)
                    {
                        notificationsToSave.Add(pendingNotification);
                    }
                }
            }

            // Save to disk
            Serializer.Serialize(notificationsToSave);
        }

        /// <summary>
        /// Initialize the notifications manager.
        /// </summary>
        /// <param name="channels">An optional collection of channels to register, for Android</param>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has already been called.</exception>
        public IEnumerator Initialize()
        {
            if (Initialized)
            {
                throw new InvalidOperationException("NotificationsManager already initialized.");
            }

            Initialized = true;

            var args = NotificationCenterArgs.Default;
            args.AndroidChannelId = "notifications";
            args.AndroidChannelName = "Notifications";
            args.AndroidChannelDescription = "Game notifications";
            Platform = new GameNotificationsPlatform(args);

            PendingNotifications = new List<PendingNotification>();
            Platform.NotificationReceived += OnNotificationReceived;

            // Check serializer
            if (Serializer == null)
            {
                Serializer = new DefaultSerializer(Path.Combine(Application.persistentDataPath, DefaultFilename));
            }

            yield return Platform.RequestNotificationPermission();

            OnForegrounding();
        }

        /// <summary>
        /// Creates a new notification object for the current platform.
        /// </summary>
        /// <returns>The new notification, ready to be scheduled, or null if there's no valid platform.</returns>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public GameNotification CreateNotification()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            return Platform?.CreateNotification();
        }

        /// <summary>
        /// Schedules a notification to be delivered.
        /// </summary>
        /// <param name="notification">The notification to deliver.</param>
        public PendingNotification ScheduleNotification(GameNotification notification, DateTime deliveryTime)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            if (notification == null || Platform == null)
            {
                return null;
            }

            bool scheduled = false;

            // If we queue, don't schedule immediately.
            // Also immediately schedule non-time based deliveries (for iOS)
            if ((mode & OperatingMode.Queue) != OperatingMode.Queue)
            {
                Platform.ScheduleNotification(notification, deliveryTime);
                scheduled = true;
            }
            else if (!notification.Id.HasValue)
            {
                // Generate an ID for items that don't have one (just so they can be identified later)
                int id = Math.Abs(DateTime.Now.ToString("yyMMddHHmmssffffff").GetHashCode());
                notification.Id = id;
            }

            // Register pending notification
            var result = new PendingNotification(notification, deliveryTime, scheduled);
            PendingNotifications.Add(result);

            return result;
        }

        /// <summary>
        /// Cancels a scheduled notification.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to cancel.</param>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public void CancelNotification(int notificationId)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            if (Platform == null)
            {
                return;
            }

            Platform.CancelNotification(notificationId);

            // Remove the cancelled notification from scheduled list
            int index = PendingNotifications.FindIndex(scheduledNotification =>
                scheduledNotification.Notification.Id == notificationId);

            if (index >= 0)
            {
                PendingNotifications.RemoveAt(index);
            }
        }

        /// <summary>
        /// Cancels all scheduled notifications.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public void CancelAllNotifications()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            if (Platform == null)
            {
                return;
            }

            Platform.CancelAllScheduledNotifications();

            PendingNotifications.Clear();
        }

        /// <summary>
        /// Dismisses a displayed notification.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to dismiss.</param>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public void DismissNotification(int notificationId)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            Platform?.DismissNotification(notificationId);
        }

        /// <summary>
        /// Dismisses all displayed notifications.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
        public void DismissAllNotifications()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            Platform?.DismissAllDisplayedNotifications();
        }

        /// <summary>
        /// Last notification tapped by user.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public GameNotification GetLastNotification()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Must call Initialize() first.");
            }

            return Platform?.GetLastNotification();
        }

        /// <summary>
        /// Event fired by <see cref="Platform"/> when a notification is received.
        /// </summary>
        private void OnNotificationReceived(GameNotification deliveredNotification)
        {
            // Ignore for background messages (this happens on Android sometimes)
            if (!inForeground)
            {
                return;
            }

            // Find in pending list
            int deliveredIndex =
                PendingNotifications.FindIndex(scheduledNotification =>
                    scheduledNotification.Notification.Id == deliveredNotification.Id);
            if (deliveredIndex >= 0)
            {
                LocalNotificationDelivered?.Invoke(PendingNotifications[deliveredIndex]);

                PendingNotifications.RemoveAt(deliveredIndex);
            }
        }

        // Clear foreground notifications and reschedule stuff from a file
        private void OnForegrounding()
        {
            PendingNotifications.Clear();

            Platform.OnForeground();

            // Deserialize saved items
            IList<PendingNotification> loaded = Serializer?.Deserialize(Platform);

            // Foregrounding
            if ((mode & OperatingMode.ClearOnForegrounding) == OperatingMode.ClearOnForegrounding)
            {
                // Clear on foregrounding
                Platform.CancelAllScheduledNotifications();

                // Only reschedule in reschedule mode, and if we loaded any items
                if (loaded == null ||
                    (mode & OperatingMode.RescheduleAfterClearing) != OperatingMode.RescheduleAfterClearing)
                {
                    return;
                }

                // Reschedule notifications from deserialization
                foreach (var savedNotification in loaded)
                {
                    if (savedNotification.DeliveryTime > DateTime.Now)
                    {
                        PendingNotification pendingNotification = ScheduleNotification(savedNotification.Notification, savedNotification.DeliveryTime);
                        pendingNotification.Reschedule = true;
                    }
                }
            }
            else
            {
                // Just create PendingNotification wrappers for all deserialized items.
                // We're not rescheduling them because they were not cleared
                if (loaded == null)
                {
                    return;
                }

                foreach (var savedNotification in loaded)
                {
                    if (savedNotification.DeliveryTime > DateTime.Now)
                    {
                        PendingNotifications.Add(savedNotification);
                    }
                }
            }
        }
    }
}
