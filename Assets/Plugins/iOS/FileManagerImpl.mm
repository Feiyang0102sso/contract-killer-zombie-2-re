#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

typedef void (*SaveFileCallback)(const char* path, bool success);
typedef void (*LoadFileCallback)(const char* path, const void* data, unsigned int length);
typedef void (*FindFilePathInCloudCallback)(const char* fileName, const char* cloudPath);

NSString* NUCreateNSString(const char* string)
{
	if (string)
	{
		return [NSString stringWithUTF8String:string];
	}
	return [NSString stringWithUTF8String:""];
}

NSString* GetRelativePathFromHomeDirectory(NSString* path)
{
	NSArray* pathComponents = [path pathComponents];
	NSArray* homeComponents = [NSHomeDirectory() pathComponents];
	NSArray* relativeComponents = [pathComponents subarrayWithRange:NSMakeRange([homeComponents count], [pathComponents count] - [homeComponents count])];
	return [NSString pathWithComponents:relativeComponents];
}

@interface FileDocument : UIDocument
{
	NSData* data;
	NSString* path;
	SaveFileCallback saveCallback;
	LoadFileCallback loadCallback;
}

@property (nonatomic, retain) NSData* data;
@property (nonatomic, retain) NSString* path;
@property (nonatomic, assign) SaveFileCallback saveCallback;
@property (nonatomic, assign) LoadFileCallback loadCallback;

@end

@implementation FileDocument

@synthesize data;
@synthesize path;
@synthesize saveCallback;
@synthesize loadCallback;

- (void)dealloc
{
	self.data = nil;
	self.path = nil;
}

- (BOOL)loadFromContents:(id)contents ofType:(NSString*)typeName error:(NSError**)outError
{
	if ([contents length] > 0)
	{
		self.data = [NSData dataWithData:contents];
	}
	else
	{
		self.data = [NSData data];
	}
	return YES;
}

- (id)contentsForType:(NSString*)typeName error:(NSError**)outError
{
	return self.data;
}

@end

@interface FindFileInCloudQuery : NSObject
{
	NSString* fileName;
	NSMetadataQuery* query;
	FindFilePathInCloudCallback onCompleteCallback;
}

@property (nonatomic, retain) NSString* fileName;
@property (nonatomic, retain) NSMetadataQuery* query;
@property (nonatomic, assign) FindFilePathInCloudCallback onCompleteCallback;

@end

@implementation FindFileInCloudQuery

@synthesize fileName;
@synthesize query;
@synthesize onCompleteCallback;

- (void)dealloc
{
	self.fileName = nil;
	if (self.query != nil)
	{
		[self.query disableUpdates];
		[self.query stopQuery];
		[[NSNotificationCenter defaultCenter] removeObserver:self name:NSMetadataQueryDidFinishGatheringNotification object:self.query];
		self.query = nil;
	}
}

- (void)queryDidFinishGathering:(NSNotification*)notification
{
	NSMetadataQuery* finishedQuery = [notification object];
	[finishedQuery disableUpdates];
	[finishedQuery stopQuery];
	[[NSNotificationCenter defaultCenter] removeObserver:self name:NSMetadataQueryDidFinishGatheringNotification object:self.query];
	if (self.onCompleteCallback != NULL)
	{
		const char* name = [self.fileName UTF8String];
		if ([finishedQuery resultCount] == 1)
		{
			NSMetadataItem* item = [finishedQuery resultAtIndex:0];
			const char* cloudPath = [[[item valueForAttribute:NSMetadataItemURLKey] path] UTF8String];
			self.onCompleteCallback(name, cloudPath);
		}
		else
		{
			self.onCompleteCallback(name, NULL);
		}
	}
	self.query = nil;
	[self release];
}

@end

extern "C"
{

bool _GetCloudContainerDirectoryPath(const char* containerID, char* buffer, unsigned int bufferLength)
{
	NSFileManager* fileManager = [NSFileManager defaultManager];
	NSURL* containerURL;
	if (containerID != NULL)
	{
		containerURL = [fileManager URLForUbiquityContainerIdentifier:NUCreateNSString(containerID)];
	}
	else
	{
		containerURL = [fileManager URLForUbiquityContainerIdentifier:nil];
	}
	if (containerURL == nil)
	{
		return false;
	}
	return [[containerURL path] getCString:buffer maxLength:bufferLength encoding:NSUTF8StringEncoding];
}

bool _ConvertLocalPathToCloudPath(const char* containerID, const char* localPath, char* buffer, unsigned int bufferLength)
{
	bool result = false;
	NSString* identifier = nil;
	if (containerID != NULL)
	{
		identifier = NUCreateNSString(containerID);
	}
	NSURL* containerURL = [[NSFileManager defaultManager] URLForUbiquityContainerIdentifier:identifier];
	if (containerURL != nil)
	{
		NSString* relativePath = GetRelativePathFromHomeDirectory(NUCreateNSString(localPath));
		[[[containerURL URLByAppendingPathComponent:relativePath] path] getCString:buffer maxLength:bufferLength encoding:NSUTF8StringEncoding];
		result = true;
	}
	return result;
}

void _FindFilePathInCloud(const char* containerID, const char* fileName, FindFilePathInCloudCallback callback)
{
	FindFileInCloudQuery* findQuery = [[FindFileInCloudQuery alloc] init];
	NSString* name = NUCreateNSString(fileName);
	NSMetadataQuery* query = [[NSMetadataQuery alloc] init];
	findQuery.query = query;
	findQuery.fileName = name;
	findQuery.onCompleteCallback = callback;
	[query setSearchScopes:[NSArray arrayWithObject:NSMetadataQueryUbiquitousDocumentsScope]];
	[query setPredicate:[NSPredicate predicateWithFormat:@"%K == %@", NSMetadataItemFSNameKey, name]];
	[[NSNotificationCenter defaultCenter] addObserver:findQuery selector:@selector(queryDidFinishGathering:) name:NSMetadataQueryDidFinishGatheringNotification object:query];
	[query startQuery];
	[query release];
}

static void CloseFileDocument(FileDocument* document)
{
	[document closeWithCompletionHandler:^(BOOL success) {
		[document release];
	}];
}

void _SaveDataToFile(const char* path, const void* data, unsigned int length, SaveFileCallback callback)
{
	NSString* filePath = NUCreateNSString(path);
	NSURL* url = [NSURL fileURLWithPath:filePath];
	__block FileDocument* document = [[FileDocument alloc] initWithFileURL:url];
	document.data = [NSData dataWithBytes:data length:length];
	document.path = filePath;
	document.saveCallback = callback;
	[document saveToURL:url forSaveOperation:UIDocumentSaveForOverwriting completionHandler:^(BOOL success) {
		if (success)
		{
			dispatch_async(dispatch_get_main_queue(), ^{
				if (document.saveCallback != NULL)
				{
					document.saveCallback([document.path UTF8String], success);
				}
				CloseFileDocument(document);
			});
		}
		else
		{
			NSLog(@"SaveDataToFile: FAILURE = %@", url);
		}
	}];
}

void _LoadDataFromFile(const char* path, LoadFileCallback callback)
{
	NSString* filePath = NUCreateNSString(path);
	NSURL* url = [NSURL fileURLWithPath:filePath];
	__block FileDocument* document = [[FileDocument alloc] initWithFileURL:url];
	document.path = filePath;
	document.loadCallback = callback;
	[document openWithCompletionHandler:^(BOOL success) {
		if (success)
		{
			dispatch_async(dispatch_get_main_queue(), ^{
				if (document.loadCallback != NULL)
				{
					document.loadCallback([document.path UTF8String], [document.data bytes], [document.data length]);
				}
				CloseFileDocument(document);
			});
		}
		else
		{
			NSLog(@"LoadDataFromFile: FAILURE = %@", url);
		}
	}];
}

}
