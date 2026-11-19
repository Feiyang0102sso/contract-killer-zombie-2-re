#import <Foundation/Foundation.h>

int Glu_Localization_GetLocale(int size, char* buf)
{
	if (size < 0 || (size > 0 && buf == NULL))
	{
		return -1;
	}
	NSString* language = [[[NSUserDefaults standardUserDefaults] objectForKey:@"AppleLanguages"] objectAtIndex:0];
	NSDictionary* components = [NSLocale componentsFromLocaleIdentifier:language];
	NSString* code = [components objectForKey:NSLocaleLanguageCode];
	if ([code isEqualToString:@"zh"])
	{
		code = language;
	}
	if (code == nil)
	{
		return -1;
	}
	int length = [code lengthOfBytesUsingEncoding:NSUTF8StringEncoding];
	if (buf == NULL)
	{
		return length + 1;
	}
	if (length + 1 > size)
	{
		return -2;
	}
	if (![code getCString:buf maxLength:size encoding:NSUTF8StringEncoding])
	{
		return -3;
	}
	return length;
}
