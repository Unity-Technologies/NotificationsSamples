using System;
using TMPro;
using UnityEngine;

#if UNITY_ANDROID
using NotificationSamples.Android;
using Unity.Notifications.Android;
#endif

namespace NotificationSamples.Demo
{
	/// <summary>
	/// Manages the console on screen that displays information about notifications,
	/// and allows you to schedule more.
	/// </summary>
	public class NotificationConsole : MonoBehaviour
	{
		private const string ChannelId = "game_channel";

		[SerializeField]
		protected TMP_InputField titleField;
		
		[SerializeField]
		protected TMP_InputField bodyField;
		
		[SerializeField]
		protected GameNotificationsManager manager;

		private void Start()
		{
#if UNITY_ANDROID
			// Initialize Android channel
			var c = new AndroidNotificationChannel()
			{
				Id = ChannelId,
				Name = "Default Game Channel",
				Importance = Importance.Default,
				Description = "Generic notifications",
			};
			AndroidNotificationCenter.RegisterNotificationChannel(c);

			if (manager.Platform is AndroidNotificationsPlatform androidPlatform)
			{
				androidPlatform.DefaultChannelId = ChannelId;
			}
#endif
		}

		public void SendNotification()
		{
			IGameNotification notification = manager.CreateNotification();

			if (notification != null)
			{
				notification.Title = titleField.text;
				notification.Body = bodyField.text;
				notification.Group = ChannelId;
				notification.DeliveryTime = DateTime.Now + TimeSpan.FromMinutes(1);

				manager.ScheduleNotification(notification);
			}
		}
	}
}
