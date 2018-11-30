using System;

namespace NotificationSamples
{
	/// <summary>
	/// Represents a notification that was scheduled with <see cref="GameNotificationsManager.ScheduleNotification"/>.
	/// </summary>
	public struct PendingNotification
	{
		/// <summary>
		/// The ID of the pending notification.
		/// </summary>
		public readonly int Id;
		/// <summary>
		/// The title of the pending notification.
		/// </summary>
		public readonly string Title;
		/// <summary>
		/// The time when the notification will be shown.
		/// </summary>
		public readonly DateTime NotificationTime;

		/// <summary>
		/// Instantiate a new instance of <see cref="PendingNotification"/>.
		/// </summary>
		/// <param name="id">The ID of the pending notification.</param>
		/// <param name="title">The title of the pending notification.</param>
		/// <param name="notificationTime">The time when the notification is scheduled to be displayed.</param>
		public PendingNotification(int id, string title, DateTime notificationTime)
		{
			Id = id;
			Title = title;
			NotificationTime = notificationTime;
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="PendingNotification"/> from a <see cref="IGameNotification"/>.
		/// </summary>
		/// <param name="notification">The notification to create from.</param>
		public PendingNotification(IGameNotification notification)
			: this(notification.Id ?? throw new ArgumentNullException(nameof(notification), "Id of provided notification cannot be null."), notification.Title, notification.DeliveryTime ?? DateTime.Now)
		{
			
		}
	}
}
