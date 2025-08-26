# DBBHelper

**Version 1.1.0**

**Update:**
    Add: Controller for two Blur Effects and seven Glitch Effects.
    Add: Tint and Saturation.
    Adjust: HDR, TintColor and Saturation are combined into ColorCorrection.

## Warning

Effects may require GPU performance support. If you're experiencing noticeable input lag, one possible solution is to disable vertical sync. Additionally, if you still feel stuttering, you can go to the game settings and disable the display of FogEffect, GodLight2D, and HDPostprocessing to avoid this issue. Of course, this means you might not get the full gaming experience as intended by the map designers.

Bugs can pop up anywhere, so if you encounter any, please let me know. Additionally, currently using sl (save/load) will cause these effect entities to stop working (F8 for instant fog removal, kind of a legit cheat command? haha). I'm currently trying to resolve these issues.

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
- SL Bug







