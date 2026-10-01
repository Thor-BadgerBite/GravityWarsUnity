# CLAUDE.md – working rules for Gravity Wars

Read this first in every session. Then read `docs/GRAVITY_WARS_GDD.md` (the GDD) and, for the system you touch, `docs/CODE_AUDIT.md`.

## Source of truth
- `docs/GRAVITY_WARS_GDD.md` is the single source of truth for design **and** for what is built. Follow it.
- When code and the GDD disagree, fix one of them deliberately in the same session and say which. Never leave them drifting.
- If a rule you need is missing or unclear, ask Thomas before deciding. Do not invent design decisions; put undecided items into the GDD "Open Questions" with options and a recommendation.
- When a design decision is made (by Thomas or agreed in chat), update the GDD section, the Tunable Parameters table and the Changelog in the same commit as the code.
- `docs/CODE_AUDIT.md` lists known problems with severity. When you fix one, remove it from the audit and mention it in the GDD Changelog. When you find a new one you are not fixing, add it to the audit with file/class/method and a suggested fix.
- `docs/_work/*.md` are backing notes from the 0.1 assessment. They may go stale; the GDD wins.
- `docs/_archive/` holds the pre-0.1 documents. Read them only as evidence of past intent; never treat them as current rules.

## Engine and language
- Unity **2022.3.34f1** (`ProjectSettings/ProjectVersion.txt`). That means **C# 9 only**: no file-scoped namespaces, no `global using`, no record structs, no `required` members, no raw string literals, no list patterns. Switch expressions, tuples, pattern matching, `init` setters and records (classes) are fine.
- Nothing can be compiled or played from a chat session. State explicitly what was verified statically (code, scene/prefab YAML) and what Thomas must check in the editor.
- Never edit `.unity`, `.prefab` or `.asset` files by hand unless the GDD milestone explicitly asks for a data change and the change is trivial YAML (a serialized number). Scene and prefab wiring is editor work; list it as such.

## Balance, economy, configuration
- Balance and economy values come from config assets (ScriptableObjects) or the one static config class the GDD names, never from literals in gameplay code. The GDD "Tunable Parameters" table says where each value lives; if you add a value, add it there.
- Generated content under `Assets/Resources/GeneratedContent/` is produced by `Assets/Editor/GameContentGenerator.cs`. Change the generator, then let Thomas re-run *Tools → Gravity Wars → Generate Game Content*. Do not hand-edit generated assets.
- One formula per concept. Do not add a second XP curve, a second rank table, a second missile-restriction rule or a second reward table. If you find one, that is a CODE_AUDIT entry.

## Working method
- Work on **one milestone (or one clearly named part of it)** from the GDD Implementation Plan at a time. Say which milestone you are on at the start of the session.
- Small, reviewable steps: one system per commit, commit message names the milestone. Push after each finished step.
- Before changing a system, read its `docs/_work/<system>.md` note and the matching CODE_AUDIT section.
- Prefer deleting duplicate/dead code over keeping it "just in case". The audit marks what is dead.
- Never create scratch scripts, test-only MonoBehaviours, temp files or throw-away scenes inside the repository. Use your scratchpad outside the repo.
- Do not add third-party packages or Asset Store content. Do not modify anything under the ignored packs below.

## Ignore completely (third-party, never read or edit)
`Assets/Sci-Fi UI Collection`, `Assets/Sci-Fi UI`, `Assets/TextMesh Pro`, `Assets/LeanTween`, `Assets/StarSparrow`, `Assets/Planets of the Solar System 3D`, `Assets/Nebula Skyboxes`, `Assets/2D Space Kit`, `Assets/3Skyboxes`, `Assets/Honeti`, and any other clear Asset Store package. Asset-only folders (`+3d Objects+`, `+Audio+`, `+Graphics+`) need no reading. (`Assets/Obsolete` was deleted in 0.3; do not recreate it.)

## Code conventions this codebase needs
- **Folders**: match/gameplay code lives in `Assets/` root today (`GameManager`, `PlayerShip`, `Missile3D`, …); meta-game systems have their own folders (`Ship System`, `+Active Perks+`, `Progression System`, `Online`, `Quests`, `Achievements`, `Leaderboards`, `Networking`, `Multiplayer`, `UI`). New scripts go into the folder of their system as listed in the GDD "Implementation Plan → Folder layout"; no new root-level scripts, no new `+Name+` folders. Editor-only code goes under an `Editor/` folder.
- **Naming**: PascalCase types and methods, camelCase public serialized fields (existing style), `_camelCase` private fields, `UPPER_SNAKE` constants. One class per file, file named after the class (`Networking/NetworkManager.cs` → `GravityWarsNetworkManager` is the remaining known offender; rename when touched).
- **Singletons**: managers that must survive scenes use the lazy pattern already used by `ProgressionManager` (`Instance` → `FindObjectOfType` → `AddComponent` on a `[Name]` object, `DontDestroyOnLoad`). Do not add a new `Instance` singleton unless the GDD lists it under "Key classes". Scene-bound objects (GameManager, UI panels) must **not** be `DontDestroyOnLoad`.
- **Scene names** only through the one `SceneNames` constants class the GDD specifies; never `LoadScene("literal")`.
- **Per-frame cost**: no `FindObjectOfType`, `GetComponent`, `Resources.Load`, LINQ or string building in `Update`/`FixedUpdate`; cache references in `Awake`/`Start`. No `Time.frameCount % n` timers; use elapsed time.
- **Async**: `async void` only for Unity event handlers with a try/catch; everything else returns `Task`. No blocking `.Wait()`/`.Result` on the main thread.
- **Serialization**: `PlayerAccountData` is saved with `JsonUtility`. Only `[Serializable]` classes, lists, primitives and Unix-timestamp `long`s survive; no `DateTime`, no `Dictionary`.
- **Networking**: everything under `#if UNITY_NETCODE_GAMEOBJECTS` stays compiled out until the GDD multiplayer milestone starts. Do not set the define casually; it currently breaks compilation (see CODE_AUDIT).
- **Logging**: prefix logs with `[ClassName]`; gate per-frame or per-step logs behind `DebugSettings`.
- **Comments in code**: explain *why*, cite the GDD section for rules ("GDD §5.2").

## End of every session
1. Update the GDD **Status** line (header) and the **Implementation Plan** milestone progress (mark the current milestone, tick "Done when" items that are done).
2. Add a **Changelog** row (next 0.x version, what changed, which audit items closed).
3. Update `docs/CODE_AUDIT.md` (remove fixed items, add new ones).
4. Commit and push. Do not create a pull request unless asked.
