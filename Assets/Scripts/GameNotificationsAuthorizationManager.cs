using System;
using UnityEngine;

namespace NotificationSamples
{
    public interface IGameNotificationsAuthorizationManager
    {
        void AreNotificationsAuthorized(Action<bool> authorized);

        void AreNotificationsAuthorized(string channelId, Action<bool> authorized);
    }

    public static class GameNotificationsAuthorizationManager
    {
        private static IGameNotificationsAuthorizationManager manager;

        public static void RegisterImplementation(IGameNotificationsAuthorizationManager manager)
        {
            GameNotificationsAuthorizationManager.manager = manager;

            Debug.Log($"{manager.GetType()} registered.");
        }

        public static void AreNotificationsAuthorized(Action<bool> authorized)
        {
            manager.AreNotificationsAuthorized(authorized);
        }

        public static void AreNotificationsAuthorized(string channelId, Action<bool> authorized)
        {
            manager.AreNotificationsAuthorized(channelId, authorized);
        }
    }
}