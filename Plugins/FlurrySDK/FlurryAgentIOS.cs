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

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_IPHONE
namespace FlurrySDKInternal
{
    public class FlurryAgentIOS : FlurryAgent
    {
        public FlurryAgentIOS()
        {
            Debug.Log("FlurryAgentIOS instance created");
        }

        [DllImport("__Internal")]
        private static extern void initializeFlurrySessionBuilder();

        [DllImport("__Internal")]
        private static extern void flurryWithCrashReporting(bool crashReporting);

        [DllImport("__Internal")]
        private static extern void flurryWithLogLevel(int logLevel);

        [DllImport("__Internal")]
        private static extern void flurryWithLogEnabled(bool logEnabled);

        [DllImport("__Internal")]
        private static extern void flurryWithSessionContinueSeconds(long sessionMillis);

        [DllImport("__Internal")]
        private static extern void flurryWithAppVersion(string appVersion);

        [DllImport("__Internal")]
        private static extern void flurryWithIncludeBackgroundSessionsInMetrics(bool includeBackgroundSessionsInMetrics);

        [DllImport("__Internal")]
        private static extern void flurrySetupMessagingWithAutoIntegration();

        [DllImport("__Internal")]
        private static extern void flurryStartSessionWithSessionBuilder(string apiKey);
       
        [DllImport("__Internal")]
        private static extern int flurryLogEvent(string eventId);

        [DllImport("__Internal")]
        private static extern int flurryLogEventWithParameter(string eventName, string keys, string values);

        [DllImport("__Internal")]
        private static extern int flurryLogTimedEvent(string eventId, bool isTimed);

        [DllImport("__Internal")]
        private static extern int flurryLogTimedEventWithParams(string eventId, string keys, string values, bool isTimed);

        [DllImport("__Internal")]
        private static extern void flurryEndTimedEvent(string eventId);

        [DllImport("__Internal")]
        private static extern void flurryEndTimedEventWithParams(string eventId, string keys, string values);
        
        [DllImport("__Internal")]
        private static extern void flurrySetUserId(string userId);

        [DllImport("__Internal")]
        private static extern void flurrySetAge(int age);

        [DllImport("__Internal")]
        private static extern void flurrySetGender(string gender);

        [DllImport("__Internal")]
        private static extern void flurryLogPageView();

        [DllImport("__Internal")]
        private static extern void flurryLogError(string errorId, string message, string errorClass);

        [DllImport("__Internal")]
        private static extern void flurryLogErrorWithParams(string errorId, string message, string errorClass, string keys, string values);

        [DllImport("__Internal")]
        private static extern void flurryAddOrigin(string originName, string originVersion);

        [DllImport("__Internal")]
        private static extern void flurrySetSessionOrigin(string originName, string deepLink);

        [DllImport("__Internal")]
        private static extern void flurrySetVersionName(string versionName);

        [DllImport("__Internal")]
        private static extern void flurryAddSessionProperty(string name, string value);

        [DllImport("__Internal")]
        private static extern void flurryAddOriginWithParams(string originName, string originVersion, string keys, string values);

        [DllImport("__Internal")]
        private static extern string flurryGetAgentVersion();

        [DllImport("__Internal")]
        private static extern void flurryGetReleaseVersion();

        [DllImport("__Internal")]
        private static extern string flurryGetSessionId();

        [DllImport("__Internal")]
        private static extern void flurryLogBreadcrumb(string crashBreadcrumb);

        [DllImport("__Internal")]
        private static extern void flurrySetIAPReportingEnabled(bool isEnabled);

        public class AgentBuilderIOS : AgentBuilder
        {
            public AgentBuilderIOS()
            {
                initializeFlurrySessionBuilder();
            }

            public override void Build(string apiKey)
            {
                flurryStartSessionWithSessionBuilder(apiKey);
            }

            public override void WithCrashReporting(bool crashReporting)
            {
                flurryWithCrashReporting(crashReporting);
            }

            public override void WithContinueSessionMillis(long sessionMillis)
            {
                //iOS uses seconds rather than milliseconds
                flurryWithSessionContinueSeconds(sessionMillis / 1000);
            }

            public override void WithIncludeBackgroundSessionsInMetrics(bool includeBackgroundSessionsInMetrics)
            {
                flurryWithIncludeBackgroundSessionsInMetrics(includeBackgroundSessionsInMetrics);
            }

            public override void WithLogEnabled(bool enableLog)
            {
                flurryWithLogEnabled(enableLog);
            }
             
            public override void WithLogLevel(FlurrySDK.Flurry.LogLevel logLevel)
            {
                flurryWithLogLevel((int)logLevel);
            }

            public void WithAppVersion(string appVersion)
            {
                flurryWithAppVersion(appVersion);
            }

            public override void WithMessaging(bool enableMessaging)
            {
                if (enableMessaging)
                {
                    flurrySetupMessagingWithAutoIntegration();
                }
            }
        }

        public override void SetMessagingListener(FlurrySDK.Flurry.IFlurryMessagingListener flurryMessagingListener)
        {
            Debug.Log("iOS does not make use of the flurryMessagingListener. This is handled by delegate methods didReceiveMessage and didReceiveActionWithIdentifier in FlurryUnityPlugin.mm");
        }

        public override void SetAge(int age)
        {
           flurrySetAge(age);
        }

        public override void SetGender(FlurrySDK.Flurry.Gender gender)
        {
            if (gender == FlurrySDK.Flurry.Gender.Male)
            {
                flurrySetGender("m");
            }
            else if (gender == FlurrySDK.Flurry.Gender.Female)
            {
                flurrySetGender("f");
            }
        }

        public override void SetReportLocation(bool reportLocation)
        {
            Debug.Log("This method is applied based on the user permissions of the app");
        }

        public override void SetSessionOrigin(string originName, string deepLink)
        {
             flurrySetSessionOrigin(originName, deepLink);
        }

        public override void SetUserId(string userId)
        {
           flurrySetUserId(userId);
        }

        public override void SetVersionName(string versionName) 
        {
            flurrySetVersionName(versionName);
        }

        public override void AddOrigin(string originName, string originVersion)
        {
              flurryAddOrigin(originName, originVersion);
        }

        public override void AddOrigin(string originName, string originVersion, IDictionary<string, string> originParameters)
        {
            string keys, values;
            ToKeyValue(originParameters, out keys, out values);
            flurryAddOriginWithParams(originName, originVersion, keys, values);
        }

        public override void AddSessionProperty(string name, string value)
        {
            flurryAddSessionProperty(name, value);
        }

        public override int GetAgentVersion()
        { 
            string agentVersionStr = flurryGetAgentVersion();
            int agentVersion = 0;

            Int32.TryParse(agentVersionStr, out agentVersion);
            return agentVersion;
        }

        public override string GetReleaseVersion()
        { 
            Debug.Log("Flurry iOS SDK does not implement getReleaseVersion method.");
            return "1.0"; 
        }

        public override string GetSessionId()
        { 
            return flurryGetSessionId();
        }

        public override int LogEvent(string eventId)
        {
            return (int) flurryLogEvent(eventId);
        }

        public override int LogEvent(string eventId, IDictionary<string, string> parameters)
        {
            string keys, values;
            ToKeyValue(parameters, out keys, out values);

            return (int) flurryLogEventWithParameter(eventId, keys, values);
        }

        public override int LogEvent(string eventId, bool timed)
        { 
            return (int) flurryLogTimedEvent(eventId, timed);
        }

        public override int LogEvent(string eventId, IDictionary<string, string> parameters, bool timed)
        { 
            string keys, values;
            ToKeyValue(parameters, out keys, out values);
            return (int) flurryLogTimedEventWithParams(eventId, keys, values, timed);
        }

        public override void EndTimedEvent(string eventId)
        {
            flurryEndTimedEvent(eventId);
        }

        public override void EndTimedEvent(string eventId, IDictionary<string, string> parameters)
        {
            string keys, values;
            ToKeyValue(parameters, out keys, out values);
            flurryEndTimedEventWithParams(eventId, keys, values);
        }

        public override void OnPageView()
        {
            flurryLogPageView();
        }

        public override void OnError(string errorId, string message, string errorClass)
        {
           flurryLogError(errorId, message, errorClass);
        }

        public override void OnError(string errorId, string message, string errorClass, IDictionary<string, string> parameters)
        {
            string keys, values;
            ToKeyValue(parameters, out keys, out values);
            flurryLogErrorWithParams(errorId, message, errorClass, keys, values);
        }

        public override void LogBreadcrumb(string crashBreadcrumb)
        {
            flurryLogBreadcrumb(crashBreadcrumb);
        }

        public override int LogPayment(string productName, string productId, int quantity, double price,
                                       string currency, string transactionId, IDictionary<string, string> parameters)
        { 
            Debug.Log("Flurry iOS SDK does not implement LogPayment method.");
            return 1; 
        }

        public override void SetIAPReportingEnabled(bool enableIAP)
        {
            flurrySetIAPReportingEnabled(enableIAP);
        }

        public override void Dispose() { }

        private static void ToKeyValue(IDictionary<string, string> dictionary, out string keys, out string values)
        {
            var keysBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();
            int i = 0;
            int length = dictionary.Count;
            foreach (KeyValuePair<string, string> pair in dictionary)
            {
                keysBuilder.Append(pair.Key);
                valuesBuilder.Append(pair.Value);
                if (++i < length)
                {
                    keysBuilder.Append("\n");
                    valuesBuilder.Append("\n");
                }
            }
            keys = keysBuilder.ToString();
            values = valuesBuilder.ToString();
        }
    };
}
#endif
