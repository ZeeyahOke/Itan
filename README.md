# Ìtàn: Moonlight Tales

A seated VR storytelling experience for Meta Quest 2. You sit by a fire with an elder
who tells the Yoruba folktale of Ajá the dog and Ìjàpá the tortoise. You bow to greet
him, answer his call out loud, watch the story happen around you, climb the rope into
the sky yourself, and pick up three Yoruba words on the way: **ajá** (dog), **ìyá**
(mother) and **ìjàpá** (tortoise). Back at the fire he checks what you remember with a
short card quiz.

This repository is the vertical slice.

## A visit, step by step

| Step | What the visitor does | Script |
|---|---|---|
| Menu | Press **Begin** on the title card | `States/MenuState.cs` |
| Greeting | Bow your head to the elder, then take the calabash he offers into your lap | `States/GreetingState.cs`, `Logic/BowCheck.cs`, `Interaction/Calabash.cs` |
| Call | He calls "Àlọ́ o!" and you answer "Ààlọ̀!" out loud (a button appears after two quiet tries) | `States/CallState.cs`, `Logic/VoiceCheck.cs` |
| Story | Smoke carries you into the tale: the famine, the rope from the sky, Ìjàpá's fall. A word card drops into your calabash at the end of each part | `States/StoryState.cs`, `Story/StoryManager.cs`, `World/*Moment.cs` |
| Climb | Grip the rope with either controller and pull yourself up to Ajá's mother's home in the clouds | `World/PlayerClimbMoment.cs` |
| Quiz | Three rounds: pick the card that matches the word | `States/QuizState.cs`, `Logic/QuizLogic.cs` |
| Summary | The elder shares the lesson; you see your words and score. **Finish** resets for the next visitor | `States/SummaryState.cs` |

The narrator speaks English and the subtitles keep the Yoruba words.

## Controls (Quest 2 Touch controllers, seated)

- **Trigger**: press buttons and pick quiz cards
- **Grip**: hold the calabash, grab the rope while climbing
- **Head**: bow to greet the elder
- **Voice**: answer the elder's call (the microphone permission is asked on first launch)
- Hold the **Meta button** to recenter if you shift in your seat

## Getting it running

1. Install **Unity 6000.3.27f1** with **Android Build Support** (OpenJDK and SDK/NDK).
2. Clone the repo and open the folder in Unity Hub.
3. Most of the art is not stored in git. In Unity, run **Itan > Assets > Download Third-Party Assets**.
   It fetches the CC0 and CC BY packs listed in `Assets/_Project/ThirdPartyAssets.txt` into
   `Assets/_ThirdParty`. Their `.meta` files are already in the repo, so every reference stays linked.
4. **The elder is a Mixamo character.** Mixamo's terms don't allow sharing the files, so
   download **David** (T-pose, FBX for Unity) from [mixamo.com](https://www.mixamo.com) with
   your own Adobe account and put it in `Assets/_ThirdParty/Mixamo/Characters/`. Then run
   **Itan > Characters > Reimport Mixamo** and **Itan > Hub > Build Elder**.
5. Run **Itan > Setup > Apply Quest Build Settings** and **Itan > Setup > Apply Quest Performance Settings**.
6. Open `Assets/_Project/Scenes/Hub.unity` and press Play. The story world
   (`StoryWorld.unity`) loads on its own in the background.

You can play in the editor without a headset. The **Itan > Debug** menu presses buttons,
simulates the bow, the calabash grab and the voice answer, picks quiz cards and skips
states. Press **N** in Play mode to skip to the next state.

## Building for Quest 2

1. Plug in the headset (developer mode on) and accept the USB debugging prompt.
2. Run **Itan > Build > Build Quest APK**. It writes `Builds/Itan.apk`.
3. Install it with `adb install -r Builds/Itan.apk`, or use **File > Build Profiles > Android > Build And Run**.

The build settings live in code so they can be re-applied at any time: IL2CPP, ARM64,
minimum API 32, Vulkan, OpenXR with the Meta Quest feature, single-pass multiview and ASTC
textures. See `Assets/_Project/Scripts/Editor/QuestBuildSetup.cs` and `QuestPerformanceSetup.cs`.

## Rebuilding the content

Almost everything in the scenes is built by editor scripts, so it can be rebuilt after a change:

| Menu | What it does |
|---|---|
| Itan > Story > Dress Story World | Rebuilds the village, the dead land, the rope spot and the sky home |
| Itan > Story > Build Story Cast | Places Ajá, his mother and Ìjàpá with their animations |
| Itan > Story > Wire Animations and Moments | Re-links the story moments to the scene |
| Itan > Hub > Dress Hub / Build Elder / Add Visitor Hands | The fireside, the elder and the visitor's hands |
| Itan > Narrator > Export Narration Script / Link Narration Clips | Exports the narrator's lines and links the recorded clips in `Assets/_Project/Audio/Narration` |
| Itan > Lighting > Bake All | Bakes the lighting for both scenes. **Re-bake after dressing either scene.** |

## Project layout

```
Assets/_Project/
  Scenes/        Hub.unity (the fireside and the session) and StoryWorld.unity (the tale), with their baked lighting
  Scripts/
    SessionManager.cs, States/   a visit as a chain of states
    Story/, Narration/, UI/      beats, lines, subtitles, word cards, menus
    World/                       story moments (rope, climb, feast, fall), scene transitions, characters
    Logic/                       plain C# rules (bow, voice, quiz) with edit-mode tests
    Player/, Interaction/        seat, hands, calabash, haptics
    Editor/                      the builders and setup menus above
  Data/          ScriptableObjects: the opening, three beats and the closing
  Tests/EditMode BowCheck, VoiceCheck and QuizLogic tests
```

Run the tests from **Window > General > Test Runner > EditMode**.

## Performance

- Baked lighting: the sun is a Mixed light in Subtractive mode, scenery is lit from lightmaps
  and the characters from light probes. The fire is the only extra real-time light.
- Static batching for all scenery, with the SRP Batcher on.
- ASTC textures with mipmaps, capped at 1024 for 3D art.
- 4x MSAA, no HDR, short hard shadows for the characters only.
- IL2CPP release build, ARM64, Vulkan and single-pass multiview.

## Credits

See [CREDITS.md](CREDITS.md). In short: characters by Polygonal Mind (CC BY 4.0), the elder
from Adobe Mixamo, VR hands by PolyOne Studio (CC BY 4.0), environment and animations by
Quaternius (CC0), textures from Poly Haven (CC0), Alegreya fonts (OFL), narrator voice made
with Kokoro (Apache 2.0).

## Tech

- Unity 6 (6000.3.27f1), Universal Render Pipeline
- OpenXR + XR Interaction Toolkit 3.5
- Android build for Meta Quest 2, controllers, seated
