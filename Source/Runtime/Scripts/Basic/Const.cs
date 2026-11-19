using System.Collections.Generic;
using UnityEngine;

public class Const : MonoBehaviour
{
	public const float MinVoiceDistance = 5f;

	public const float MaxVoiceDistance = 25f;

	public const string PathWeapons = "Assets/Prefabs/Items/Weapons";

	public const string PathConsumables = "Assets/Prefabs/Items/Consumables";

	public const string CKZ2_PLAYHAVEN_TOKEN = "a63d65d508d24eccbe9706766d6ee594";

	public const string CKZ2_PLAYHAVEN_SECRET = "aa8d8a9604454629a3a3c74183781e30";

	public const string CKZ2_TAPJOY_APP_ID = "c631fb4d-a728-4f34-ac42-42f5cb6b9e80";

	public const string CKZ2_TAPJOY_APP_SECRET_KEY = "MCw1DV6xmrO81ah15AAl";

	public const string CKZ2_FLURRY_API_KEY = "ZA1BTLN37AZJ25USG9YC";

	public const string CKZ2_ASSET_BUNDLES_URL = "http://glusvn.s3.amazonaws.com/ckzombies2-live";

	public const string CKZ2_KONTAGENT_KEY = "7a6da10d46fd4f469fa488bbf1c51a06";

	public const string CKZ2_PUSHNOTIFICATIONS_KEY = "KqYZlpA3QnGPhuAZh-gMqg";

	public const string CKZ2_PUSHNOTIFICATIONS_SECRET = "2WpCQ-X1R_y_ozFwXIxVXg";

	public const string CKZ2_PUSHNOTIFICATIONS_MASTER = "";

	public const string CKZ2_RATE_ME_URL = "http://gcs.glu.com/gcs/iphone/iphone.jsp?t=ckzombies2&src=ckzombies2&p=iphone&page=review";

	public const string CKZ1_URL = "http://gcs.glu.com/gcs/iphone/iphone.jsp?t=zombsniper&src=zombsniper&p=iphone";

	public const string CKZ2_BURSTLY_IPHONE_PUB = "QRCoGFfRqUCKHHpIEiTe9A";

	public const string CKZ2_BURSTLY_IPHONE_BANNER_ZONE = "0357914379087244521";

	public const string CKZ2_FACEBOOK_APP_SECRET = "31a6b3ae7fcfe8cdc69e849c2e039eb8";

	public const string CKZ2_FACEBOOK_APP_LINK = "http://www.glu.com/game/contract-killer-zombies-2";

	public const string CKZ2_FACEBOOK_APP_ICON_LINK = "https://s3.amazonaws.com/glu-facebook/CKZ2_1.1_512icon.png";

	public const string CKZ2_FACEBOOK_CHAT_LINK = "http://www.facebook.com/ContractKillerZombies?fref=ts";

	public const string CKZ2_ADMARVEL_SITEID_IPHONE = "18268";

	public const string CKZ2_ADMARVEL_SITEID_IPAD = "22642";

	public const string CKZ2_ADMARVEL_PARTNERID = "a1434e57627654cf";

	public const string CKZ2_MAT_ADVERTISER_ID = "2376";

	public const string CKZ2_MAT_APP_KEY = "533273abed4546b082352289881f70d4";

	public const string CKZ2_LEADERBOARD_ZOMBIES_KILLED = "CKZOMBIES2_BOARD_ZOMBIES_KILLED";

	public const string CKZ2_LEADERBOARD_SURVIVAL_MODE = "CKZOMBIES2_BOARD_SURVIVAL_SCORE";

	public const string CKZ2_LEADERBOARD_ARENA_MODE = "CKZOMBIES2_BOARD_ARENA_SCORE";

	public const string ANDROID_MARKET_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAiDe5FBbiqA55lvMO+FnsGzdtZpLsbhEMg2IiAIow4RUSDBHLnBf6m2BbxnusxZS47dpG0hq3Daqw1WkMxHj6UTjOQc9V+GYwvAX/h55nT4gFpMTzzjx+yECekyXUVZ/dEDl2Nfil3rTRlwiURbpA1CIObiuFQmP2vH8pRkXnvUaj60EGtrqZydGzsuR8QVX8fa7eOzopBNsUqU5kqrAsggi52bws08OBfYZ2ehlIrBxts5UipYQVwFuFKtmc53XI1jb4sQOs5UdT9wvJ87Cg+C8vI85Z1sDjBCn8XSkJsN3Y3TAdv4H2gA92pUza3K+kdIHUa24h+zQxwWe8gdQiNwIDAQAB";

	public const string CKZ2_PRIVACY_TERMS_URL = "http://www.glu.com/legal";

	public static int TargetFPS = 30;

	public static int FullEffectsFPS = 25;

	public static int LiteEffectsFPS = 20;

	public static int MinEffectsFPS = 15;

	public static int MaxRagdollsSpawnedPerFrame = 2;

	public static int MaxRagdollsOnScene = 5;

	public static string SaveFileName = "ckz2.savegame";

	public static string DataFileName = "ckz2.gamedata";

	public static string SettingsFileName = "ckz2.settings";

	public static int LayerDefault;

	public static int LayerAntiHitable;

	public static int LayerTransparentFX;

	public static int LayerHitable;

	public static int LayerSubHitable;

	public static int LayerRagdoll;

	public static int LayerDecal;

	public static int LayerGui;

	public static int LayerGui3d;

	public static int LayerDynamicObstacle;

	public static int LayerEyeCandy;

	public static int LayerMaskObstacles;

	public static int LayerMaskAllHitable;

	public static int LayerMaskHitByProjectile;

	public static List<string> supportedLocales = new List<string>
	{
		"en", "fr", "de", "it", "pt", "ru", "es", "ja", "ko", "zh-Hans",
		"zh-Hant"
	};

	private void Awake()
	{
		LayerDefault = LayerMask.NameToLayer("Default");
		LayerAntiHitable = LayerMask.NameToLayer("AntiHitable");
		LayerTransparentFX = LayerMask.NameToLayer("TransparentFX");
		LayerHitable = LayerMask.NameToLayer("Hitable");
		LayerSubHitable = LayerMask.NameToLayer("SubHitable");
		LayerRagdoll = LayerMask.NameToLayer("Ragdoll");
		LayerDecal = LayerMask.NameToLayer("Decal");
		LayerGui = LayerMask.NameToLayer("Gui");
		LayerGui3d = LayerMask.NameToLayer("Gui3d");
		LayerDynamicObstacle = LayerMask.NameToLayer("DynamicObstacle");
		LayerEyeCandy = LayerMask.NameToLayer("EyeCandy");
		LayerMaskObstacles = (1 << LayerDefault) | (1 << LayerDynamicObstacle) | (1 << LayerAntiHitable);
		LayerMaskAllHitable = (1 << LayerHitable) | (1 << LayerSubHitable);
		LayerMaskHitByProjectile = LayerMaskObstacles | LayerMaskAllHitable | (1 << LayerRagdoll);
	}
}
