#include "NativeMessage.h"

extern "C" void NM_showAlertWithTitleMessageAndButton(const char* title, const char* message, const char* buttonTitle)
{
	NativeMessage::showAlertWithTitleMessageAndButton(title, message, buttonTitle);
}
