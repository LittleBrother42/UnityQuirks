SplitScreen

This sample is designed to show solutions to two major issues with split-screen games in Unity.


1. Multiple player inputs sharing the same device (keyboard)

For this to work, the player inputs need to be manually instantiated and paired with the appropriate device. In this sample, player 1 controls the red cube using the WASD keys, while player 2 controls the blue cube using the arrow keys. See PlayerController.cs for the implementation, and this forum link for further reading: https://discussions.unity.com/t/multiple-players-on-keyboard-new-input-system/754028


2. Listening to 3D sounds from multiple locations in the scene

This one is a huge pain, since you can only have one audio listener per scene; the trick we use here is to place the audio listener at (0, 0, 0) and then move the audio sources around to act as an "average" of how both players would be able to hear them. The system we use here is fairly simple and relies on linear rolloff, but it's good enough in practice and can be expanded upon if necessary.

In this example, the cameras of the two players used as the "fake audio listeners", and the sound is emanating from the yellow sphere. Move the players around and see how it affects the volume and panning of the sound! A few examples:
  - If you move one player away, the volume won't change. But if you move both away, it will get quieter.
  - If both players move to the left (i.e. the cubes move apart), you'll mainly hear the sound through the right speaker, since the yellow sphere is now to the right of both players. Similarly if you move to the right, the sound will come from the left.
  - If both players move in opposite directions (i.e. one left and one right so that the cubes are opposite each other), the panning of the sound will stay roughly central since the sound source is now to the left of one player and to the right of the other.

See MultiListenerSoundManager for the implementation; the system currently only supports playing sounds that are present in the scene from the start, but can be expanded to support audio sources that are instantiated at a later date.

The sound made by the sphere is a modified version of "16_ca_pads.wav" by lerwickdj on freesound.org: https://freesound.org/people/lerwickdj/sounds/246154/