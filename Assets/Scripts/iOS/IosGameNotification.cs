#if UNITY_IOS
using System;
using Unity.Notifications.iOS;

namespace NotificationSamples.iOS
{
	/// <summary>
	/// iOS implementation of <see cref="IGameNotification"/>.
	/// </summary>
	public class IosGameNotification : IGameNotification
	{
		private readonly iOSNotification internalNotification;

		/// <summary>
		/// Gets the internal notification object used by the mobile notifications system.
		/// </summary>
		public iOSNotification InternalNotification => internalNotification;

		/// <inheritdoc />
		public string Id { get => internalNotification.Identifier; set => internalNotification.Identifier = value; }

		/// <inheritdoc />
		public string Title { get => internalNotification.Title; set => internalNotification.Title = value; }

		/// <inheritdoc />
		public string Body { get => internalNotification.Body; set => internalNotification.Body = value; }

		/// <inheritdoc />
		public string Subtitle { get => internalNotification.Subtitle; set => internalNotification.Subtitle = value; }

		/// <inheritdoc />
		/// <remarks>
		/// On iOS, this represents the notification's Category Identifier.
		/// </remarks>
		/// <value>The value of <see cref="CategoryIdentifier"/>.</value>
		public string Group { get => CategoryIdentifier; set => CategoryIdentifier = value; }

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
		/// Instantiate a new instance of <see cref="IosGameNotification"/>.
		/// </summary>
		public IosGameNotification()
		{
			internalNotification = new iOSNotification();
		}
	}
}
#endif