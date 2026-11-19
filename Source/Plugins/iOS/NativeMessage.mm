#include "NativeMessage.h"
#import "NativeMessageInstance.h"

void NativeMessage::showAlertWithTitleMessageAndButton(const char* title, const char* message, const char* buttonTitle)
{
	NSString* titleString = [NSString stringWithCString:title encoding:NSASCIIStringEncoding];
	NSString* messageString = [NSString stringWithCString:message encoding:NSASCIIStringEncoding];
	NSString* buttonString = [NSString stringWithCString:buttonTitle encoding:NSASCIIStringEncoding];
	[[NativeMessageInstance nativeMessageInstance] showAlert:titleString :messageString :buttonString];
}
