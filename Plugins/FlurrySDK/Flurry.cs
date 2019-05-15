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
using System.Collections.Generic;
using UnityEngine;

using FlurrySDKInternal;

namespace FlurrySDK
{
    /// <summary>
    /// A Unity plugin for Flurry SDK.
    /// The Flurry agent allows you to track the usage and behavior of your application
    /// on users' devices for viewing in the Flurry Analytics system.
    /// Set of methods that allow developers to capture detailed, aggregate information
    /// regarding the use of their app by end users.
    /// </summary>
    public class Flurry : IDisposable
    {
        // init static Flurry agent object.
        private static FlurryAgent flurryAgent;
        static Flurry()
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                flurryAgent = new FlurryAgentAndroid();
            }
#elif UNITY_IPHONE
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                flurryAgent = new FlurryAgentIOS();
            }
#else
            flurryAgent = null;
#endif
        }

        // Flurry SDK constants.

        /// <summary>
        /// Constants for setting log level in analytics SDK.
        /// </summary>
        public enum LogLevel
        {
            LogVERBOSE = 2,
            LogDEBUG   = 3,
            LogINFO    = 4,
            LogWARN    = 5,
            LogERROR   = 6,
            LogASSERT  = 7
        }

        /// <summary>
        /// Status for analytics event recording.
        /// </summary>
        public enum EventRecordStatus
        {
            FlurryEventFailed = 0,
            FlurryEventRecorded,
            FlurryEventUniqueCountExceeded,
            FlurryEventParamsCountExceeded,
            FlurryEventLogCountExceeded,
            FlurryEventLoggingDelayed,
            FlurryEventAnalyticsDisabled
        }

        /// <summary>
        /// Constants for setting user gender in analytics SDK.
        /// </summary>
        public enum Gender
        {
            Male,
            Female
        }

        /// <summary>
        /// Flurry message.
        /// </summary>
        public class FlurryMessage
        {
            public string Title;
            public string Body;
            public string ClickAction;
            public IDictionary<string, string> Data;
        }

        /// <summary>
        /// If listener is set, Flurry will call this method to notify you a notification has been received.
        /// </summary>
        public interface IFlurryMessagingListener
        {
            /// <summary>
            /// If listener is set, Flurry will call this method to notify you
            /// a notification has been received. If you would like to handle
            /// the notification yourself, be sure to return true to notify
            /// Flurry you've handled it. If you return false, Flurry will continue with
            /// default behavior, which is show the notification if app is in background,
            /// and do nothing if app is in foreground.
            /// </summary>
            /// <returns><c>true</c>, if you've handled the notification. <c>false</c> if you haven't and want Flurry to handle it.</returns>
            /// <param name="message">Message.</param>
            bool OnNotificationReceived(FlurryMessage message);

            /// <summary>
            /// If listener is set, Flurry will call this method to notify you
            /// a notification has been clicked. If you would like to handle
            /// the UI navigation yourself, be sure to return true to notify
            /// Flurry you've handled it.  If you return false, Flurry will continue with
            /// default behavior, which is launch the app or "click_action" activity.
            /// </summary>
            /// <returns><c>true</c>, if you've handled the notification. <c>false</c> if you haven't and want Flurry to handle it.</returns>
            /// <param name="message">Message.</param>
            bool OnNotificationClicked(FlurryMessage message);

            /// <summary>
            /// If listener is set, Flurry will notify you if user has cancelled/dismissed the notification.
            /// </summary>
            /// <returns><c>true</c>, if you've handled the notification. <c>false</c> if you haven't and want Flurry to handle it.</returns>
            /// <param name="message">Message.</param>
            void OnNotificationCancelled(FlurryMessage message);

            /// <summary>
            /// If listener is set, Flurry will notify you if push notification token has been changed.
            /// </summary>
            /// <param name="token">Token.</param>
            void OnTokenRefresh(string token);

            /// <summary>
            /// If listener is set, Flurry will notify you when a notification
            /// has been received that was not sent from Flurry. Based on the various
            /// push providers you have integrated, you may cast the Object to the appropriate type.
            /// </summary>
            /// <param name="nonFlurryMessage">Non flurry message.</param>
            void OnNonFlurryNotificationReceived(IDisposable nonFlurryMessage);

        }

        /// <summary>
        /// Builder Pattern class for Flurry
        /// </summary>
        public class Builder
        {
            private FlurryAgent.AgentBuilder builder;

            public Builder()
            {
#if UNITY_ANDROID
                if (Application.platform == RuntimePlatform.Android)
                {
                    builder = new FlurryAgentAndroid.AgentBuilderAndroid();
                }
#elif UNITY_IPHONE
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    builder = new FlurryAgentIOS.AgentBuilderIOS();
                }
#else
                builder = null;
#endif
            }

            public void Build(string apiKey)
            {
                if (builder != null)
                {
                    builder.Build(apiKey);
                }
            }

            /// <summary>
            /// True to enable or false to disable the ability to catch all uncaught exceptions and have them reported back to Flurry.
            /// </summary>
            /// <returns>The builder.</returns>
            /// <param name="crashReporting">If set to <c>true</c> to enable crash reporting.</param>
            public Builder WithCrashReporting(bool crashReporting = true)
            {
                if (builder != null)
                {
                    builder.WithCrashReporting(crashReporting);
                }
                return this;
            }

            /// <summary>
            /// Set the timeout for expiring a Flurry session.
            /// </summary>
            /// <returns>The builder.</returns>
            /// <param name="sessionMillis">Session timeout millis.</param>
            public Builder WithContinueSessionMillis(long sessionMillis = 10000)
            {
                if (builder != null)
                {
                    builder.WithContinueSessionMillis(sessionMillis);
                }
                return this;
            }

            /// <summary>
            /// True if this session should be added to total sessions/DAUs when applicationstate is inactive or background.
            /// </summary>
            /// <returns>The builder.</returns>
            /// <param name="includeBackgroundSessionsInMetrics">If set to <c>true</c> to include background sessions in metrics.</param>
            public Builder WithIncludeBackgroundSessionsInMetrics(bool includeBackgroundSessionsInMetrics = true)
            {
                if (builder != null)
                {
                    builder.WithIncludeBackgroundSessionsInMetrics(includeBackgroundSessionsInMetrics);
                }
                return this;
            }

            /// <summary>
            /// True to enable or false to disable the internal logging for the Flurry SDK.
            /// </summary>
            /// <returns>The builder.</returns>
            /// <param name="enableLog">If set to <c>true</c> to enable log.</param>
            public Builder WithLogEnabled(bool enableLog = true)
            {
                if (builder != null)
                {
                    builder.WithLogEnabled(enableLog);
                }
                return this;
            }

            /// <summary>
            /// Set the log level of the internal Flurry SDK logging.
            /// </summary>
            /// <returns>The builder.</returns>
            /// <param name="logLevel">Log level.</param>
            public Builder WithLogLevel(LogLevel logLevel = LogLevel.LogWARN)
            {
                if (builder != null)
                {
                    builder.WithLogLevel(logLevel);
                }
                return this;
            }

            /// <summary>
            /// Enable Flurry add-on Messaging.
            /// </summary>
            /// <returns>The builder.</returns>
            /// <param name="enableMessaging">If set to <c>true</c> to enable messaging.</param>
            public Builder WithMessaging(bool enableMessaging = true)
            {
                if (builder != null)
                {
                    builder.WithMessaging(enableMessaging);
                }
                return this;
            }

        }

        /// <summary>
        /// Sets the age of the user at the time of this session.
        /// </summary>
        /// <param name="age">Age.</param>
        public static void SetAge(int age)
        {
            if (flurryAgent != null)
            {
                flurryAgent.SetAge(age);
            }
        }

        /// <summary>
        /// Sets the gender of the user.
        /// </summary>
        /// <param name="gender">Gender.</param>
        public static void SetGender(Gender gender)
        {
            if (flurryAgent != null)
            {
                flurryAgent.SetGender(gender);
            }
        }

        /// <summary>
        /// Set whether Flurry should record location via GPS.
        /// </summary>
        /// <param name="reportLocation">If set to <c>true</c> report location.</param>
        public static void SetReportLocation(bool reportLocation)
        {
            if (flurryAgent != null)
            {
                flurryAgent.SetReportLocation(reportLocation);
            }
        }

        /// <summary>
        /// This method allows you to specify session origin and deep link for each session.
        /// </summary>
        /// <param name="originName">Origin name.</param>
        /// <param name="deepLink">Deep link.</param>
        public static void SetSessionOrigin(string originName, string deepLink)
        {
            if (flurryAgent != null)
            {
                flurryAgent.SetSessionOrigin(originName, deepLink);
            }
        }

        /// <summary>
        /// Sets the Flurry userId for this session.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        public static void SetUserId(string userId)
        {
            if (flurryAgent != null)
            {
                flurryAgent.SetUserId(userId);
            }
        }

        /// <summary>
        /// Set the version name of the app.
        /// </summary>
        /// <param name="versionName">Version name.</param>
        public static void SetVersionName(string versionName)
        {
            if (flurryAgent != null)
            {
                flurryAgent.SetVersionName(versionName);
            }
        }

        /// <summary>
        /// Add origin attribution.
        /// </summary>
        /// <param name="originName">Origin name.</param>
        /// <param name="originVersion">Origin version.</param>
        public static void AddOrigin(string originName, string originVersion)
        {
            if (flurryAgent != null)
            {
                flurryAgent.AddOrigin(originName, originVersion);
            }
        }

        /// <summary>
        /// Add origin attribution with parameters.
        /// </summary>
        /// <param name="originName">Origin name.</param>
        /// <param name="originVersion">Origin version.</param>
        /// <param name="originParameters">Origin parameters.</param>
        public static void AddOrigin(string originName, string originVersion, IDictionary<string, string> originParameters)
        {
            if (flurryAgent != null)
            {
                flurryAgent.AddOrigin(originName, originVersion, originParameters);
            }
        }

        /// <summary>
        /// This method allows you to associate parameters with an session.
        /// </summary>
        /// <param name="name">Property Name.</param>
        /// <param name="value">Property Value.</param>
        public static void AddSessionProperty(string name, string value)
        {
            if (flurryAgent != null)
            {
                flurryAgent.AddSessionProperty(name, value);
            }
        }

        /// <summary>
        /// Set a listener to listen notification events.
        /// </summary>
        /// <param name="flurryMessagingListener">Flurry messaging listener.</param>
        public static void SetMessagingListener(IFlurryMessagingListener flurryMessagingListener)
        {
            if (flurryAgent != null)
            {
                flurryAgent.SetMessagingListener(flurryMessagingListener);
            }
        }

        /// <summary>
        /// Get the version of the Flurry SDK.
        /// </summary>
        /// <returns>The agent version.</returns>
        public static int GetAgentVersion()
        {
            if (flurryAgent != null)
            {
                return flurryAgent.GetAgentVersion();
            }

            return 0;
        }

        /// <summary>
        /// Get the release version of the Flurry SDK.
        /// </summary>
        /// <returns>The release version.</returns>
        public static string GetReleaseVersion()
        {
            if (flurryAgent != null)
            {
                return flurryAgent.GetReleaseVersion();
            }

            return null;
        }

        /// <summary>
        /// Check to see if there is an active session.
        /// </summary>
        /// <returns>The session identifier.</returns>
        public static string GetSessionId()
        {
            if (flurryAgent != null)
            {
                return flurryAgent.GetSessionId();
            }

            return null;
        }

        /// <summary>
        /// Log an event.
        /// </summary>
        /// <returns>The event recording status.</returns>
        /// <param name="eventId">Event identifier.</param>
        public static EventRecordStatus LogEvent(string eventId)
        {
            if (flurryAgent != null)
            {
                return (EventRecordStatus) flurryAgent.LogEvent(eventId);
            }

            return EventRecordStatus.FlurryEventRecorded;
        }

        /// <summary>
        /// Log a timed event.
        /// </summary>
        /// <returns>The event recording status.</returns>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="timed">If set to <c>true</c> to log timed event.</param>
        public static EventRecordStatus LogEvent(string eventId, bool timed)
        {
            if (flurryAgent != null)
            {
                return (EventRecordStatus) flurryAgent.LogEvent(eventId, timed);
            }

            return EventRecordStatus.FlurryEventRecorded;
        }

        /// <summary>
        /// Log an event with parameters.
        /// </summary>
        /// <returns>The event recording status.</returns>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="parameters">Event parameters.</param>
        public static EventRecordStatus LogEvent(string eventId, IDictionary<string, string> parameters)
        {
            if (flurryAgent != null)
            {
                return (EventRecordStatus) flurryAgent.LogEvent(eventId, parameters);
            }

            return EventRecordStatus.FlurryEventRecorded;
        }

        /// <summary>
        /// Log a timed event with parameters.
        /// </summary>
        /// <returns>The event recording status.</returns>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="parameters">Event parameters.</param>
        /// <param name="timed">If set to <c>true</c> to log timed event.</param>
        public static EventRecordStatus LogEvent(string eventId, IDictionary<string, string> parameters, bool timed)
        {
            if (flurryAgent != null)
            {
                return (EventRecordStatus) flurryAgent.LogEvent(eventId, parameters, timed);
            }

            return EventRecordStatus.FlurryEventRecorded;
        }

        /// <summary>
        /// End a timed event.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        public static void EndTimedEvent(string eventId)
        {
            if (flurryAgent != null)
            {
                flurryAgent.EndTimedEvent(eventId);
            }
        }

        /// <summary>
        /// End a timed event.Only up to 10 unique parameters total can be passed for an event,
        /// including those passed when the event was initiated.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="parameters">Event parameters.</param>
        public static void EndTimedEvent(string eventId, IDictionary<string, string> parameters)
        {
            if (flurryAgent != null)
            {
                flurryAgent.EndTimedEvent(eventId, parameters);
            }
        }

        /// <summary>
        /// Log a page view.
        /// </summary>
        public static void OnPageView()
        {
            if (flurryAgent != null)
            {
                flurryAgent.OnPageView();
            }
        }

        /// <summary>
        /// Report errors that your app catches.
        /// </summary>
        /// <param name="errorId">Error identifier.</param>
        /// <param name="message">Error message.</param>
        /// <param name="errorClass">Error class.</param>
        public static void OnError(string errorId, string message, string errorClass)
        {
            if (flurryAgent != null)
            {
                flurryAgent.OnError(errorId, message, errorClass);
            }
        }

        /// <summary>
        /// Report errors that your app catches with parameters.
        /// </summary>
        /// <param name="errorId">Error identifier.</param>
        /// <param name="message">Error message.</param>
        /// <param name="errorClass">Error class.</param>
        /// <param name="parameters">Error parameters.</param>
        public static void OnError(string errorId, string message, string errorClass, IDictionary<string, string> parameters)
        {
            if (flurryAgent != null)
            {
                flurryAgent.OnError(errorId, message, errorClass, parameters);
            }
        }

        /// <summary>
        /// Logs the breadcrumb.
        /// </summary>
        /// <param name="crashBreadcrumb">Crash breadcrumb.</param>
        public static void LogBreadcrumb(string crashBreadcrumb)
        {
            if (flurryAgent != null)
            {
                flurryAgent.LogBreadcrumb(crashBreadcrumb);
            }
        }

        /// <summary>
        /// Log a payment.
        /// </summary>
        /// <returns>The payment recording status.</returns>
        /// <param name="productName">Product name.</param>
        /// <param name="productId">Product identifier.</param>
        /// <param name="quantity">Quantity.</param>
        /// <param name="price">Price.</param>
        /// <param name="currency">Currency.</param>
        /// <param name="transactionId">Transaction identifier.</param>
        /// <param name="parameters">Payment parameters.</param>
        public static EventRecordStatus LogPayment(
            string productName, string productId, int quantity, double price,
            string currency, string transactionId, IDictionary<string, string> parameters)
        {
            if (flurryAgent != null)
            {
                return (EventRecordStatus) flurryAgent.LogPayment(productName, productId, quantity, price,
                                                                  currency, transactionId, parameters);
            }

            return EventRecordStatus.FlurryEventRecorded;
        }

        /// <summary>
        /// Sets the iOS In-App Purchase reporting enabled.
        /// </summary>
        /// <param name="enableIAP">If set to <c>true</c> to enable iOS In-App Purchase.</param>
        public static void SetIAPReportingEnabled(bool enableIAP)
        {
            if (flurryAgent != null)
            {
                flurryAgent.SetIAPReportingEnabled(enableIAP);
            }
        }

        public void Dispose()
        {
            if (flurryAgent != null)
            {
                flurryAgent.Dispose();
            }
        }

    }
}
