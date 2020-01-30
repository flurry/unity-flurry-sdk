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

#import "FlurryUnityPlugin.h"
#import "Flurry.h"
#import "FlurryCCPA.h"
#import "FlurryUserProperties.h"

#if __has_include("FlurryMessaging.h")
#import "FlurryMessaging.h"
#endif

@implementation FlurryUnityPlugin

static FlurryUnityPlugin *_sharedInstance;

+ (FlurryUnityPlugin*) shared
{
    static dispatch_once_t once;
    static id _sharedInstance;
    dispatch_once(&once, ^{
        NSLog(@"Creating FlurryUnityPlugin shared instance");
        _sharedInstance = [[FlurryUnityPlugin alloc] init];
    });
    return _sharedInstance;
}

-(id)init {
    self = [super init];
    return self;
}

- (void) setupFlurryAutoMessaging {
    #if __has_include("FlurryMessaging.h")
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        [FlurryMessaging setAutoIntegrationForMessaging];
        FlurryUnityPlugin* sharedInstance = [FlurryUnityPlugin shared];
        [FlurryMessaging setMessagingDelegate: (id <FlurryMessagingDelegate>)  sharedInstance];
    });
    #endif
}

- (void) flurrySessionDidCreateWithInfo:  (NSDictionary *)  info
{
    NSLog(@"Flurry session started");
    
    NSString* originName = @"unity-flurry-sdk";
    
    #if __has_include("FlurryMessaging.h")
    NSString* originVersion = @"2.4.0.messaging";
    #else
    NSString* originVersion = @"2.4.0";
    #endif
    
    
    [Flurry addOrigin:originName withVersion:originVersion];
    
    //For use in testing Flurry push
    //NSString *idfv = UIDevice.currentDevice.identifierForVendor.UUIDString;
    //NSLog(@"IDFV = %@", idfv);
    
};

#if __has_include("FlurryMessaging.h")
-(void) didReceiveMessage:(nonnull FlurryMessage*)message {
    NSLog(@"didReceiveMessage = %@", [message description]);
    //App specific implementation
    
}

// delegate method when a notification action is performed
-(void) didReceiveActionWithIdentifier:(nullable NSString*)identifier message:(nonnull FlurryMessage*)message {
    NSLog(@"didReceiveAction %@ , Message = %@",identifier, [message description]);
    //Any app specific logic goes here.
    //Ex: Deeplink logic. See Flurry Push sample App (loading of viewControllers (nibs or storboards))
    
}
#endif

NSString* strToNSStr(const char* str)
{
    if (!str)
        return [NSString stringWithUTF8String: ""];
    
    return [NSString stringWithUTF8String: str];
}

char* strDup(const char* str)
{
    if (!str)
        return NULL;
    
    return strcpy((char*)malloc(strlen(str) + 1), str);
    
}

NSMutableDictionary* keyValueToDict(const char* keys, const char* values)
{
    if (!keys || !values)
    {
        return nil;
    }
    
    NSMutableDictionary* dict = [[NSMutableDictionary alloc] init];
    
    NSArray* keysArray = [strToNSStr(keys) componentsSeparatedByString : @"\n"];
    NSArray* valuesArray = [strToNSStr(values) componentsSeparatedByString : @"\n"];
    
    for (int i = 0; i < [keysArray count]; i++)
    {
        [dict setObject:[valuesArray objectAtIndex: i] forKey:[keysArray objectAtIndex: i]];
    }
    
    return dict;
}

NSArray* cStringToArray(const char* values) {
    if (!values) {
        return nil;
    }
    
    NSArray* valuesArray = [strToNSStr(values) componentsSeparatedByString : @"\n"];
    
    return valuesArray;
}

extern "C" {
    
    FlurrySessionBuilder* builder;
    bool FlurryLogEnabled = true;
    
    const void initializeFlurrySessionBuilder() {
        builder = [FlurrySessionBuilder new];
        FlurryUnityPlugin* sharedInstance = [FlurryUnityPlugin shared];
        [Flurry setDelegate: (id <FlurryDelegate>)  sharedInstance];
    }
    
    const void flurryWithCrashReporting(bool crashReporting){
        [builder withCrashReporting: crashReporting];
    }
    
    const void flurryWithDataSaleOptOut(bool isOptOut){
        [builder withDataSaleOptOut: isOptOut];
    }
    
    const void flurryWithLogLevel(int logLevel){
        
        if (FlurryLogEnabled) {
            if (logLevel == 2) {
                [builder withLogLevel: FlurryLogLevelAll];
            } else if (logLevel == 3 || logLevel == 4 || logLevel == 5) {
                [builder withLogLevel: FlurryLogLevelDebug];
            } else {
                [builder withLogLevel: FlurryLogLevelCriticalOnly]; //default
            }
        }
        
    }
    
    const void flurryWithLogEnabled(bool logEnabled){
        if (logEnabled == false) {
            [builder withLogLevel: FlurryLogLevelNone];
            FlurryLogEnabled = false;
        } else {
            FlurryLogEnabled = true;
        }
    }
    
    const void flurryWithSessionContinueSeconds(long seconds){
        [builder withSessionContinueSeconds: seconds];
    }
    
    const void flurryWithIncludeBackgroundSessionsInMetrics(bool includeBackgroundSessionsInMetrics){
        [builder withIncludeBackgroundSessionsInMetrics: includeBackgroundSessionsInMetrics];
    }
    
    const void flurryWithAppVersion(const char *appVersion){
        NSString *appVersionStr = strToNSStr(appVersion);
        [builder withAppVersion: appVersionStr];
    }
    
    const void flurryStartSessionWithSessionBuilder(const char *apiKey){
        NSString *apiKeyStr = strToNSStr(apiKey);
        if (![Flurry activeSessionExists]) {
            [Flurry startSession:apiKeyStr withSessionBuilder:builder];
        }
    }
    
    const void flurrySetupMessagingWithAutoIntegration() {
        FlurryUnityPlugin* sharedInstance = [FlurryUnityPlugin shared];
        [sharedInstance setupFlurryAutoMessaging];
    }
    
    const void flurrySetDataSaleOptOut(bool isOptOut){
        [FlurryCCPA setDataSaleOptOut: isOptOut];
    }
    
    const void flurrySetDelete(){
        [FlurryCCPA setDelete];
    }
    
    const int flurryLogEvent(const char *eventName){
        NSString *eventNameStr=strToNSStr(eventName);
        return [Flurry logEvent:eventNameStr];
    }
    
    const int flurryLogEventWithParameter(const char* eventName, const char* keys, const char* values){
        NSString *eventNameStr=strToNSStr(eventName);
        return [Flurry logEvent: eventNameStr withParameters:keyValueToDict(keys,values)];
    }
    
    const int flurryLogTimedEventWithParams(const char* eventName, const char* keys, const char* values, bool isTimed) {
        NSString *eventNameStr=strToNSStr(eventName);
        return [Flurry logEvent:eventNameStr withParameters:keyValueToDict(keys,values) timed:isTimed];
    }
    
    const void flurrySetUserId(const char* userId) {
        NSString *userIdStr = strToNSStr(userId);
        [Flurry setUserID:userIdStr];
    }
    
    const void flurrySetAge(const int age) {
        [Flurry setAge:age];
    }
    
    const void flurrySetGender(const char* gender) {
        NSString *genderStr = strToNSStr(gender);
        [Flurry setGender:genderStr];
    }
    
    const void flurryLogPageView() {
        [Flurry logPageView];
    }
    
    
    const void flurryLogError(const char* errorId, const char* message, const char* errorClass) {
        [Flurry logError:strToNSStr(errorId) message:strToNSStr(message) exception:[[NSException alloc] initWithName:strToNSStr(errorClass) reason:@"" userInfo:nil] withParameters: nil];
    }
    
    const void flurryLogErrorWithParams(const char* errorId, const char* message, const char* errorClass, const char* keys, const char* values) {
        
        [Flurry logError:strToNSStr(errorId) message:strToNSStr(message) exception:[[NSException alloc] initWithName:strToNSStr(errorClass) reason:@"" userInfo:nil] withParameters: keyValueToDict(keys,values)];
    }
    
    const void flurryAddOrigin(const char* originName, const char* originVersion) {
        NSString *originNameStr = strToNSStr(originName);
        NSString *originVersionStr = strToNSStr(originVersion);
        
        [Flurry addOrigin:originNameStr withVersion:originVersionStr];
    }
    
    const void flurrySetSessionOrigin(const char* originName, const char* deepLink) {
        NSString *originNameStr = strToNSStr(originName);
        NSString *deepLinkStr = strToNSStr(deepLink);
        
        [Flurry addSessionOrigin:originNameStr withDeepLink:deepLinkStr];
    }
    
    const void flurrySetVersionName(const char* versionName) {
        NSLog(@"SetVersionName is removed from the Flurry SDK. Use WithAppVersion in the session builder instead.");
    }
    
    const void flurryAddSessionProperty(const char* name, const char* value) {
        
        [Flurry sessionProperties: keyValueToDict(name, value)];
    }
    
    
    const void flurryAddOriginWithParams(const char* originName,const char* originVersion,const char* keys,const char* values){
        
        NSString *originNameStr = strToNSStr(originName);
        NSString *originVersionStr = strToNSStr(originVersion);
        
        [Flurry addOrigin: originNameStr withVersion: originVersionStr withParameters: keyValueToDict(keys,values)];
    }
    
    const char* flurryGetAgentVersion()
    {
        return strDup([[Flurry getFlurryAgentVersion] UTF8String]);
    }
    
    const char* flurryGetReleaseVersion()
    {
        return strDup([[Flurry getFlurryAgentVersion] UTF8String]);
    }
    
    const char* flurryGetSessionId()
    {
        return strDup([[Flurry getSessionID] UTF8String]);
    }
    
    const void flurryLogTimedEvent(const char* eventId, bool isTimed) {
        NSString *eventIdStr = strToNSStr(eventId);
        [Flurry logEvent:eventIdStr withParameters:nil timed:isTimed];
    }
    
    const void flurryEndTimedEvent(const char* eventName) {
        NSString *eventNameStr = strToNSStr(eventName);
        [Flurry endTimedEvent:eventNameStr withParameters:nil];
    }
    
    const void flurryEndTimedEventWithParams(const char* eventName, const char* keys,const char* values) {
        
        NSString *eventNameStr = strToNSStr(eventName);
        
        [Flurry endTimedEvent:eventNameStr withParameters:keyValueToDict(keys,values)];
    }
    
    const void flurryLogBreadcrumb(const char* crashBreadcrumb){
        
        
        NSString *crashBreadcrumbStr = strToNSStr(crashBreadcrumb);
        
        [Flurry leaveBreadcrumb:crashBreadcrumbStr];
    }
    
    const void flurrySetIAPReportingEnabled(bool enableIAP){
        [Flurry setIAPReportingEnabled: enableIAP];
    }
    
    const void flurrySetUserPropertyValues(const char* propertyName, const char* values){
        [FlurryUserProperties set:strToNSStr(propertyName) values:cStringToArray(values)];
    }
    
    const void flurrySetUserPropertyValue(const char* propertyName, const char* value){
        [FlurryUserProperties set:strToNSStr(propertyName) value:strToNSStr(value)];
    }
    
    const void flurryAddUserPropertyValues(const char* propertyName, const char* values){
        [FlurryUserProperties add:strToNSStr(propertyName) values:cStringToArray(values)];
    }
    
    const void flurryAddUserPropertyValue(const char* propertyName, const char* value){
        [FlurryUserProperties add:strToNSStr(propertyName) value:strToNSStr(value)];
    }
    
    const void flurryRemoveUserPropertyValues(const char* propertyName, const char* values){
        [FlurryUserProperties remove:strToNSStr(propertyName) values:cStringToArray(values)];
    }
    
    const void flurryRemoveUserPropertyValue(const char* propertyName, const char* value){
        [FlurryUserProperties remove:strToNSStr(propertyName) value:strToNSStr(value)];
    }
    
    const void flurryRemoveUserProperty(const char* propertyName){
        [FlurryUserProperties remove:strToNSStr(propertyName)];
    }
    
    const void flurryFlagUserProperty(const char* propertyName){
        [FlurryUserProperties flag:strToNSStr(propertyName)];
    }
}

@end
