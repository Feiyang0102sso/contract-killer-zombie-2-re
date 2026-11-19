#include "ServerTimeInterface.h"
#include <mach/mach_time.h>

unsigned long long ServerTimeInterface::getTicks()
{
	static unsigned int ticksDivisor = 0;
	if (ticksDivisor == 0)
	{
		mach_timebase_info_data_t info;
		if (mach_timebase_info(&info) == KERN_SUCCESS)
		{
			ticksDivisor = (unsigned int)((double)info.denom * 1000000.0 / (double)info.numer);
		}
	}
	return mach_absolute_time() / ticksDivisor;
}
