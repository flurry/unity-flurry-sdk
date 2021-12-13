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
#import "Flurry+Event.h"
#import "FlurryCCPA.h"
#import "FlurryUserProperties.h"

#if __has_include("FlurryMessaging.h")
#import "FlurryMessaging.h"
#endif

#if __has_include(<StoreKit/SKAdNetwork.h>)
#import "FlurrySKAdNetwork.h"
#endif

typedef void (*OnFetched) (const char *data);
typedef void (*OnPSFetched) (const char *data);

typedef void (*OnConfigFetched)();
typedef void (*OnConfigFetchNoChange)();
typedef void (*OnConfigFetchFailed)();
typedef void (*OnConfigActivated)();

typedef void (*OnNotificationReceived)(const char *title, const char *body, const char *sound, const char *appData);
typedef void (*OnNotificationClicked)(const char *title, const char *body, const char *sound, const char *appData);

static OnPSFetched onPSFetchedBlock;

static OnConfigFetched onConfigFetchedBlock;
static OnConfigFetchNoChange onConfigFetchNoChangeBlock;
static OnConfigFetchFailed onConfigFetchFailedBlock;
static OnConfigActivated onConfigActivatedBlock;

static OnNotificationReceived onNotificationReceivedBlock;
static OnNotificationClicked onNotificationClickedBlock;

// TODO: Deprecated, need to remove for next GA release
static OnFetched onFetchedBlock;

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
    NSString* originVersion = @"4.2.0.messaging";
    #else
    NSString* originVersion = @"4.2.0";
    #endif
    
    
    [Flurry addOrigin:originName withVersion:originVersion];
    
    //For use in testing Flurry push
    //NSString *idfv = UIDevice.currentDevice.identifierForVendor.UUIDString;
    //NSLog(@"IDFV = %@", idfv);
    
};

- (void)onFetched:(NSDictionary<NSString *, NSString *> *_Nullable)publisherData{
    NSLog(@"onFetched native callback= %@", publisherData);
    if(publisherData){
        onFetchedBlock(strDup([dictionaryToNSString(publisherData) UTF8String]));
        onPSFetchedBlock(strDup([dictionaryToNSString(publisherData) UTF8String]));
    }else{
        onFetchedBlock("");
        onPSFetchedBlock("");
        
        
    }
}

- (void) fetchComplete{
    onConfigFetchedBlock();
}


- (void) fetchCompleteNoChange{
    onConfigFetchNoChangeBlock();
}


- (void) fetchFail{
    onConfigFetchFailedBlock();
}

- (void) activationComplete{
    onConfigActivatedBlock();
}

#if __has_include("FlurryMessaging.h")
-(void) didReceiveMessage:(nonnull FlurryMessage*)message {
    NSLog(@"didReceiveMessage = %@", [message description]);
    
    onNotificationReceivedBlock(strDup([message.title UTF8String]), strDup([message.body UTF8String]), strDup([message.sound UTF8String]), strDup([dictionaryToNSString(message.appData) UTF8String]));

}

// delegate method when a notification action is performed
-(void) didReceiveActionWithIdentifier:(nullable NSString*)identifier message:(nonnull FlurryMessage*)message {
    NSLog(@"didReceiveAction %@ , Message = %@",identifier, [message description]);
    
    onNotificationClickedBlock(strDup([message.title UTF8String]), strDup([message.body UTF8String]), strDup([message.sound UTF8String]), strDup([dictionaryToNSString(message.appData) UTF8String]));
    
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

NSString* dictionaryToNSString(NSDictionary<NSString *, NSString *> *dict){
    NSMutableString *mutableStr = [NSMutableString new];
    for(NSString *key in [dict allKeys]){
        NSString *val = dict[key];
        [mutableStr appendString:key];
        [mutableStr appendString:@":"];
        [mutableStr appendString:val];
        [mutableStr appendString:@";"];
    }
    if([mutableStr length] > 0){
        [mutableStr deleteCharactersInRange:NSMakeRange([mutableStr length] - 1, 1)];
    }
    return [mutableStr copy];
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
    
    const int flurryLogStandardEventWithParameter(const char* eventName, const char* keys, const char* values){
        NSString *eventNameStr = strToNSStr(eventName);
        NSMutableDictionary *tmpParam = keyValueToDict(keys,values);
        
        NSDictionary *eventIdStringMap = @{
              @"AD_CLICK":@(0),
              @"AD_IMPRESSION":@(1),
              @"AD_REWARDED":@(2),
              @"AD_SKIPPED":@(3),
              @"CREDITS_SPENT":@(4),
              @"CREDITS_PURCHASED":@(5),
              @"CREDITS_EARNED":@(6),
              @"ACHIEVEMENT_UNLOCKED":@(7),
              @"LEVEL_COMPLETED":@(8),
              @"LEVEL_FAILED":@(9),
              @"LEVEL_UP":@(10),
              @"LEVEL_STARTED":@(11),
              @"LEVEL_SKIP":@(12),
              @"SCORE_POSTED":@(13),
              @"CONTENT_RATED":@(14),
              @"CONTENT_VIEWED":@(15),
              @"CONTENT_SAVED":@(16),
              @"PRODUCT_CUSTOMIZED":@(17),
              @"APP_ACTIVATED":@(18),
              @"APPLICATION_SUBMITTED":@(19),
              @"ADD_ITEM_TO_CART":@(20),
              @"ADD_ITEM_TO_WISH_LIST":@(21),
              @"COMPLETED_CHECKOUT":@(22),
              @"PAYMENT_INFO_ADDED":@(23),
              @"ITEM_VIEWED":@(24),
              @"ITEM_LIST_VIEWED":@(25),
              @"PURCHASED":@(26),
              @"PURCHASE_REFUNDED":@(27),
              @"REMOVE_ITEM_FROM_CART":@(28),
              @"CHECKOUT_INITIATED":@(29),
              @"FUNDS_DONATED":@(30),
              @"USER_SCHEDULED":@(31),
              @"OFFER_PRESENTED":@(32),
              @"SUBSCRIPTION_STARTED":@(33),
              @"SUBSCRIPTION_ENDED":@(34),
              @"GROUP_JOINED":@(35),
              @"GROUP_LEFT":@(36),
              @"TUTORIAL_STARTED":@(37),
              @"TUTORIAL_COMPLETED":@(38),
              @"TUTORIAL_STEP_COMPLETED":@(39),
              @"TUTORIAL_SKIPPED":@(40),
              @"LOGIN":@(41),
              @"LOGOUT":@(42),
              @"USER_REGISTERED":@(43),
              @"SEARCH_RESULT_VIEWED":@(44),
              @"KEYWORD_SEARCHED":@(45),
              @"LOCATION_SEARCHED":@(46),
              @"INVITE":@(47),
              @"SHARE":@(48),
              @"LIKE":@(49),
              @"COMMENT":@(50),
              @"MEDIA_CAPTURED":@(51),
              @"MEDIA_STARTED":@(52),
              @"MEDIA_STOPPED":@(53),
              @"MEDIA_PAUSED":@(54),
              @"PRIVACY_PROMPT_DISPLAYED":@(55),
              @"PRIVACY_OPT_IN":@(56),
              @"PRIVACY_OPT_OUT":@(57)
        };
        
        NSDictionary *eventParamStringMap = @{
            @"AD_TYPE" : @"fl.ad.type",
            @"LEVEL_NAME" : @"fl.level.name",
            @"LEVEL_NUMBER" : @"fl.level.number",
            @"CONTENT_NAME" : @"fl.content.name",
            @"CONTENT_TYPE" : @"fl.content.type",
            @"CONTENT_ID" : @"fl.content.id",
            @"CREDIT_NAME" : @"fl.credit.name",
            @"CREDIT_TYPE" : @"fl.credit.type",
            @"CREDIT_ID" : @"fl.credit.id",
            @"IS_CURRENCY_SOFT" : @"fl.is.currency.soft",
            @"CURRENCY_TYPE" : @"fl.currency.type",
            @"PAYMENT_TYPE" : @"fl.payment.type",
            @"ITEM_NAME" : @"fl.item.name",
            @"ITEM_TYPE" : @"fl.item.type",
            @"ITEM_ID" : @"fl.item.id",
            @"ITEM_COUNT" : @"fl.item.count",
            @"ITEM_CATEGORY" : @"fl.item.category",
            @"ITEM_LIST_TYPE" : @"fl.item.list.type",
            @"PRICE" : @"fl.price",
            @"TOTAL_AMOUNT" : @"fl.total.amount",
            @"ACHIEVEMENT_ID" : @"fl.achievement.id",
            @"SCORE" : @"fl.score",
            @"RATING" : @"fl.rating",
            @"TRANSACTION_ID" : @"fl.transaction.id",
            @"SUCCESS" : @"fl.success",
            @"IS_ANNUAL_SUBSCRIPTION" : @"fl.is.annual.subscription",
            @"SUBSCRIPTION_COUNTRY" : @"fl.subscription.country",
            @"TRIAL_DAYS" : @"fl.trial.days",
            @"PREDICTED_LTV" : @"fl.predicted.ltv",
            @"GROUP_NAME" : @"fl.group.name",
            @"TUTORIAL_NAME" : @"fl.tutorial.name",
            @"STEP_NUMBER" : @"fl.step.number",
            @"USER_ID" : @"fl.user.id",
            @"METHOD" : @"fl.method",
            @"QUERY" : @"fl.query",
            @"SEARCH_TYPE" : @"fl.search.type",
            @"SOCIAL_CONTENT_NAME" : @"fl.social.content.name",
            @"SOCIAL_CONTENT_ID" : @"fl.social.content.id",
            @"LIKE_TYPE" : @"fl.like.type",
            @"MEDIA_NAME" : @"fl.media.name",
            @"MEDIA_TYPE" : @"fl.media.type",
            @"MEDIA_ID" : @"fl.media.id",
            @"DURATION" : @"fl.duration",
        };
        
        int eventId = [eventIdStringMap[eventNameStr] intValue];
        FlurryParamBuilder *builder = [FlurryParamBuilder new];
        for(NSString *key in [tmpParam allKeys]){
            if(eventParamStringMap[key] != nil){
                [builder setString: tmpParam[key] forKey: eventParamStringMap[key]];
            }else{
                [builder setString: tmpParam[key] forKey:key];
            }
        }
        return [Flurry logStandardEvent:(FlurryEvent)eventId withParameters:builder];
    }
    const void flurrySetUserId(const char* userId) {
        NSString *userIdStr = strToNSStr(userId);
        [Flurry setUserID:userIdStr];
    }

    const void flurrySetSessionContinueSeconds(long seconds){
        [Flurry setSessionContinueSeconds: seconds];
    }

    const void flurrySetIncludeBackgroundSessionsInMetrics(bool includeBackgroundSessionsInMetrics) {
        [Flurry setCountBackgroundSessions:includeBackgroundSessionsInMetrics];
    }
    
    const void flurrySetAge(const int age) {
        [Flurry setAge:age];
    }
    
    const void flurrySetGender(const char* gender) {
        NSString *genderStr = strToNSStr(gender);
        [Flurry setGender:genderStr];
    }
    
    const void flurryLogPageView() {
       // [Flurry logPageView];
        NSLog(@"[Flurry logPageView] is removed in Flurry 11.0.0");
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
        NSString *versionNameStr = strToNSStr(versionName);
        [Flurry setAppVersion:versionNameStr];
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

    const void flurryLogPayment(const char* productName, const char* productId, const int quantity, const double price, const char* currency, const char* transactionId, const char* keys, const char* values) {
        NSString *productNameStr = strToNSStr(productName);
        NSString *productIdStr = strToNSStr(productId);
        NSString *currencyStr = strToNSStr(currency);
        NSString *transactionIdStr = strToNSStr(transactionId);

        [Flurry logPaymentTransactionWithTransactionId:transactionIdStr
                productId:productIdStr
                quantity:quantity
                price:price
                currency:currencyStr
                productName:productNameStr
                transactionState:FlurryPaymentTransactionStatePurchasing
                userDefinedParams:keyValueToDict(keys,values)
                statusCallback:nil];
    }
    
    const void flurrySetIAPReportingEnabled(bool enableIAP){
        [Flurry setIAPReportingEnabled: enableIAP];
    }
    
    const void flurryOpenPrivacyDashboard(){
        [Flurry openPrivacyDashboard:^(BOOL success) {
            NSLog(@"Flurry privacy dashboard opened successfully.");
        }];
    }
    
    const void flurryUpdateConversionValue(int conversionValue){
        if (@available(iOS 14.0, *)) {
            [FlurrySKAdNetwork flurryUpdateConversionValue: conversionValue];
        }
    }
    
    const void flurryUpdateConversionValueWithEvent(int flurryEvent){
        if (@available(iOS 14.0, *)) {
            [FlurrySKAdNetwork flurryUpdateConversionValueWithEvent:(FlurryConversionValueEventType) flurryEvent];
        }
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
    
    const void flurryFetchPublisherSegmentation(){
        [Flurry fetch];
    }
    
    const void flurrySetPublisherSegmentationListener(){
        FlurryUnityPlugin* sharedInstance = [FlurryUnityPlugin shared];
        [Flurry registerFetchObserver:sharedInstance withExecutionQueue:dispatch_get_main_queue()];
    }
    
    const char* flurryGetPublisherData(){
        NSDictionary<NSString *, NSString *> *data = [Flurry getPublisherData];
        return strDup([dictionaryToNSString(data) UTF8String]);
    }
    
    const void flurryRegisterOnPSFetchedCallback(OnPSFetched handler){
        onPSFetchedBlock = handler;
    }
    
    // TODO: Deprecated, and need to remove for next GA release
    const void flurryRegisterOnFetchedCallback(OnFetched handler)
    {
        onFetchedBlock = handler;
    }
    
    const void flurrySetConfigListener(){
#if __has_include("FConfig.h")
        FlurryUnityPlugin* sharedInstance = [FlurryUnityPlugin shared];
        [[FConfig sharedInstance] registerObserver:sharedInstance withExecutionQueue:dispatch_get_main_queue()];
#endif
    }
    
    const void flurryRegisterConfigCallback(OnConfigFetched handler1, OnConfigFetchNoChange handler2, OnConfigFetchFailed handler3, OnConfigActivated handler4){
        
#if __has_include("FConfig.h")
        onConfigFetchedBlock = handler1;
        onConfigFetchNoChangeBlock = handler2;
        onConfigFetchFailedBlock = handler3;
        onConfigActivatedBlock = handler4;
#endif

    }
    
    const void flurryConfigFetch(){
#if __has_include("FConfig.h")
        [[FConfig sharedInstance] fetch];
#endif
    }
    
    const void flurryConfigActivate(){
#if __has_include("FConfig.h")
        [[FConfig sharedInstance] activateConfig];
#endif
    }
    
    const char* flurryConfigGetString(const char* key, const char* defaultValue){
        NSString *str = nil;
#if __has_include("FConfig.h")
         str = [[FConfig sharedInstance] getStringForKey: strToNSStr(key)
                                             withDefault: strToNSStr(defaultValue)];
#endif
        return strDup([str UTF8String]);
        
    }
    
    const void flurryRegisterMessagingCallback(OnNotificationReceived handler1, OnNotificationClicked handler2){
#if __has_include("FlurryMessaging.h")
        onNotificationReceivedBlock = handler1;
        onNotificationClickedBlock = handler2;
#endif
        
    }
    
}

@end


