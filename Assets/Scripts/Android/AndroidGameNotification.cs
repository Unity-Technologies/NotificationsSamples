#if UNITY_ANDROID
using System;
using Unity.Notifications.Android;

namespace NotificationSamples.Android
{
	/// <summary>
	/// Android specific implementation of <see cref="IGameNotification"/>.
	/// </summary>
	public class AndroidGameNotification : IGameNotification
	{
		private AndroidNotification internalNotification;
		private int id;

		/// <summary>
		/// Gets the internal notification object used by the mobile notifications system.
		/// </summary>
		public AndroidNotification InternalNotification => internalNotification;

		/// <inheritdoc />
		/// <summary>
		/// On Android, IDs are only available after the message has been sent.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">Throw when trying to set this value.</exception>
		/// TODO: Update once unity API supports setting ID.
		public int? Id { get => id; set => throw new NotSupportedException(); }

		/// <inheritdoc />
		public string Title { get => InternalNotification.Title; set => internalNotification.Title = value; }

		/// <inheritdoc />
		public string Body { get => InternalNotification.Text; set => internalNotification.Text = value; }

		/// <summary>
		/// Does nothing on Android.
		/// </summary>
		public string Subtitle { get => null; set { } }

		/// <inheritdoc />
		/// <remarks>
		/// On Android, this represents the notification's channel, and is required. Will be configured automatically by
		/// <see cref="AndroidNotificationsPlatform"/> if <see cref="AndroidNotificationsPlatform.DefaultChannelId"/> is set
		/// </remarks>
		/// <value>The value of <see cref="DeliveredChannel"/>.</value>
		public string Group { get => DeliveredChannel; set => DeliveredChannel = value; }

		/// <inheritdoc />
		public int? BadgeNumber
		{
			get => internalNotification.Number != -1 ? internalNotification.Number : (int?)null; 
			set => internalNotification.Number = value ?? -1;
		}

		/// <inheritdoc />
		public DateTime? DeliveryTime
		{
			get => InternalNotification.FireTime;
			set => internalNotification.FireTime = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Gets or sets the channel for this notification.
		/// </summary>
		public string DeliveredChannel { get; set; }

		/// <summary>
		/// Instantiate a new instance of <see cref="AndroidGameNotification"/>.
		/// </summary>
		public AndroidGameNotification()
		{
			internalNotification = new AndroidNotification();
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="AndroidGameNotification"/> from a delivered notification
		/// </summary>
		/// <param name="deliveredNotification">The notification that has been delivered.</param>
		/// <param name="deliveredId">The ID of the delivered notification.</param>
		/// <param name="deliveredChannel">The channel the notification was delivered to.</param>
		internal AndroidGameNotification(AndroidNotification deliveredNotification, int deliveredId, string deliveredChannel)
		{
			internalNotification = deliveredNotification;
			id = deliveredId;
			DeliveredChannel = deliveredChannel;
		}

		/// <summary>
		/// Called when this notification gets scheduled
		/// </summary>
		/// <param name="returnedId">The id this notification was provided</param>
		internal void OnScheduled(int returnedId)
		{
			id = returnedId;
		}
	}
}
#endif