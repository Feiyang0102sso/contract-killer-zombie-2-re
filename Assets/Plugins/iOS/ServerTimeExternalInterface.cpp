#include "ServerTimeInterface.h"

extern "C" long long ServerTime_getTicks()
{
	return ServerTimeInterface::getTicks();
}
