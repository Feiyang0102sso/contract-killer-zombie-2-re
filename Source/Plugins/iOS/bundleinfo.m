#import <Foundation/Foundation.h>

void GetBundleVersion(char* buf, int size)
{
	[[[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleVersion"] getCString:buf maxLength:size encoding:NSASCIIStringEncoding];
}

void GetBundleId(char* buf, int size)
{
	[[[NSBundle mainBundle] bundleIdentifier] getCString:buf maxLength:size encoding:NSASCIIStringEncoding];
}
