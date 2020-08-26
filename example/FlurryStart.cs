/*
 * Copyright 2018, Oath Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
                  .WithLogLevel(Flurry.LogLevel.VERBOSE)
                  .WithMessaging(true)
                  .WithPerformanceMetrics(Flurry.Performance.ALL)
                  .Build(FLURRY_API_KEY);

        // Example to get Flurry versions.
        Debug.Log("AgentVersion: " + Flurry.GetAgentVersion());
        Debug.Log("ReleaseVersion: " + Flurry.GetReleaseVersion());

        // Set user preferences.
        Flurry.SetAge(36);
        Flurry.SetGender(Flurry.Gender.Female);
        Flurry.SetReportLocation(true);
  
        // Set user properties.
        Flurry.UserProperties.Set(Flurry.UserProperties.PROPERTY_REGISTERED_USER, "True");

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
        // ...
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
