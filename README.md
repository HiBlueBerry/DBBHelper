# DBBHelper

**Version 1.1.0**

**Update:**
    BugFix: Fixed the issue where the level could not be entered after disabling special effects when opening DBBHelper separately.
    BugFix: Adjusted the contrast algorithm to better handle non-fully opaque rendering.
    Add: Controller for two GodLight2D and PointLight.
    Add: (Animated)Texture Light, a new special light effect that allows you to use a texture as light directly. 
    Add: PhysicalParticle, a styleground that uses simple physics-based particle simulation.


## Warning
Bugs about CloudZipper still exist, but I'm working on it. The main purpose of this version is to fix several serious rendering-related bugs...

## Features

### 1. CloudZipper
This is a combination of cloud and zipper.

### 2. InvisibleLight (or Advanced)
Sometimes you just need a simple light source without any visible entities, and this entity may help you.

### 3. DarkIn
If you stay away from the light source for too long, Madeline will die. As you approach the light source, Madeline will gradually restore light radius.

### 4. ConditionalLightningBreakerBox (ConditionalLightning)
Lightning (breaker box) with a flag that enables you to control when this entity should exist.

### 5. Wave and distort
I just took out the wave and distortion effect separately, and you can use a mask image to create various unique visual effects.

### 6. LaserGenerator (Translate)
Deadly laser with a flag to control its movement.

### 7. ScreenBlackEdge (Horizontal and Vertical)
Used to add borders to the upper (or left) and lower (or right) sides of the screen to achieve different picture proportions.

### 8. Rope
A simple physically simulated rope.

### 9. Aligned Text
CoreMessage that allows left (or right) aligned text.

### 10. FogEffect (and its controller)
Powerful entity to make and control FBM fog.

### 11. GodLight2D
Powerful entity to make Tyndall Light Effect.

### 12. Glitch\Blur Effect (and its controller)
HD screen post-processing effects. The HD screen post-processing effects are divided into a blur layer and a distortion layer. Effects from entities in different layers will stack with each other. Effects from entities in the same layer will overwrite each other. The processing order is distortion first, then blur.

### 13. Fresnel Point Light 
A funny point light with Fresnel effect.

### 14. ColorCorrection
Convenient tool to adjust tint color, saturation, exposure, gamma and contrast for gameplay render.

## TODO
- Bug







