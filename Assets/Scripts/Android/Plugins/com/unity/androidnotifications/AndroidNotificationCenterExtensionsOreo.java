package com.unity.androidnotifications;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.os.Build;

// From Orea notification channels are supported, and can be enabled individually
public class AndroidNotificationCenterExtensionsOreo extends AndroidNotificationCenterExtensions
{
    public AndroidNotificationCenterExtensionsOreo(Context context, NotificationManager notificationManager) {
        super(context, notificationManager);
    }

    // Called from managed code
    @Override
    public boolean areNotificationsEnabled(String channelId) {
        // See https://stackoverflow.com/questions/11649151/android-4-1-how-to-check-notifications-are-disabled-for-the-application
        if (super.areNotificationsEnabled()) {
            NotificationChannel channel = notificationManager.getNotificationChannel(channelId);

            // Channel has not yet been created, so permission is not yet denied
            if (channel == null) {
                return true;
            }

            // Check if notifications on given channel id are enabled
            if (channel.getImportance() != NotificationManager.IMPORTANCE_NONE) {
                return true;
            }
        }

        return false;
    }
}
