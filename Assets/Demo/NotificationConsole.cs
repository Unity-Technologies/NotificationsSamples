using System;
using NotificationSamples.Android;
using Unity.Notifications.Android;
using UnityEngine;

namespace NotificationSamples
{
	/// <summary>
	/// Manages the console on screen that displays information about notifications,
	/// and allows you to schedule more.
	/// </summary>
	public class NotificationConsole : MonoBehaviour
	{
		private const string channelId = "game_channel";
		
		[SerializeField]
		protected GameNotificationsManager manager;
		
		private void Start()
		{
#if UNITY_ANDROID
			// Initialize android channel
			var c = new AndroidNotificationChannel()
			{
				Id = channelId,
				Name = "Default Game Channel",
				Importance = Importance.Default,
				Description = "Generic notifications",
			};
			AndroidNotificationCenter.RegisterNotificationChannel(c);

			var androidPlatform = manager.Platform as AndroidNotificationsPlatform;
			if (androidPlatform != null)
			{
				androidPlatform.DefaultChannelId = channelId;
			}
#endif
		}
		
		public void SendNotification()
		{
			IGameNotification notification = manager.CreateNotification();

			if (notification != null)
			{
				notification.Title = "Notification sample";
				notification.Body = "This is a test notification";
				notification.Group = channelId;
				notification.DeliveryTime = DateTime.Now + TimeSpan.FromMinutes(2);
				
				manager.ScheduleNotification(notification);
			}
		}
	}
}
