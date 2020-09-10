#if UNITY_ANDROID
using UnityEngine;

namespace NotificationSamples.Android
{
    /// <summary>
    /// 
    /// </summary>
    public class AndroidNotificationCenterExtensions
    {
        private static bool initialized;
        private static AndroidJavaObject adroidNotificationExtensions;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

#if UNITY_EDITOR
            adroidNotificationExtensions = null;
            initialized = false;
#else
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            AndroidJavaClass managerClass = new AndroidJavaClass("com.unity.androidnotifications.UnityNotificationManager");
            AndroidJavaObject notificationManagerImpl = managerClass.CallStatic<AndroidJavaObject>("getNotificationManagerImpl", context, activity);
            AndroidJavaObject notificationManager = notificationManagerImpl.Call<AndroidJavaObject>("getNotificationManager");

            AndroidJavaClass pluginClass = new AndroidJavaClass("com.unity.androidnotifications.AndroidNotificationCenterExtensions");
            adroidNotificationExtensions = pluginClass.CallStatic<AndroidJavaObject>("getExtensionsImpl", context, notificationManager);

            initialized = true;
#endif
            return initialized;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool AreNotificationsEnabled()
        {
            if (!initialized)
            {
                // By default notifications are enabled
                return true;
            }

            return adroidNotificationExtensions.Call<bool>("areNotificationsEnabled");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public static bool AreNotificationsEnabled(string channelId)
        {
            if (!initialized)
            {
                // By default notifications are enabled
                return true;
            }

            return adroidNotificationExtensions.Call<bool>("areNotificationsEnabled", channelId);
        }
    }
}
#endif
