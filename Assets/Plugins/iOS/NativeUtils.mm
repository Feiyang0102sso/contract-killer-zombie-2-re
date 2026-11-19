#import <Foundation/Foundation.h>
#import <SystemConfiguration/SystemConfiguration.h>
#include <ifaddrs.h>
#include <arpa/inet.h>
#include <net/if.h>
#include <netinet/in.h>
#include <mach/mach.h>

NSString* _getWiFiIPAddress()
{
	BOOL success;
	struct ifaddrs* addrs;
	const struct ifaddrs* cursor;

	success = getifaddrs(&addrs) == 0;
	if (success)
	{
		cursor = addrs;
		while (cursor != NULL)
		{
			if (cursor->ifa_addr->sa_family == AF_INET && (cursor->ifa_flags & IFF_LOOPBACK) == 0)
			{
				NSString* name = [NSString stringWithUTF8String:cursor->ifa_name];
				if ([name isEqualToString:@"en0"] || [name isEqualToString:@"en1"])
				{
					return [NSString stringWithUTF8String:inet_ntoa(((struct sockaddr_in*)cursor->ifa_addr)->sin_addr)];
				}
			}
			cursor = cursor->ifa_next;
		}
		freeifaddrs(addrs);
	}
	return NULL;
}

extern "C" int NetworkStatus()
{
	struct sockaddr_in zeroAddress;
	bzero(&zeroAddress, sizeof(zeroAddress));
	zeroAddress.sin_len = sizeof(zeroAddress);
	zeroAddress.sin_family = AF_INET;
	SCNetworkReachabilityRef reachability = SCNetworkReachabilityCreateWithAddress(NULL, (const struct sockaddr*)&zeroAddress);
	SCNetworkReachabilityFlags flags;
	SCNetworkReachabilityGetFlags(reachability, &flags);
	bool isReachable = (flags & kSCNetworkReachabilityFlagsReachable) != 0;
	bool needsConnection = (flags & kSCNetworkReachabilityFlagsConnectionRequired) != 0;
	bool isWWAN = (flags & kSCNetworkReachabilityFlagsIsWWAN) != 0;
	if (isReachable && !needsConnection)
	{
		NSString* address = _getWiFiIPAddress();
		if (isWWAN)
		{
			return 1;
		}
		if (address != nil)
		{
			return 2;
		}
	}
	return 0;
}

extern "C" int MT_GetCurrentMemoryBytes()
{
	struct task_basic_info info;
	mach_msg_type_number_t size = sizeof(info);
	kern_return_t kerr = task_info(mach_task_self(), TASK_BASIC_INFO, (task_info_t)&info, &size);
	if (kerr == KERN_SUCCESS)
	{
		return info.resident_size;
	}
	return -1;
}
