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

@interface FlurryCCPA : NSObject

/*!
*@brief An api to send ccpa compliance data to Flurry on the user's choice to opt out or opt in to data sale to third parties.
*   @since 10.1.0
*
*
* @param isOptOut   boolean true if the user wants to opt out of data sale, the default value is false
*/
+(void) setDataSaleOptOut: (BOOL) isOptOut;

/*!
*@brief An api to allow the user to request Flurry delete their collected data from this app.
*   @since 10.1.0
*
*/
+(void) setDelete;

@end
