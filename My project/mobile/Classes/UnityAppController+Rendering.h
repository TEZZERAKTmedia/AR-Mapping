#pragma once

#include "UnityForwardDecls.h"
#include "UnityAppController.h"
#include "UnityRendering.h"
#if PLATFORM_VISIONOS
#include "UnityAppController+Rendering+visionOS.h"
#endif

@interface UnityAppController (Rendering)

#if !PLATFORM_VISIONOS
@property (readonly) BOOL usingCompositorLayer;
#endif

- (void)createDisplayLink;
- (void)repaintDisplayLink;
- (void)destroyDisplayLink;

- (void)repaint;

#if !PLATFORM_VISIONOS
- (void)repaintCompositorLayer;
#endif

- (void)selectRenderingAPI;
@property (readonly, nonatomic) UnityRenderingAPI   renderingAPI;

@end

// helper to run unity loop along with proper handling of the rendering
#ifdef __cplusplus
extern "C" {
#endif

void UnityRepaint();

#ifdef __cplusplus
} // extern "C"
#endif
