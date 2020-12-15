using System;
using UnityEngine;

namespace NotificationSamples
{
    public delegate IGameNotificationsPlatform GameNotificationsPlatformFactoryMethod(params GameNotificationChannel[] channels);

    public static class GameNotificationsPlatformFactory
    {
        private static GameNotificationsPlatformFactoryMethod factoryMethod;

        public static void RegisterFactoryMethod(Type type, GameNotificationsPlatformFactoryMethod factoryMethod)
        {
            GameNotificationsPlatformFactory.factoryMethod = factoryMethod;

            Debug.Log($"{type} registered.");
        }

        public static IGameNotificationsPlatform Create(params GameNotificationChannel[] channels)
        {
            return factoryMethod.Invoke(channels);
        }
    }
}
