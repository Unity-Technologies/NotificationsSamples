using System;
using TMPro;
using UnityEngine;

namespace NotificationSamples.Demo
{
	/// <summary>
	/// UI item for creating a notification.
	/// </summary>
	public class CreateNotificationItem : MonoBehaviour
	{
		[SerializeField]
		protected TextMeshProUGUI titleLabel;
		[SerializeField]
		protected TextMeshProUGUI timeLabel;
		
		// References
		private NotificationConsole console;

		/// <summary>
		/// Title to show in the notification.
		/// </summary>
		public string Title => titleLabel.text;

		/// <summary>
		/// Description to show in the notification.
		/// </summary>
		public string Description { get; private set; }
		
		/// <summary>
		/// Show this number on the badge.
		/// </summary>
		public int BadgeNumber { get; private set; }
		
		/// <summary>
		/// Fire notification after this amount of minutes.
		/// </summary>
		public float Minutes { get; private set; }

		/// <summary>
		/// Initialise the item with the specified settings.
		/// </summary>
		public void Initialise(NotificationConsole notificationConsole, string title, float minutes, string description, 
		                       int badgeNumber)
		{
			console = notificationConsole;
			titleLabel.text = title;
			Minutes = minutes;
			timeLabel.text = $"{minutes} Minute(s)";
			Description = description;
			BadgeNumber = badgeNumber;
		}
		
		/// <summary>
		/// Called when the create button is clicked.
		/// </summary>
		public void OnCreate()
		{
			console.SendNotification(Title, Description, DateTime.Now + TimeSpan.FromMinutes(Minutes), BadgeNumber);
		}
	}
}
