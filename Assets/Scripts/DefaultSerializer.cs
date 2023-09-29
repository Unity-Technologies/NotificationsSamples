using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NotificationSamples
{
    /// <summary>
    /// Standard serializer used by the <see cref="GameNotificationsManager"/> if no others
    /// are provided. Saves a simple binary format.
    /// </summary>
    public class DefaultSerializer : IPendingNotificationsSerializer
    {
        private const byte Version = 1;

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
            try
            {
                using (var file = new FileStream(filename, FileMode.Create))
                {
                    using (var writer = new BinaryWriter(file))
                    {
                        // Write version number
                        writer.Write(Version);

                        // Write list length
                        writer.Write(notifications.Count);

                        // Write each item
                        foreach (PendingNotification notificationToSave in notifications)
                        {
                            GameNotification notification = notificationToSave.Notification;

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
                            //writer.Write(notification.Subtitle ?? "");

                            // Data
                            writer.Write(notification.Data ?? "");

                            // Badge
                            writer.Write(notification.BadgeNumber);

                            // Time (must have a value)
                            writer.Write(notificationToSave.DeliveryTime.Ticks);
                        }

                        writer.Flush();
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogException(e);
            }
        }

        /// <inheritdoc />
        public IList<PendingNotification> Deserialize(GameNotificationsPlatform platform)
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            try
            {
                using (var file = new FileStream(filename, FileMode.Open))
                {
                    using (var reader = new BinaryReader(file))
                    {
                        // Version
                        var version = reader.ReadByte();

                        // Length
                        int numElements = reader.ReadInt32();

                        var result = new List<PendingNotification>(numElements);
                        for (var i = 0; i < numElements; ++i)
                        {
                            GameNotification notification = platform.CreateNotification();
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

                            // Data, introduced in version 1
                            if (version > 0)
                                notification.Data = reader.ReadString();

                            // Badge
                            notification.BadgeNumber = reader.ReadInt32();

                            // Time
                            var deliveryTime = new DateTime(reader.ReadInt64(), DateTimeKind.Local);

                            result.Add(new PendingNotification(notification, deliveryTime));
                        }

                        return result;
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}
