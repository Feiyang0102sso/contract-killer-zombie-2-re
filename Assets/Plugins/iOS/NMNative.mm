#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern "C" int PushNotificationsGetBadgeNumber()
{
	return [[UIApplication sharedApplication] applicationIconBadgeNumber];
}

extern "C" void PushNotificationsSetBadgeNumber(int val)
{
	[[UIApplication sharedApplication] setApplicationIconBadgeNumber:val];
}

extern "C" void PushNotificationsGetTimezoneOlson(char* dest, int n)
{
	NSTimeZone* timeZone = [NSTimeZone localTimeZone];
	if (timeZone != nil && dest != NULL)
	{
		[[timeZone name] getCString:dest maxLength:n encoding:NSASCIIStringEncoding];
	}
}
