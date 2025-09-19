//
//  mouse.m
//  mouse
//
//  Created by pc on 2021/5/28.
//  Copyright © 2021 pc. All rights reserved.
//
#import "mouse.h"

#import <Foundation/Foundation.h>
#import "AppKit/AppKit.h"
@implementation mouse : NSObject

@end



 CGPoint GetPoint(){
  CGPoint p = CGPointMake([NSEvent mouseLocation].x, [NSScreen mainScreen].frame.size.height - [NSEvent mouseLocation].y);
    return p;
    
}

__attribute__((constructor)) void Move2Left(){
    CGEventSourceRef source = CGEventSourceCreate(kCGEventSourceStateCombinedSessionState);
    CGPoint p =  GetPoint();
    CGEventRef mouse = CGEventCreateMouseEvent (NULL, kCGEventMouseMoved, CGPointMake( p.x - 1, p.y), 0);
    CGEventPost(kCGHIDEventTap, mouse);
    CFRelease(mouse);
    CFRelease(source);
}

__attribute__((constructor)) void Move2Top(){
    CGEventSourceRef source = CGEventSourceCreate(kCGEventSourceStateCombinedSessionState);
    CGPoint p =  GetPoint();
    CGEventRef mouse = CGEventCreateMouseEvent (NULL, kCGEventMouseMoved, CGPointMake( p.x, p.y - 1), 0);
    CGEventPost(kCGHIDEventTap, mouse);
    CFRelease(mouse);
    CFRelease(source);
}

__attribute__((constructor)) void Move2Right(){
    CGEventSourceRef source = CGEventSourceCreate(kCGEventSourceStateCombinedSessionState);
    CGPoint p =  GetPoint();
    CGEventRef mouse = CGEventCreateMouseEvent (NULL, kCGEventMouseMoved, CGPointMake( p.x + 1, p.y), 0);
    CGEventPost(kCGHIDEventTap, mouse);
    CFRelease(mouse);
    CFRelease(source);
}

__attribute__((constructor)) void Move2Bottom(){
    CGEventSourceRef source = CGEventSourceCreate(kCGEventSourceStateCombinedSessionState);
    CGPoint p =  GetPoint();
    CGEventRef mouse = CGEventCreateMouseEvent (NULL, kCGEventMouseMoved, CGPointMake( p.x, p.y + 1), 0);
    CGEventPost(kCGHIDEventTap, mouse);
    CFRelease(mouse);
    CFRelease(source);
}
