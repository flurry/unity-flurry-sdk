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

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface FlurrySKAdNetwork : NSObject

typedef NS_ENUM(NSUInteger, FlurryConversionValueEventType) {
NoEvent = 0, //0000
Registration = 1, //0001
Login, //0010
Subscription, //0100
InAppPurchase // 1000

};

/*!
 *@brief Call this api to allow Flurry to set your conversion value.
 *  The final conversion value is a decimal number between 0-63.
 *       The conversion value is calculated from a 6 bit binary number.
 *       The first two bits represent days of user retention from 0-3 days
 *       The last four bits represent a true false state indicating if the user has completed the post install event.
 *   @since 11.0.0
 *
 *
 * @param event   Event name using the FlurryConversionValueEventType defined above.
 *
 */

+ (void) flurryUpdateConversionValueWithEvent: (FlurryConversionValueEventType) event API_AVAILABLE(ios(14.0));

/*!
 *@brief Call this api to send your conversion value to Flurry.  You must calculate the conversion value yourself.
 *
 *   @since 11.0.0
 *
 *
 * @param conversionValue The conversion value is a decimal number between 0-63.
 */

+ (void) flurryUpdateConversionValue: (NSInteger) conversionValue API_AVAILABLE(ios(14.0));
@end

NS_ASSUME_NONNULL_END
