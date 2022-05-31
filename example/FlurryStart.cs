/*
 * Copyright 2022, Yahoo Inc.
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

using System;
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
                  .WithMessaging(true, new MyMessagingListener())
                  .WithPerformanceMetrics(Flurry.Performance.ALL)
                  .Build(FLURRY_API_KEY);

        // Example to get Flurry versions.
        Debug.Log("AgentVersion: " + Flurry.GetAgentVersion());
        Debug.Log("ReleaseVersion: " + Flurry.GetReleaseVersion());

        // Set Flurry preferences.
        Flurry.SetLogEnabled(true);
        Flurry.SetLogLevel(Flurry.LogLevel.VERBOSE);

        // Set user preferences.
        Flurry.SetAge(36);
        Flurry.SetGender(Flurry.Gender.Female);
        Flurry.SetReportLocation(true);
  
        // Set user properties.
        Flurry.UserProperties.Set(Flurry.UserProperties.PROPERTY_REGISTERED_USER, "True");

        // Set Config, Messaging listener & Publisher Segmentation listener
        Flurry.Config.RegisterListener(new MyConfigListener());
        Flurry.Config.Fetch();

        // Set Messaging listener only for customizing notification by the native SDK.
        // Flurry.SetMessagingListener(new MyMessagingListener());

        Debug.Log("Flurry Publisher Segmentation Cache Data: " +
            PrintData<string, string>(Flurry.PublisherSegmentation.GetData()));
        Flurry.PublisherSegmentation.RegisterListener(new MyPublisherSegmentationListener());
        Flurry.PublisherSegmentation.Fetch();

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

        // Log Flurry standard events.
        status = Flurry.LogEvent(Flurry.Event.APP_ACTIVATED);
        Debug.Log("Log Unity Standard Event status: " + status);

        Flurry.EventParams stdParams = new Flurry.EventParams()
            .PutDouble(Flurry.EventParam.TOTAL_AMOUNT, 34.99)
            .PutBoolean(Flurry.EventParam.SUCCESS, true)
            .PutString(Flurry.EventParam.ITEM_NAME, "book 1")
            .PutString("note", "This is an awesome book to purchase !!!");
        status = Flurry.LogEvent(Flurry.Event.PURCHASED, stdParams);
        Debug.Log("Log Unity Standard Event with parameters status: " + status);
    }

    public class MyConfigListener : Flurry.IConfigListener
    {
        public void OnFetchSuccess()
        {
            Debug.Log("Config Fetch Completed with state: Success");
            Flurry.Config.Activate();
        }

        public void OnFetchNoChange()
        {
            Debug.Log("Config Fetch Completed with state: No Change");
            complete();
        }

        public void OnFetchError(bool isRetrying)
        {
            Debug.Log("Config Fetch Completed with state: Fail - " + (isRetrying ? "Retrying" : "End"));
            complete();
        }

        public void OnActivateComplete(bool isCache)
        {
            Debug.Log("Config Fetch Completed with state: Activate Completed - " + (isCache ? "Cached" : "New"));
            complete();
        }

        private void complete()
        {
            string welcome_message = Flurry.Config.GetString("welcome_message", "Welcome!");
            Debug.Log("Get Config Welcome message: " + welcome_message);
        }
    }

    public class MyMessagingListener : Flurry.IMessagingListener
    {
        // If you would like to handle the notification yourself, return true to notify Flurry
        // you've handled it, and Flurry will not show the notification.
        public bool OnNotificationReceived(Flurry.FlurryMessage message)
        {
            Debug.Log("Flurry Messaging Notification Received: "
                + message.Title + ", " + message.Body + ", " + message.ClickAction
                + PrintData<string, string>(message.Data));
            return false;
        }

        public bool OnNotificationClicked(Flurry.FlurryMessage message)
        {
            Debug.Log("Flurry Messaging Notification Clicked: "
                + message.Title + ", " + message.Body + ", " + message.ClickAction
                + PrintData<string, string>(message.Data));
            return false;
        }

        public void OnNotificationCancelled(Flurry.FlurryMessage message)
        {
            Debug.Log("Flurry Messaging Notification Cancelled: "
                + message.Title + ", " + message.Body + ", " + message.ClickAction
                + PrintData<string, string>(message.Data));
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

    public class MyPublisherSegmentationListener : Flurry.IPublisherSegmentationListener
    {
        public void OnFetched(IDictionary<string, string> data)
        {
            Debug.Log("Flurry Publisher Segmentation Fetched: "
                + PrintData<string, string>(data));
            Flurry.LogEvent("OnFetched() called in Unity");
        }
    }

    private static string PrintData<TKey, TValue>(IDictionary<TKey, TValue> data)
    {
        string list = "";
        if (data != null)
        {
            foreach (KeyValuePair<TKey, TValue> pair in data)
            {
                list += "\n    {" + pair.Key + ", " + pair.Value + "}";
            }
        }
        return list;
    }
}
