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
		public DateTime DeliveryTime { get; set; }

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