using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

	/// <summary>
	/// Standard serializer used by the <see cref="GameNotificationsManager"/> if no others
	/// are provided. Saves a simple binary format.
	/// </summary>
	public class DefaultSerializer : IPendingNotificationsSerializer
	{
		private const byte Version = 0;
		
		private readonly string filename;

		/// <summary>
		/// Instantiate a new instance of <see cref="DefaultSerializer"/>.
		/// </summary>
		/// <param name="filename">The filename to save to. This should be an absolute path.</param>
		public DefaultSerializer(string filename)
		{
			this.filename = filename;
		}

		/// <inheritdoc />
		public void Serialize(IList<PendingNotification> notifications)
		{
			// Filter out non rescheduling items
			var notificationsToSave = new List<PendingNotification>(notifications.Count);
			foreach (PendingNotification pendingNotification in notifications)
			{
				if (pendingNotification.Reschedule &&
				    pendingNotification.Notification.Scheduled &&
				    pendingNotification.Notification.DeliveryTime.HasValue)
				{
					notificationsToSave.Add(pendingNotification);
				}
			}
			
			using (var file = new FileStream(filename, FileMode.Create))
			{
				using (var writer = new BinaryWriter(file))
				{
					// Write version number
					writer.Write(Version);
					
					// Write list length
					writer.Write(notificationsToSave.Count);
					
					// Write each item
					foreach (PendingNotification notificationToSave in notificationsToSave)
					{
						IGameNotification notification = notificationToSave.Notification;
						
						// ID
						writer.Write(notification.Id.HasValue);
						if (notification.Id.HasValue)
						{
							writer.Write(notification.Id.Value);
						}
						
						// Title
						writer.Write(notification.Title ?? "");
						
						// Body
						writer.Write(notification.Body ?? "");
						
						// Subtitle
						writer.Write(notification.Subtitle ?? "");
						
						// Group
						writer.Write(notification.Group ?? "");
						
						// Badge
						writer.Write(notification.BadgeNumber.HasValue);
						if (notification.BadgeNumber.HasValue)
						{
							writer.Write(notification.BadgeNumber.Value);
						}
						
						// Time (must have a value)
						writer.Write(notification.DeliveryTime.Value.Ticks);
					}
				}
			}
		}

		/// <inheritdoc />
		public IList<IGameNotification> Deserialize(IGameNotificationsPlatform platform)
		{
			if (!File.Exists(filename))
			{
				return null;
			}

			using (var file = new FileStream(filename, FileMode.Open))
			{
				using (var reader = new BinaryReader(file))
				{
					// Version
					reader.ReadByte();
					
					// Length
					int numElements = reader.ReadInt32();
					
					var result = new List<IGameNotification>(numElements);
					for (var i = 0; i < numElements; ++i)
					{
						IGameNotification notification = platform.CreateNotification();
						bool hasValue;
						
						// ID
						hasValue = reader.ReadBoolean();
						if (hasValue)
						{
							notification.Id = reader.ReadInt32();
						}

						// Title
						notification.Title = reader.ReadString();
						
						// Body
						notification.Body = reader.ReadString();
						
						// Body
						notification.Subtitle = reader.ReadString();
						
						// Group
						notification.Group = reader.ReadString();
						
						// Badge
						hasValue = reader.ReadBoolean();
						if (hasValue)
						{
							notification.BadgeNumber = reader.ReadInt32();
						}
						
						// Time
						notification.DeliveryTime = new DateTime(reader.ReadInt64());

						result.Add(notification);
					}

					return result;
				}
			}
		}
	}
}
