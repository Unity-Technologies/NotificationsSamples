#if UNITY_IOS
using System;
using System.Collections;
using Unity.Notifications.iOS;
using UnityEngine;
using UnityEngine.Scripting;

namespace NotificationSamples.iOS
{
    public class iOSGameNotificationAuthorizationManager : IGameNotificationsAuthorizationManager
    {
        private iOSGameNotificationAuthorizationCoroutineRunner coroutineRunner;
        private Coroutine coroutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad), Preserve]
        static void Register()
        {
            GameNotificationsAuthorizationManager.RegisterImplementation(new iOSGameNotificationAuthorizationManager());
        }

        public void AreNotificationsAuthorized(Action<bool> authorized)
        {
            if (!coroutineRunner)
            {
                var gameObject = new GameObject("[GameNotificationAuthorizationCoroutineRunner]",
                    typeof(iOSGameNotificationAuthorizationCoroutineRunner));
                UnityEngine.Object.DontDestroyOnLoad(gameObject);

                coroutineRunner = gameObject.GetComponent<iOSGameNotificationAuthorizationCoroutineRunner>();
            }

            if (coroutine != null)
            {
                Debug.LogError("Already requesting notification authorization status!");
                return;
            }

            coroutine = coroutineRunner.StartCoroutine(DoAreNotificationsAuthorized(authorized));
        }

        public void AreNotificationsAuthorized(string channelId, Action<bool> authorized)
        {
            AreNotificationsAuthorized(authorized);
        }

        private IEnumerator DoAreNotificationsAuthorized(Action<bool> authorized)
        {
            var authorizationStatus = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            switch (authorizationStatus)
            {
                case AuthorizationStatus.NotDetermined:
                    // This bit is very game specific...
                    var authorizationOptions =
                        AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
                    using (var authorizationRequest = new AuthorizationRequest(authorizationOptions, false))
                    {
                        while (!authorizationRequest.IsFinished)
                        {
                            yield return null;
                        }
                    }
                    break;
            }

            coroutine = null;

            authorizationStatus = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            authorized?.Invoke(authorizationStatus == AuthorizationStatus.Authorized);
        }
    }
}
#endif
