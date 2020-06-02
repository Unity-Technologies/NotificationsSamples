using System;
using System.Linq;

namespace NotificationSamples
{
    /// <summary>
    /// Cross-platform wrapper to represent channels for notifications.
    /// </summary>
    /// <remarks>
    /// <para>On Android, this maps pretty closely to Android Notification Channels. On iOS, this does nothing.</para>
    /// <para>For projects targeting Android, you need to have at least one channel.</para>
    /// </remarks>
    public struct GameNotificationChannel
    {
        /// <summary>
        /// The style of notification shown for this channel. Corresponds to the Importance setting of
        /// an Android notification, and do nothing on iOS.
        /// </summary>
        public enum NotificationStyle
        {
            /// <summary>
            /// Notification does not appear in the status bar.
            /// </summary>
            None = 0,
            /// <summary>
            /// Notification makes no sound.
            /// </summary>
            NoSound = 2,
            /// <summary>
            /// Notification plays sound.
            /// </summary>
            Default = 3,
            /// <summary>
            /// Notification also displays a heads-up popup.
            /// </summary>
            Popup = 4
        }

        /// <summary>
        /// Controls how notifications display on the device lock screen.
        /// </summary>
        public enum PrivacyMode
        {
            /// <summary>
            /// Notifications aren't shown on secure lock screens.
            /// </summary>
            Secret = -1,
            /// <summary>
            /// Notifications display an icon, but content is concealed on secure lock screens.
            /// </summary>
            Private = 0,
            /// <summary>
            /// Notifications display on all lock screens.
            /// </summary>
            Public
        }

        /// <summary>
        /// The identifier for the channel.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// The name of the channel as displayed to the user.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The description of the channel as displayed to the user.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// A flag determining whether messages on this channel can show a badge. Defaults to true.
        /// </summary>
        public readonly bool ShowsBadge;

        /// <summary>
        /// A flag determining whether messages on this channel cause the device light to flash. Defaults to false.
        /// </summary>
        public readonly bool ShowLights;

        /// <summary>
        /// A flag determining whether messages on this channel cause the device to vibrate. Defaults to true.
        /// </summary>
        public readonly bool Vibrates;

        /// <summary>
        /// A flag determining whether messages on this channel bypass do not disturb settings. Defaults to false.
        /// </summary>
        public readonly bool HighPriority;

        /// <summary>
        /// The display style for this notification. Defaults to <see cref="NotificationStyle.Popup"/>.
        /// </summary>
        public readonly NotificationStyle Style;

        /// <summary>
        /// The privacy setting for this notification. Defaults to <see cref="PrivacyMode.Public"/>.
        /// </summary>
        public readonly PrivacyMode Privacy;

        /// <summary>
        /// The custom vibration pattern for this channel. Set to null to use the default.
        /// </summary>
        public readonly int[] VibrationPattern;

        /// <summary>
        /// Initialize a new instance of <see cref="GameNotificationChannel"/> with
        /// optional fields set to their default values.
        /// </summary>
        public GameNotificationChannel(string id, string name, string description) : this()
        {
            Id = id;
            Name = name;
            Description = description;

            ShowsBadge = true;
            ShowLights = false;
            Vibrates = true;
            HighPriority = false;
            Style = NotificationStyle.Popup;
            Privacy = PrivacyMode.Public;
            VibrationPattern = null;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="GameNotificationChannel"/>, providing the notification style
        /// and optionally all other settings.
        /// </summary>
        public GameNotificationChannel(string id, string name, string description, NotificationStyle style, bool showsBadge = true, bool showLights = false, bool vibrates = true, bool highPriority = false, PrivacyMode privacy = PrivacyMode.Public, long[] vibrationPattern = null)
        {
            Id = id;
            Name = name;
            Description = description;
            ShowsBadge = showsBadge;
            ShowLights = showLights;
            Vibrates = vibrates;
            HighPriority = highPriority;
            Style = style;
            Privacy = privacy;
            if (vibrationPattern != null)
                VibrationPattern = vibrationPattern.Select(v => (int)v).ToArray();
            else
                VibrationPattern = null;
        }
    }
}
