#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface NativeMessageInstance : NSObject

+ (NativeMessageInstance*)nativeMessageInstance;
- (void)showAlert:(NSString*)title :(NSString*)message :(NSString*)buttonTitle;

@end
