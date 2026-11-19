#import "NativeMessageInstance.h"

extern "C" void UnitySendMessage(const char* obj, const char* method, const char* msg);

static NativeMessageInstance* sharedInstance_ = nil;

@implementation NativeMessageInstance

+ (NativeMessageInstance*)nativeMessageInstance
{
	if (sharedInstance_ == nil)
	{
		sharedInstance_ = [super alloc];
	}
	return sharedInstance_;
}

- (void)alertView:(UIAlertView*)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
	NSString* index = [NSString stringWithFormat:@"%d", buttonIndex];
	UnitySendMessage("NativeMessageGameObject", "ButtonTapped", [index UTF8String]);
}

- (void)showAlert:(NSString*)title :(NSString*)message :(NSString*)buttonTitle
{
	UIAlertView* alert = [[UIAlertView alloc] initWithTitle:title message:message delegate:self cancelButtonTitle:buttonTitle otherButtonTitles:nil];
	[alert show];
	[alert release];
}

- (void)dealloc
{
	[sharedInstance_ release];
	[super dealloc];
}

@end
