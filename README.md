# DBBHelper
DBBHelper 
    Version 1.0.3

    Update:
        Add a new Entity: AlignedText
        Add a new Entity: ScreenBlackEdgeVertical
        Adjust: ScreenBlackEdge was renamed ScreenBlackEdgeHorizontal
    Features:

        1.CloudZipper
        This is a combination of cloud and zipper.

        2.InvisibleLight(or Advance)
        Sometimes you just need a simple light source without any visible entities, and this entity may help you.

        3.DarkIn
        If you stay away from the light source for too long, Madeline will die. As you approach the light source, Madeline will gradually restore light radius.

        4.ConditionalLightningBreakerBox(ConditionalLightning)
        Lightning (breaker box) with a flag that enables you to control when this entity should exist.

        5.Wave and distort
        I just took out the wave and distortion effect separately, and you can use a mask image to create various unique visual effects.

        6.LaserGenerator(Translate)
        Deadly laser with a flag to control its movement.

        7.ScreenBlackEdge(Horizontal and Vertical)
        Used to add borders to the upper (or left) and lower (or right) sides of the screen to achieve different picture proportions.

        8.Rope
        A simple physically simulated rope.

        9.Aligned Text
        CoreMessage that allows left (or right) aligned text, but please be careful when turning on the mirror mode.


    TODO:

        LaserGenerator(Rotate):Deadly laser with a flag to control its rotation.
        (A control sequence using lua may be the best choice, but you know I'm a lazy guy.Ok,if HollowKnight SilkSong is released, I will start working on this matter.)

        FanBumper:Ok, now I'm considering making this into a HD texture version.

        Fluid:Actually I hate everything about physics, especially when some strange bugs about fluid simulation have cracked your mind for more than 3 days.

        FBMFog:Actually I have prepared the core content, but now I am making a UI that adapts to post-processing effects.
        (Tell you a secret,various kinds of BlurEffect and GlitchEffect have also been prepared,and now all I need is just an appropriate UI.:)
