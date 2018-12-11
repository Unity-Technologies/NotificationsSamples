using System.Collections.Generic;

namespace NotificationSamples
{
	/// <summary>
	/// Responsible for the serialization and deserialization of pending notifications for a
	/// <see cref="GameNotificationsManager"/> that is in <see
	/// cref="GameNotificationsManager.OperatingMode.RescheduleAfterClearing"/> mode.
	/// </summary>
	public interface IPendingNotificationsSerializer
	{
		/// <summary>
		/// Save a list of pending notifications.
		/// </summary>
		/// <param name="notifications">The collection notifications to save.</param>
		void Serialize(IList<PendingNotification> notifications);

		/// <summary>
		/// Retrieve a saved list of pending notifications.
		/// </summary>
		/// <returns>The deserialized collection of pending notifications, or null if the file did not exist.</returns>
		IList<IGameNotification> Deserialize(IGameNotificationsPlatform platform);
	}
}
