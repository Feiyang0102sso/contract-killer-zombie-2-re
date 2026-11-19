// Facebook stub (tuned build only). Entry points map one-to-one to the DllImports in Assets/Packages/Facebook/Scripts/Facebook.cs.
// Login never succeeds; callbacks never fire.

typedef void (*LoginCallback)(void);
typedef void (*UserInfoCallback)(void);

void FacebookInitialize(LoginCallback callback, UserInfoCallback callback2)
{
}

int FacebookUserLoggedIn(void)
{
	return 0;
}

void FacebookLogin(const char* permissions)
{
}

void FacebookLogout(void)
{
}

void FacebookRequestUserInfo(void)
{
}

void FacebookPostMessageToFeed(const char* postData)
{
}

void FacebookPostLinkToFeed(const char* link, const char* picture, const char* name, const char* caption, const char* description)
{
}

void FacebookAppRequestToFriends(const char* requestText, const char* notificationText)
{
}

void FacebookPublishScore(int score)
{
}
