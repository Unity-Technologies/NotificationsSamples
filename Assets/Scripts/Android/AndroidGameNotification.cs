#if UNITY_ANDROID
using System;
using Unity.Notifications.Android;

namespace NotificationSamples.Android
{
	/// <summary>
	/// Android specific implementation of <see cref="IGameNotification"/>
	/// </summary>
	public class AndroidGameNotification : IGameNotification
	{
		private AndroidNotification internalNotification;
		private int id;
		private string idString;

		/// <summary>
		/// Gets the internal 
		/// </summary>
		public AndroidNotification InternalNotification => internalNotification;

		/// <summary>
		/// On Android, IDs are only available after the message has been sent.
		/// </summary>
		/// <exception cref="NotSupportedException">Throw when trying to set this value.</exception>
		public string Id { get => idString; set => throw new NotSupportedException(); }

		/// <inheritdoc />
		public string Title { get => InternalNotification.Title; set => internalNotification.Title = value; }

		/// <inheritdoc />
		public string Body { get => InternalNotification.Title; set => internalNotification.Title = value; }

		/// <summary>
		/// Does nothing on Android.
		/// </summary>
		public string Subtitle { get => null; set { } }

		/// <inheritdoc />
		/// <remarks>
		/// On Android, this represents the notification's channel, and is required. Will be configured automatically by
		/// <see cref="AndroidNotificationsPlatform"/> if <see cref="AndroidNotificationsPlatform.DefaultChannelId"/> is set
		/// </remarks>
		/// <value>The value of <see cref="Channel"/>.</value>
		public string Group { get => Channel; set => Channel = value; }

		/// <inheritdoc />
		public DateTime DeliveryTime
		{
			get => InternalNotification.FireTime;
			set => internalNotification.FireTime = value;
		}

		/// <summary>
		/// Gets or sets the channel for this notification.
		/// </summary>
		public string Channel { get; set; }

		/// <summary>
		/// Instantiate a new instance of <see cref="AndroidGameNotification"/>.
		/// </summary>
		public AndroidGameNotification()
		{
			internalNotification = new AndroidNotification();
		}

		/// <summary>
		/// Called when this notification gets scheduled
		/// </summary>
		/// <param name="returnedId">The id this notification was provided</param>
		internal void OnScheduled(int returnedId)
		{
			id = returnedId;
			idString = id.ToString();
		}
	}
}
#endif