#if UNITY_IOS
using System;
using System.Collections;
using Unity.Notifications;

namespace NotificationSamples.iOS
{
    /// <summary>
    /// iOS implementation of <see cref="IGameNotificationsPlatform"/>.
    /// </summary>
    public class iOSNotificationsPlatform : GameNotificationsPlatform
    {
        /// <summary>
        /// Instantiate a new instance of <see cref="iOSNotificationsPlatform"/>.
        /// </summary>
        public iOSNotificationsPlatform()
        {

        }
    }
}
#endif
