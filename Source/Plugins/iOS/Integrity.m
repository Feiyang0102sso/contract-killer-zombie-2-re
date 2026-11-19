#import <Foundation/Foundation.h>
#include <dlfcn.h>
#include <stdlib.h>
#include <mach-o/loader.h>

int main(int argc, char* argv[]);

BOOL IsJailbroken()
{
	return system(NULL) != 0;
}

BOOL IsEncrypted()
{
	Dl_info dlinfo;
	if (dladdr(main, &dlinfo) == 0 || dlinfo.dli_fbase == NULL)
	{
		return NO;
	}
	const struct mach_header* header = (const struct mach_header*)dlinfo.dli_fbase;
	struct load_command* cmd = (struct load_command*)(header + 1);
	for (uint32_t i = 0; cmd != NULL && i < header->ncmds; i++)
	{
		if (cmd->cmd == LC_ENCRYPTION_INFO)
		{
			struct encryption_info_command* crypt_cmd = (struct encryption_info_command*)cmd;
			if (crypt_cmd->cryptid < 1)
			{
				return NO;
			}
			return YES;
		}
		cmd = (struct load_command*)((uint8_t*)cmd + cmd->cmdsize);
	}
	return NO;
}
