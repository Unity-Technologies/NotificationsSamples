#if UNITY_ANDROID
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace NotificationSamples.Android
{
    public class AndroidGameNotificationAuthorizationManager : IGameNotificationsAuthorizationManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad), Preserve]
        static void Register()
        {
            AndroidNotificationCenterExtensions.Initialize();
            GameNotificationsAuthorizationManager.RegisterImplementation(new AndroidGameNotificationAuthorizationManager());
        }

        public void AreNotificationsAuthorized(Action<bool> authorized)
        {
            authorized?.Invoke(AndroidNotificationCenterExtensions.AreNotificationsEnabled());
        }

        public void AreNotificationsAuthorized(string channelId, Action<bool> authorized)
        {
            authorized?.Invoke(AndroidNotificationCenterExtensions.AreNotificationsEnabled(channelId));
        }
    }
}
#endif
