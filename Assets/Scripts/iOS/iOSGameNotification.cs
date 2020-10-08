#if UNITY_IOS
using System;
using Unity.Notifications.iOS;
using UnityEngine;
using UnityEngine.Assertions;

namespace NotificationSamples.iOS
{
    /// <summary>
    /// iOS implementation of <see cref="IGameNotification"/>.
    /// </summary>
    public class iOSGameNotification : IGameNotification
    {
        private readonly iOSNotification internalNotification;

        /// <summary>
        /// Gets the internal notification object used by the mobile notifications system.
        /// </summary>
        public iOSNotification InternalNotification => internalNotification;

        /// <inheritdoc />
        /// <remarks>
        /// Internally stored as a string. Gets parsed to an integer when retrieving.
        /// </remarks>
        /// <value>The identifier as an integer, or null if the identifier couldn't be parsed as a number.</value>
        public int? Id
        {
            get
            {
                if (!int.TryParse(internalNotification.Identifier, out int value))
                {
                    Debug.LogWarning("Internal iOS notification's identifier isn't a number.");
                    return null;
                }

                return value;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                internalNotification.Identifier = value.Value.ToString();
            }
        }

        /// <inheritdoc />
        public string Title { get => internalNotification.Title; set => internalNotification.Title = value; }

        /// <inheritdoc />
        public string Body { get => internalNotification.Body; set => internalNotification.Body = value; }

        /// <inheritdoc />
        public string Subtitle { get => internalNotification.Subtitle; set => internalNotification.Subtitle = value; }

        /// <inheritdoc />
        public string Data { get => internalNotification.Data; set => internalNotification.Data = value; }

        /// <inheritdoc />
        /// <remarks>
        /// On iOS, this represents the notification's Category Identifier.
        /// </remarks>
        /// <value>The value of <see cref="CategoryIdentifier"/>.</value>
        public string Group { get => CategoryIdentifier; set => CategoryIdentifier = value; }

        /// <inheritdoc />
        public int? BadgeNumber
        {
            get => internalNotification.Badge != -1 ? internalNotification.Badge : (int?)null;
            set => internalNotification.Badge = value ?? -1;
        }

        /// <inheritdoc />
        public bool ShouldAutoCancel { get; set; }

        /// <inheritdoc />
        public bool Scheduled { get; private set; }

        /// <inheritdoc />
        /// <remarks>
        /// <para>On iOS, setting this causes the notification to be delivered on a calendar time.</para>
        /// <para>If it has previously been manually set to a different type of trigger, or has not been set before,
        /// this returns null.</para>
        /// <para>The millisecond component of the provided DateTime is ignored.</para>
        /// </remarks>
        /// <value>A <see cref="DateTime"/> representing the delivery time of this message, or null if
        /// not set or the trigger isn't a <see cref="iOSNotificationCalendarTrigger"/>.</value>
        public DateTime? DeliveryTime
        {
            get
            {
                if (!(internalNotification.Trigger is iOSNotificationCalendarTrigger calendarTrigger))
                {
                    return null;
                }

                DateTime now = DateTime.Now;
                var result = new DateTime
                    (
                    calendarTrigger.Year ?? now.Year,
                    calendarTrigger.Month ?? now.Month,
                    calendarTrigger.Day ?? now.Day,
                    calendarTrigger.Hour ?? now.Hour,
                    calendarTrigger.Minute ?? now.Minute,
                    calendarTrigger.Second ?? now.Second,
                    DateTimeKind.Local
                    );

                return result;
            }
            set
            {
                if (!value.HasValue)
                {
                    return;
                }

                DateTime date = value.Value.ToLocalTime();

                internalNotification.Trigger = new iOSNotificationCalendarTrigger
                {
                    Year = date.Year,
                    Month = date.Month,
                    Day = date.Day,
                    Hour = date.Hour,
                    Minute = date.Minute,
                    Second = date.Second
                };
            }
        }

        /// <summary>
        /// The category identifier for this notification.
        /// </summary>
        public string CategoryIdentifier
        {
            get => internalNotification.CategoryIdentifier;
            set => internalNotification.CategoryIdentifier = value;
        }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public string SmallIcon { get => null; set {} }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public string LargeIcon { get => null; set {} }

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSGameNotification"/>.
        /// </summary>
        public iOSGameNotification()
        {
            internalNotification = new iOSNotification
            {
                ShowInForeground = true // Deliver in foreground by default
            };
        }

        /// <summary>
        /// Instantiate a new instance of <see cref="iOSGameNotification"/> from a delivered notification.
        /// </summary>
        /// <param name="internalNotification">The delivered notification.</param>
        internal iOSGameNotification(iOSNotification internalNotification)
        {
            this.internalNotification = internalNotification;
        }

        /// <summary>
        /// Mark this notifications scheduled flag.
        /// </summary>
        internal void OnScheduled()
        {
            Assert.IsFalse(Scheduled);
            Scheduled = true;
        }
    }
}
#endif
