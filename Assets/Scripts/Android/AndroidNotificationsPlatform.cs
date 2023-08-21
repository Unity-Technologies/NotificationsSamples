#if UNITY_ANDROID
using System;
using System.Collections;
using Unity.Notifications;

namespace NotificationSamples.Android
{
    /// <summary>
    /// Android implementation of <see cref="IGameNotificationsPlatform"/>.
    /// </summary>
    public class AndroidNotificationsPlatform : GameNotificationsPlatform
    {
        /// <summary>
        /// Gets or sets the default channel ID for notifications.
        /// </summary>
        /// <value>The default channel ID for new notifications, or null.</value>
        public string DefaultChannelId { get; set; }

        /// <summary>
        /// Instantiate a new instance of <see cref="AndroidNotificationsPlatform"/>.
        /// </summary>
        public AndroidNotificationsPlatform()
        {
        }
    }
}
#endif
