# NotificationsSamples
This sample Unity projet demonstrate how to use the Unity Mobile Notifications API in real-world use cases, including a simple cross platform wrapper to show how to use the APIs.


# Game Notifications Manager

The primary component of the wrapper is the Game Notifications Manager. It is the interface through which you can schedule cross-platform local notifications.


### Usage



1.  Add the Game Notifications Manager to a Game Object (Note that the Manager's lifetime is currently limited to one scene. Up to you to add a DontDestroyOnLoad)
2.  Reference it from a game script that will send notifications.
3.  Initialize the Manager. Provide at least one channel if targeting Android. This should only be done once per application session.


```
var channel = new GameNotificationChannel(ChannelId, "Default Game Channel", "Generic notifications");
manager.Initialize(channel);

```

4.  Create a new notification:

```
IGameNotification notification = manager.CreateNotification();

```
(Note that the wrapper might return a null object on some platforms. Check for null before continuing.)
    
5.  Fill in the important fields


```
notification.Title = title;
notification.Body = body;
notification.DeliveryTime = deliveryTime;

```



6.  Schedule the notification.


```
manager.ScheduleNotification(notification);
```


The Manager saves a small file to disk whenever backgrounding so that it can keep track of notifications that were published and scheduled in previous sessions.


### Operation Modes

The manager features several operation modes that control its behaviour. The default operation mode has queueing, foreground clearing and automatic rescheduling all on.



*   **No queueing**

    The wrapper immediately schedules messages with the underlying operating system. Notifications can and will appear while the game is in the foreground.

*   **Queuing**

    The wrapper only schedules messages with the OS when backgrounding. Any messages that aren't shown (because the app is in the foreground) fire an expiry event. The system will also (optionally, but on by default) have the ability to calculate badge numbers automatically in this mode. If you don't provide any badge numbers manually with your notifications, the wrapper will set the badge numbers so that they increment based on the scheduled time of each notification.

*   **Foreground clearing**

    The wrapper will remove all scheduled messages when the app comes into the foreground.

*   **Automatic rescheduling**

    If this is set, after clearing all messages when foregrounding, the wrapper will go through all notifications marked this way, and put them back in the queue for delivery.


    To configure a notification for rescheduling:



```
var notificationToDisplay = manager.ScheduleNotification(notification);
notificationToDisplay.Reschedule = true;
```



# Platform Support notes

Each platform supported by the Game Notifications Manager is implemented by two types. To implement new platforms, simply implement a new IGameNotificationsPlatform and IGameNotification, and instantiate it in GameNotificationsManager.Initialize.


### IGameNotificationsPlatform 

Wrapper for interfacing with the platform operating system. Generally talks directly to AndroidNotificationCenter and iOSNotificationCenter. Also responsible for creating IGameNotifications.


### IGameNotification

Platform implementation of an instance of a notification.


# Demo Notes



*   Operates using all operation modes on by default.
*   Features a very simple clicker game. It will schedule local notifications whenever a cookie or cupcake is completed.
*   The News Feed button will retrieve the latest item from the Unity news RSS and schedule it as a notification 5 minutes in the future (configurable in GameController)
*   More Options contains a 'play reminder' button which will schedule an absence notification message for a fixed time of day the following day. (6am by default, configurable in GameController)


# Android icon feedback notes

The current preferences-based setup for Android icons makes a lot of sense to us as users. We only really have two points of feedback:



1.  If possible, being able to configure one icon as a default would be nice. If configured this way, it would override the Unity icon for all notifications where the icon isn't explicitly specified.
1.  There's possible fragility with using the string IDs for the icons. It is possible for things to break if an ID is changed in the settings without updating code, the icons could break. If it were possible to, at runtime, reference an asset that contains the ID, this might be safer, but we don't see a clean and obvious way to address this, and it might not be a concern anyway.
