# Unity Flurry SDK (unity-flurry-sdk)

A Unity plugin for Flurry SDK

## Table of contents
- [Installation](#installation)
  - [Android](#android)
  - [iOS](#ios)
- [Example](#example)
- [API Reference](#api-reference)
- [Support](#support)
- [License](#license)

## Installation

1. Download the Flurry Unity package from [flurry-sdk-1.1.0.unitypackage](https://github.com/flurry/unity-flurry-sdk/raw/master/flurry-sdk-1.1.0.unitypackage).
2. Open your project in Unity Editor, choose menu **Assets** > **Import Package** > **Custom Package…** to bring up the File chooser, and select the package downloaded.
3. Add Flurry code
   
   ```c
   using FlurrySDK;
   ```

### Android

To improve analytics identities, please see [Manual Flurry Android SDK Integration](https://developer.yahoo.com/flurry/docs/integrateflurry/android-manual/) for adding Google Play Services library in your app by including `play-services-base` and `play-services-ads` libraries.

### iOS

There are some minor differences between the Android and iOS plugin:

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
        // Initialize Flurry once.
        new Flurry.Builder()
                  .WithCrashReporting(true)
                  .WithLogEnabled(true)
                  .WithLogLevel(Flurry.LogLevel.LogVERBOSE)
                  .Build(FLURRY_API_KEY);

        // Example to get Flurry versions.
        Debug.Log("AgentVersion: " + Flurry.GetAgentVersion());
        Debug.Log("ReleaseVersion: " + Flurry.GetReleaseVersion());

        // Set users preferences.
        Flurry.SetAge(36);
        Flurry.SetGender(Flurry.Gender.Female);
        Flurry.SetReportLocation(true);

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
    ...
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
  
## Support

- [Flurry Developer Support Site](https://developer.yahoo.com/flurry/docs/)

## License

Copyright 2018 Oath Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
