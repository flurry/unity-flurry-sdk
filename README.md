# Unity Flurry SDK (unity-flurry-sdk)

A Unity plugin for Flurry SDK

**Flurry Push** for messaging is now supported by our plugin!

## Table of contents

- [Installation](#installation)
  - [Android](#android)
  - [iOS](#ios)
- [Example](#example)
- [API Reference](#api-reference)
- [Support](#support)
- [License](#license)

## Installation

1. Download the Flurry Unity package from [flurry-sdk-1.5.0.unitypackage](https://github.com/flurry/unity-flurry-sdk/raw/master/flurry-sdk-1.5.0.unitypackage).
2. Open your project in Unity Editor, choose menu **Assets** > **Import Package** > **Custom Package…** to bring up the File chooser, and select the package downloaded.
3. Add Flurry code

   ```c
   using FlurrySDK;
   ```

### Android

- To improve analytics identities, please see [Manual Flurry Android SDK Integration](https://developer.yahoo.com/flurry/docs/integrateflurry/android-manual/) for adding Google Play Services library in your app by including `play-services-base` and `play-services-ads` libraries.
- **Flurry Push**</br>
  In order to use [Flurry Push](https://developer.yahoo.com/flurry/docs/push/) for [Android](https://developer.yahoo.com/flurry/docs/push/integration/android/), please follow the additional steps below:
  1. Follow [Set up a Firebase Cloud Messaging client app with Unity](https://firebase.google.com/docs/cloud-messaging/unity/client). Complete to the 5th step for importing Firebase SDK. There should be a file `google-services.json` in your project's `Android` folder now. You do not need to provide any setup codes here.
  2. Configure an Android entry point Application. Please rename the following Android manifest template file `Assets/Plugins/Android/AndroidManifest_Flurry-template.xml` that comes with the Flurry SDK plugin to `AndroidManifest.xml`, and merge the contents with yours if needed.
  3. Update the metadata section in your `AndroidManifest.xml`.

     ```xml
      <!-- Flurry Agent settings; please update -->
      <meta-data android:name="flurry_apikey" android:value="FLURRY_ANDROID_API_KEY" />
      <meta-data android:name="flurry_with_crash_reporting" android:value="true" />
      <meta-data android:name="flurry_with_continue_session_millis" android:value="10000L" />
      <meta-data android:name="flurry_with_include_background_sessions_in_metrics" android:value="true" />
      <meta-data android:name="flurry_with_log_enabled" android:value="true" />
      <meta-data android:name="flurry_with_log_level" android:value="2" />
      <meta-data android:name="flurry_with_messaging" android:value="true" />
     ```

### iOS

For further details on configuring xcode for push notifications see here: [Flurry Push for Unity iOS](https://developer.yahoo.com/flurry/docs/push/integration/unityios/).

There are some minor differences between the Android and iOS plugin:

- iOS does not make use of the messaging listeners in C-sharp. Delegate methods didReceiveMessage/didReceiveActionWithIdentifier in FlurryUnityPlug.mm may be optionally modified to customize app behavior.
- iOS does not have an equivalent method for Android's GetReleaseVersion method.
- iOS does not yet have an equivalent method for Android's LogPayment method, however if SetIAPReportingEnabled is set to true Flurry will automatically track in app purchases.

## Example

```c
using System.Collections.Generic;
using UnityEngine;

using FlurrySDK;

public class FlurryStart : MonoBehaviour
{

#if UNITY_ANDROID
    private readonly string FLURRY_API_KEY = FLURRY_ANDROID_API_KEY;
#elif UNITY_IPHONE
    private readonly string FLURRY_API_KEY = FLURRY_IOS_API_KEY;
#else
    private readonly string FLURRY_API_KEY = null;
#endif

    void Start()
    {
        // Note: When enabling Messaging, Flurry Android should be initialized by using AndroidManifest.xml.
        // Initialize Flurry once.
        new Flurry.Builder()
                  .WithCrashReporting(true)
                  .WithLogEnabled(true)
                  .WithLogLevel(Flurry.LogLevel.LogVERBOSE)
                  .withMessaging(true);
                  .Build(FLURRY_API_KEY);

        // Example to get Flurry versions.
        Debug.Log("AgentVersion: " + Flurry.GetAgentVersion());
        Debug.Log("ReleaseVersion: " + Flurry.GetReleaseVersion());

        // Set users preferences.
        Flurry.SetAge(36);
        Flurry.SetGender(Flurry.Gender.Female);
        Flurry.SetReportLocation(true);

        // Set Messaging listener
        Flurry.SetMessagingListener(new MyMessagingListener());

        // Log Flurry events.
        Flurry.EventRecordStatus status = Flurry.LogEvent("Unity Event");
        Debug.Log("Log Unity Event status: " + status);

        // Log Flurry timed events with parameters.
        IDictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("Author", "Flurry");
        parameters.Add("Status", "Registered");
        status = Flurry.LogEvent("Unity Event Params Timed", parameters, true);
        Debug.Log("Log Unity Event with parameters timed status: " + status);
        ...
        Flurry.EndTimedEvent("Unity Event Params Timed");
    }

    public class MyMessagingListener : Flurry.IFlurryMessagingListener
    {
        // If you would like to handle the notification yourself, return true to notify Flurry
        // you've handled it, and Flurry will not show the notification.
        public bool OnNotificationReceived(Flurry.FlurryMessage message)
        {
            Debug.Log("Flurry Messaging Notification Received: " + message.Title);
            return false;
        }

        // If you would like to handle the notification yourself, return true to notify Flurry
        // you've handled it, and Flurry will not launch the app or "click_action" activity.
        public bool OnNotificationClicked(Flurry.FlurryMessage message)
        {
            Debug.Log("Flurry Messaging Notification Clicked: " + message.Title);
            return false;
        }

        public void OnNotificationCancelled(Flurry.FlurryMessage message)
        {
            Debug.Log("Flurry Messaging Notification Cancelled: " + message.Title);
        }

        public void OnTokenRefresh(string token)
        {
            Debug.Log("Flurry Messaging Token Refresh: " + token);
        }

        public void OnNonFlurryNotificationReceived(IDisposable nonFlurryMessage)
        {
            Debug.Log("Flurry Messaging Non-Flurry Notification.");
        }
    }
}
```

## API Reference

See [Android](http://flurry.github.io/flurry-android-sdk/)-[(FlurryAgent)](http://flurry.github.io/flurry-android-sdk/com/flurry/android/FlurryAgent.html) /
[iOS](http://flurry.github.io/flurry-ios-sdk/)-[(Flurry)](http://flurry.github.io/flurry-ios-sdk/interface_flurry.html) for the Flurry references.

- **Methods in Flurry.Builder to initialize Flurry Agent**

  ```c
  void Build(string apiKey);
  Flurry.Builder WithCrashReporting(bool crashReporting);
  Flurry.Builder WithContinueSessionMillis(long sessionMillis);
  Flurry.Builder WithIncludeBackgroundSessionsInMetrics(bool includeBackgroundSessionsInMetrics);
  Flurry.Builder WithLogEnabled(bool enableLog);
  Flurry.Builder WithLogLevel(FlurrySDK.Flurry.LogLevel logLevel);
  Flurry.Builder.withMessaging(bool enableMessaging);
  ```

- **Methods to set users preferences**

  ```c
  void SetAge(int age);
  void SetGender(Flurry.Gender gender);
  void SetReportLocation(bool reportLocation);
  void SetSessionOrigin(string originName, string deepLink);
  void SetUserId(string userId);
  void SetVersionName(string versionName);

  void AddOrigin(string originName, string originVersion);
  void AddOrigin(string originName, string originVersion, IDictionary<string, string> originParameters);
  void AddSessionProperty(string name, string value);
  ```

- **Methods to get Flurry versions**

  ```c
  int GetAgentVersion();
  string GetReleaseVersion();
  string GetSessionId();
  ```

- **Methods to log Flurry events**

  ```c
  Flurry.EventRecordStatus LogEvent(string eventId);
  Flurry.EventRecordStatus LogEvent(string eventId, bool timed);
  Flurry.EventRecordStatus LogEvent(string eventId, IDictionary<string, string> parameters);
  Flurry.EventRecordStatus LogEvent(string eventId, IDictionary<string, string> parameters, bool timed);

  void EndTimedEvent(string eventId);
  void EndTimedEvent(string eventId, IDictionary<string, string> parameters);

  void OnPageView();

  void OnError(string errorId, string message, string errorClass);
  void OnError(string errorId, string message, string errorClass, IDictionary<string, string> parameters);

  void LogBreadcrumb(string crashBreadcrumb);
  
  Flurry.EventRecordStatus LogPayment(string productName, string productId, int quantity, double price,
                                      string currency, string transactionId, IDictionary<string, string> parameters);
  ```

- **Methods to enable IAP reporting (iOS)**

  ```c
  void SetIAPReportingEnabled(bool enableIAP);
  ```

- **Methods for Messaging (Flurry Push)**

  ```c
  void SetMessagingListener(IFlurryMessagingListener flurryMessagingListener);

  interface IFlurryMessagingListener
  {
      bool OnNotificationReceived(FlurryMessage message);
      bool OnNotificationClicked(FlurryMessage message);
      void OnNotificationCancelled(FlurryMessage message);
      void OnTokenRefresh(string token);
      void OnNonFlurryNotificationReceived(IDisposable nonFlurryMessage);
  }

  class FlurryMessage
  {
      string Title;
      string Body;
      string ClickAction;
      IDictionary<string, string> Data;
  }
  ```

## Support

- [Flurry Developer Support Site](https://developer.yahoo.com/flurry/docs/)

## License

Copyright 2018 Oath Inc.

This project is licensed under the terms of the [Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0) open source license. Please refer to [LICENSE](LICENSE) for the full terms.
