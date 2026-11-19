#import <Foundation/Foundation.h>
#import <Security/Security.h>
#include <stdbool.h>

@interface KeychainWrapper : NSObject

+ (KeychainWrapper*)sharedInstance;
- (NSMutableDictionary*)newDictionaryForIdentifier:(NSString*)identifier;
- (NSData*)secureDataForIdentifier:(NSString*)identifier;
- (BOOL)setSecureData:(NSData*)data forIdentifier:(NSString*)identifier;

@end

@implementation KeychainWrapper

+ (KeychainWrapper*)sharedInstance
{
	static KeychainWrapper* sharedInstance = nil;
	static dispatch_once_t onceToken;
	dispatch_once(&onceToken, ^{
		sharedInstance = [[KeychainWrapper alloc] init];
	});
	return sharedInstance;
}

- (NSMutableDictionary*)newDictionaryForIdentifier:(NSString*)identifier
{
	NSMutableDictionary* searchDictionary = [[NSMutableDictionary alloc] initWithCapacity:4];
	NSData* encodedIdentifier = [identifier dataUsingEncoding:NSUTF8StringEncoding];
	[searchDictionary setObject:(id)kSecClassGenericPassword forKey:(id)kSecClass];
	[searchDictionary setObject:encodedIdentifier forKey:(id)kSecAttrGeneric];
	[searchDictionary setObject:encodedIdentifier forKey:(id)kSecAttrAccount];
	[searchDictionary setObject:@"com.glu.StorageManager" forKey:(id)kSecAttrService];
	return searchDictionary;
}

- (NSData*)secureDataForIdentifier:(NSString*)identifier
{
	NSMutableDictionary* searchDictionary = [self newDictionaryForIdentifier:identifier];
	[searchDictionary setObject:(id)kSecMatchLimitOne forKey:(id)kSecMatchLimit];
	[searchDictionary setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnData];
	NSData* result = nil;
	SecItemCopyMatching((CFDictionaryRef)searchDictionary, (CFTypeRef*)&result);
	[searchDictionary release];
	return result;
}

- (BOOL)setSecureData:(NSData*)data forIdentifier:(NSString*)identifier
{
	NSMutableDictionary* searchDictionary = [self newDictionaryForIdentifier:identifier];
	[searchDictionary setObject:(id)kSecMatchLimitOne forKey:(id)kSecMatchLimit];
	[searchDictionary setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnAttributes];
	NSDictionary* existing = nil;
	OSStatus status = SecItemCopyMatching((CFDictionaryRef)searchDictionary, (CFTypeRef*)&existing);
	NSMutableDictionary* dictionary = [self newDictionaryForIdentifier:identifier];
	if (status == errSecSuccess)
	{
		NSMutableDictionary* updateDictionary = [[NSMutableDictionary alloc] init];
		[updateDictionary setObject:data forKey:(id)kSecValueData];
		status = SecItemUpdate((CFDictionaryRef)dictionary, (CFDictionaryRef)updateDictionary);
		[dictionary release];
		[updateDictionary release];
	}
	else
	{
		[dictionary setObject:data forKey:(id)kSecValueData];
		status = SecItemAdd((CFDictionaryRef)dictionary, NULL);
		[dictionary release];
	}
	return status == errSecSuccess;
}

@end

const char* GetSafeDataForKey(const char* key)
{
	NSString* identifier = [NSString stringWithUTF8String:key];
	NSData* data = [[KeychainWrapper sharedInstance] secureDataForIdentifier:identifier];
	if (data == nil)
	{
		return "";
	}
	return [[[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease] UTF8String];
}

bool SetSafeDataForKey(const char* key, const char* value)
{
	NSString* identifier = [NSString stringWithUTF8String:key];
	NSData* data = [[NSString stringWithUTF8String:value] dataUsingEncoding:NSUTF8StringEncoding];
	return [[KeychainWrapper sharedInstance] setSecureData:data forIdentifier:identifier];
}
