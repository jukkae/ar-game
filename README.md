# Eclipse Realm: A survival / treasure hunt AR game built on VimAI, Unity and ARCore

Unity version used: 2018.2.10f1

## Play instructions

The instructions are shown in-game when Eclipse Realm is selected from the popup menu.

## Development

The main Unity scene is `AR View.unity`.

### Building and running: Development & debug

When developing and debugging, you need to have an ARCore-enabled phone connected. The phone itself is not needed for input nor output, it just needs to be there.

You should disable AR controls from `Hierarchy -> Controller -> AR Controller (Script) -> Disable AR control`.

You will probably also want to replace the default, invisible material for obstacle meshes with a visible one: `Hierarchy -> World -> Eclipse Realm (Script) -> Change Material to Debug`.

You might also get a popup asking about installing Instant Preview. Debug has proven to work reliably with version 1.0.10, and should probably work with any newer version, too.

When AR controls are disabled, you can move with WSAD, turn with arrow keys and interact with objects with mouse. The phone screen does not work as an input device.

### Building and running: Release

For release builds, you must enable AR controls and disable replacing obstacle mesh material: `Hierarchy -> Controller -> AR Controller (Script) -> Disable AR control` and `Hierarchy -> World -> Eclipse Realm (Script) -> Change Material to Debug`.

You only need to include `AR View.unity` in the build.

Other than that, build, run, enjoy!

### Hacking

Note: This section contains information that is bound to get out of date with further development. The information is correct as of 2018-11-02 at project handover and can be considered a brief introduction to what's happening where, so as to speed up further development efforts' ramp-up time.

The initial game selection pop-up is implemented in `WorldController.cs`. For any of the other games, the correct Game ID is actually requested and gotten from the server. For Eclipse Realm, however, the network request is never sent, and instead, the response is mocked up locally. In either case, the obstacle mesh corresponding to the building is received from the server. After that, `InitializeRealm()` function from `EclipseRealm.cs` is called, control is moved to `EclipseRealm.cs` and the game starts.

When running in debug, the player is randomly positioned on the mesh. In release, the player position is resolved by the server.

The help screen is always shown at the beginning. This happens from within `InitializeRealm()`.

When the realm has been initialized, `EclipseRealm.cs` – attached to World GameObject in the hierarchy – only handles spawning of enemies and items at appropriate intervals.

Much of the game logic is implemented in `EclipsePlayer.cs` script, attached to `ARCore Device -> Main Camera / Player` GameObject. One noteworthy feature is that while many of the UI elements – score and powerup indicators – are pre-existing elements on the `Canvas` GameObject in hierarchy, the energy and health bars are generated procedurally in `EclipsePlayer.cs`'s `OnGUI()`. Furthermore, the Game Over screen is also created from within `OnGUI()`, perhaps a bit counterintuitively. A more ideal place would be `Die()`, but due to Unity's threading model, that is not possible.

The collectibles and powerups are simply spawned based on their prefabs, which reside under `Assets/Prefabs/Eclipse Realm`. A pickable object should have a collider that is set to be a trigger collider, as well as `Eclipse Pickable` script and the halo particle system. For further information on the particulars, please see the existing prefabs.

The enemies' main controller is `SkeletonEnemyController.cs`. One would expect to find AI code there, but unfortunately Unity's animation system forces to spread AI functionalities over code, `Nav Mesh Agent` component and `Animator` component. Basically, the idea is that target position is generated in the code, Nav Mesh Agent calculates a path to that target, code moves the enemy along that path and updates the current velocity to the Animator so that the correct animation clips get blended so that animation looks good. The Animator component is basically a state machine that operates based on 1) parameters that are set via code and 2) exit times for certain animations. For example, the getting-hit animation always takes a certain amount of time (unless re-triggered), and after that the enemy returns to idle animation. The actual damage done to the player happens again in code. The logic is, unfortunately, more complicated than it (seemingly) would need to be, but there's not a lot that can be done about that. The situation is exacerbated by animations not being exactly perfect all the time – for example, the dying animation, while in itself quite beautifully animated, ends with the character model floating half a meter above the ground. Not having access to animation project files, this is 'fixed' in code. However, when it comes to interaction between the player and the enemies, everything happens via just a couple of function calls, so building a new enemy type, for example, from the ground up shouldn't be too complicated.

### Outstanding issues

- The enemies should preferably have a minimum distance, and if they are within that radius from the player, they should try to take a step back before attacking. Unfortunately, the implementation turned out to be very hard to tune reliably to avoid feedback loops leading to glitches. Hence, the current version simply has increased the attack distance to 2 meters, which does, in practice, avoid most of the issues.

- The obstacle mesh material should ideally occlude virtual objects behind it, while still being invisible in the sense that it would allow the pixels underneath the object to clear to camera feed. The implementation is not complicated, but this was dropped, because if the virtual objects don't line up exactly with the real-world objects, the artefacts caused by invisible occluders will be more distracting than just having completely invisible and transparent walls and floors.

- Sometimes an error message appears in Unity's message log: `'skeleton_animated(Clone)' AnimationEvent 'SetAgentStopped' has no receiver! Are you missing a component?` This is actually not an error, or at least, everything still does work. I was unable to find the source of this. This can just be ignored.

## Debug Menu Items

Restart App - Reload scene completely

Select Building - Select a building manually

Dummy Location - Get a random position in the world (as detected from image)

Test Image - Send random image from /Android/data/[package name]/files/test/ folder to location server (must be created manually)