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

package com.flurry.android;

import android.app.Application;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;

import com.flurry.android.FlurryAgent;
import com.flurry.android.marketing.FlurryMarketingModule;
import com.flurry.android.marketing.FlurryMarketingOptions;
import com.flurry.android.marketing.messaging.FlurryMessagingListener;
import com.flurry.android.marketing.messaging.notification.FlurryMessage;

/**
 * Unity Application for Flurry; to initialize Flurry Agent and Messaging from Application.
 */
public class FlurryUnityApplication extends Application {
    private static final String TAG = "FlurryUnityApplication";

    // Flurry metadata Keys
    private static final String FLURRY_KEY_APIKEY                                 = "flurry_apikey";
    private static final String FLURRY_KEY_WithCrashReporting                     = "flurry_with_crash_reporting";
    private static final String FLURRY_KEY_WithContinueSessionMillis              = "flurry_with_continue_session_millis";
    private static final String FLURRY_KEY_WithIncludeBackgroundSessionsInMetrics = "flurry_with_include_background_sessions_in_metrics";
    private static final String FLURRY_KEY_WithLogEnabled                         = "flurry_with_log_enabled";
    private static final String FLURRY_KEY_WithLogLevel                           = "flurry_with_log_level";
    private static final String FLURRY_KEY_WithMessaging                          = "flurry_with_messaging";

    // Flurry default settings
    private String  mApiKey;
    private boolean mWithCrashReporting                     = true;
    private long    mWithContinueSessionMillis              = 10000L;
    private boolean mWithIncludeBackgroundSessionsInMetrics = true;
    private boolean mWithLogEnabled                         = true;
    private int     mWithLogLevel                           = Log.WARN;
    private boolean mWithMessaging                          = false;

    private static FlurryMessagingListener sFlurryMessagingListener = null;

    @Override
    public void onCreate() {
        super.onCreate();

        // Get Flurry setting from metadata.
        Bundle bundle = null;
        try {
            ApplicationInfo info = getPackageManager().getApplicationInfo(getPackageName(), PackageManager.GET_META_DATA);
            bundle = info.metaData;
        } catch (PackageManager.NameNotFoundException e) {
            // no metadata;
            e.printStackTrace();
        }

        if (bundle != null) {
            mApiKey = bundle.getString(FLURRY_KEY_APIKEY);
            mWithCrashReporting = bundle.getBoolean(FLURRY_KEY_WithCrashReporting, mWithCrashReporting);

            String str = bundle.getString(FLURRY_KEY_WithContinueSessionMillis, Long.toString(mWithContinueSessionMillis));
            if (str.endsWith("L") || str.endsWith("l")) {
                str = str.substring(0, str.length()-1);
            }
            mWithContinueSessionMillis = Long.parseLong(str);

            mWithIncludeBackgroundSessionsInMetrics = bundle.getBoolean(FLURRY_KEY_WithIncludeBackgroundSessionsInMetrics,
                    mWithIncludeBackgroundSessionsInMetrics);
            mWithLogEnabled = bundle.getBoolean(FLURRY_KEY_WithLogEnabled, mWithLogEnabled);
            mWithLogLevel = bundle.getInt(FLURRY_KEY_WithLogLevel, mWithLogLevel);
            mWithMessaging = bundle.getBoolean(FLURRY_KEY_WithMessaging, mWithMessaging);

            Log.d(TAG, "Receive Flurry Metadata:" +
                    "\n    ApiKey: " + mApiKey +
                    "\n    CrashReporting: " + mWithCrashReporting +
                    "\n    ContinueSessionMillis: " + mWithContinueSessionMillis +
                    "\n    IncludeBackground: " + mWithIncludeBackgroundSessionsInMetrics +
                    "\n    LogEnabled: " + mWithLogEnabled +
                    "\n    LogLevel: " + mWithLogLevel +
                    "\n    Messaging: " + mWithMessaging);

            FlurryAgent.Builder builder = new FlurryAgent.Builder();

            // Set Flurry Messaging
            if (mWithMessaging) {
                UnityFlurryMessagingListener messagingListener = new UnityFlurryMessagingListener();

                FlurryMarketingOptions flurryMessagingOptions = new FlurryMarketingOptions.Builder()
                        .setupMessagingWithAutoIntegration()
                        .withFlurryMessagingListener(messagingListener)
                     // Define yours if needed
                     // .withDefaultNotificationChannelId(NOTIFICATION_CHANNEL_ID)
                     // .withDefaultNotificationIconResourceId(R.mipmap.ic_launcher_round)
                     // .withDefaultNotificationIconAccentColor(getResources().getColor(R.color.colorPrimary))
                        .build();

                FlurryMarketingModule marketingModule = new FlurryMarketingModule(flurryMessagingOptions);
                builder.withModule(marketingModule);
            }

            // Init Flurry
            builder
                    .withCaptureUncaughtExceptions(mWithCrashReporting)
                    .withContinueSessionMillis(mWithContinueSessionMillis)
                    .withIncludeBackgroundSessionsInMetrics(mWithIncludeBackgroundSessionsInMetrics)
                    .withLogEnabled(mWithLogEnabled)
                    .withLogLevel(mWithLogLevel)
                    .build(this, mApiKey);
        }
    }

    /**
     * Set FlurryMessagingListener.
     * @param messagingListener FlurryMessagingListener object
     */
    public static void withFlurryMessagingListener(FlurryMessagingListener messagingListener) {
        sFlurryMessagingListener = messagingListener;
    }

    /**
     * Wrapper Flurry Messaging listenet.
     */
    class UnityFlurryMessagingListener implements FlurryMessagingListener {

        @Override
        public boolean onNotificationReceived(FlurryMessage flurryMessage) {
            if (sFlurryMessagingListener != null) {
                return sFlurryMessagingListener.onNotificationReceived(flurryMessage);
            }
            return false;
        }

        @Override
        public boolean onNotificationClicked(FlurryMessage flurryMessage) {
            if (sFlurryMessagingListener != null) {
                return sFlurryMessagingListener.onNotificationClicked(flurryMessage);
            }
            return false;
        }

        @Override
        public void onNotificationCancelled(FlurryMessage flurryMessage) {
            if (sFlurryMessagingListener != null) {
                sFlurryMessagingListener.onNotificationCancelled(flurryMessage);
            }
        }

        @Override
        public void onTokenRefresh(String token) {
            if (sFlurryMessagingListener != null) {
                sFlurryMessagingListener.onTokenRefresh(token);
            }
        }

        @Override
        public void onNonFlurryNotificationReceived(Object message) {
            if (sFlurryMessagingListener != null) {
                sFlurryMessagingListener.onNonFlurryNotificationReceived(message);
            }
        }
    }

}

