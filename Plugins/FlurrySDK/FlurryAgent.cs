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

namespace FlurrySDKInternal
{
    public abstract class FlurryAgent : IDisposable
    {
        public abstract class AgentBuilder
        {
            public abstract void Build(string apiKey);

            public abstract void WithCrashReporting(bool crashReporting);

            public abstract void WithContinueSessionMillis(long sessionMillis);

            public abstract void WithIncludeBackgroundSessionsInMetrics(bool includeBackgroundSessionsInMetrics);

            public abstract void WithLogEnabled(bool enableLog);

            public abstract void WithLogLevel(FlurrySDK.Flurry.LogLevel logLevel);
        }

        abstract public void SetAge(int age);

        abstract public void SetGender(FlurrySDK.Flurry.Gender gender);

        abstract public void SetReportLocation(bool reportLocation);

        abstract public void SetSessionOrigin(string originName, string deepLink);

        abstract public void SetUserId(string userId);

        abstract public void SetVersionName(string versionName);

        abstract public void AddOrigin(string originName, string originVersion);

        abstract public void AddOrigin(string originName, string originVersion, IDictionary<string, string> originParameters);

        abstract public void AddSessionProperty(string name, string value);

        abstract public int GetAgentVersion();

        abstract public string GetReleaseVersion();

        abstract public string GetSessionId();

        abstract public int LogEvent(string eventId);

        abstract public int LogEvent(string eventId, bool timed);

        abstract public int LogEvent(string eventId, IDictionary<string, string> parameters);

        abstract public int LogEvent(string eventId, IDictionary<string, string> parameters, bool timed);

        abstract public void EndTimedEvent(string eventId);

        abstract public void EndTimedEvent(string eventId, IDictionary<string, string> parameters);

        abstract public void OnPageView();

        abstract public void OnError(string errorId, string message, string errorClass);

        abstract public void OnError(string errorId, string message, string errorClass, IDictionary<string, string> parameters);

        abstract public void LogBreadcrumb(string crashBreadcrumb);

        abstract public int LogPayment(string productName, string productId, int quantity, double price,
                                       string currency, string transactionId, IDictionary<string, string> parameters);

        abstract public void SetIAPReportingEnabled(bool enableIAP);


        abstract public void Dispose();

    };
}
