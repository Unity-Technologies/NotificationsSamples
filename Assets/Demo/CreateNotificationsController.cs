using System;
using UnityEngine;

namespace NotificationSamples.Demo
{
    /// <summary>
    /// Controller to handle the <see cref="CreateNotificationItem"/> items.
    /// </summary>
    public class CreateNotificationsController : MonoBehaviour
    {
        /// <summary>
        /// Data for populating <see cref="CreateNotificationItem"/> items.
        /// </summary>
        [Serializable]
        public class CreateNotificationData
        {
            [Tooltip("Title to show in the notification.")]
            public string Title;
            [Tooltip("Description to show in the notification.")]
            public string Description;
            [Tooltip("Fire notification after this amount of minutes.")]
            public float Minutes;
            [Tooltip("Show this number on the badge.")]
            public int BadgeNumber;
        }

        [SerializeField, Tooltip("Reference to the notification console.")]
        protected NotificationConsole console;

        [SerializeField, Tooltip("Item prefab to clone.")]
        protected CreateNotificationItem createNotificationItemPrefab;

        [SerializeField, Tooltip("Parent to hold the items.")]
        protected Transform contentHolder;
        
        [SerializeField, Tooltip("List of data for populating the items.")]
        protected CreateNotificationData[] createNotificationData;
        
        // Create the <see cref="CreateNotificationItem"/> items.
        private void Awake()
        {
            for (int i = 0, len = createNotificationData.Length; i < len; i++)
            {
                CreateNotificationData data = createNotificationData[i];
                CreateNotificationItem item = Instantiate(createNotificationItemPrefab, contentHolder);
                item.Initialise(console, data.Title, data.Minutes, data.Description, data.BadgeNumber);
            }
        }
    }
}
