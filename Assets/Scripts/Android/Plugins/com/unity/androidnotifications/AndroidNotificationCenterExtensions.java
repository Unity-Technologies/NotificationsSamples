package com.unity.androidnotifications;

import android.app.AppOpsManager;
import android.app.NotificationManager;
import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.os.Build;

import java.lang.reflect.Field;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;

public class AndroidNotificationCenterExtensions
{
    private static final String CHECK_OP_NO_THROW = "checkOpNoThrow";
    private static final String OP_POST_NOTIFICATION = "OP_POST_NOTIFICATION";

    protected static AndroidNotificationCenterExtensions extentionsImpl;

    protected Context context = null;
    protected NotificationManager notificationManager = null;

    public AndroidNotificationCenterExtensions(Context context, NotificationManager notificationManager) {
        this.context = context;
        this.notificationManager = notificationManager;
    }

    // Called from managed code.
    public static AndroidNotificationCenterExtensions getExtensionsImpl(Context context,
        NotificationManager notificationManager) {
        if (extentionsImpl != null) {
            return extentionsImpl;
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            extentionsImpl = new AndroidNotificationCenterExtensionsOreo(context, notificationManager);
        }
        else {
            extentionsImpl = new AndroidNotificationCenterExtensions(context, notificationManager);
        }

        return extentionsImpl;
    }

    // Called from managed code
    public boolean areNotificationsEnabled() {
        // See https://stackoverflow.com/questions/11649151/android-4-1-how-to-check-notifications-are-disabled-for-the-application
        if (Build.VERSION.SDK_INT >= 24) {
            return notificationManager.areNotificationsEnabled();
        }
        else if (Build.VERSION.SDK_INT >= 19) {
            AppOpsManager appOps = (AppOpsManager)context.getSystemService(Context.APP_OPS_SERVICE);
            ApplicationInfo appInfo = context.getApplicationInfo();
            String pkg = context.getApplicationContext().getPackageName();
            int uid = appInfo.uid;
            try {
                Class appOpsClass = Class.forName(AppOpsManager.class.getName());
                Method checkOpNoThrowMethod = appOpsClass.getMethod(CHECK_OP_NO_THROW,
                    Integer.TYPE, Integer.TYPE, String.class);
                Field opPostNotificationValue = appOpsClass.getDeclaredField(OP_POST_NOTIFICATION);
                int value = (int)opPostNotificationValue.get(Integer.class);
                return (int)checkOpNoThrowMethod.invoke(appOps, value, uid, pkg) == AppOpsManager.MODE_ALLOWED;
            }
            catch (ClassNotFoundException | NoSuchMethodException | NoSuchFieldException |
                InvocationTargetException | IllegalAccessException | RuntimeException e) {
                    return true;
            }
        }
        else {
            return true;
        }
    }

    // Called from managed code
    public boolean areNotificationsEnabled(String channelId) {
        return areNotificationsEnabled();
    }
}
