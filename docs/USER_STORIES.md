---
codex: 1
project: GTFO.CrashTestDummies
code: G.CTD
layer: stories
status: living
updated: 2026-10-16
---
<!-- GENERATED from MindAttic.Workflow/data/workflow.db by `workflow db render`. Do not edit: this file is rewritten from the database; change stories and tickets with the workflow CLI. -->

# GTFO.CrashTestDummies — User Stories
> ✅ done (shipped & tested) · 🟡 partial · ⬜ planned. Every ✅ cites the test.

## Priority backlog
_Nothing recorded yet._

## Tickets
> Permanent log of every change and fix — this repo's ticket system. Rows are append-only: never
> delete or reword one; only move its Status (open → closed / wont-do) and fill in Ref. Status is
> a word, not a glyph, so tickets don't count toward the story index. Keep this section last.

| ID | Date | Type | Status | Summary | Ref |
|---|---|---|---|---|---|
| G.CTD-T1 | 2026-10-16 | docs | closed | Start the permanent ticket log in the workflow database | — |
| G.CTD-T2 | 2026-10-16 | chore | closed | install.ps1 adds this repo's 15 .prose/commands stubs, AGENTS.md pointer for the shared global commands, so any LLM client finds them | mindattic-agent-standard/install.ps1 run 2026-10-16 |
| G.CTD-T3 | 2026-10-16 | chore | open | Bug hunt: run an exhaustive /bughunt over the whole repo and fix every real bug found (each fix under its own fix ticket), then hunt again; close this only when a full pass finds nothing | user request 2026-10-16: approved repo sweep |
| G.CTD-T4 | 2026-10-16 | chore | open | Run the full unit test suite and fix every failure (each under its own fix ticket) until it passes; put the passing run in --ref. No test project: close this wont-do saying so | user request 2026-10-16: approved repo sweep |
| G.CTD-T5 | 2026-10-16 | chore | open | Clean up code and docs: remove dead code, stale comments and outdated or duplicated docs, and simplify where behaviour doesn't change (/simplify); tests stay green | user request 2026-10-16: approved repo sweep |
| G.CTD-T6 | 2026-10-16 | docs | open | Bring README.md up to date with the code as it is now (what it is, setup, usage, commands, current state), following codex-standard/README-STYLE.md; README.htm regenerated if the repo has one | user request 2026-10-16: approved repo sweep |
| G.CTD-T7 | 2026-10-16 | chore | closed | Add .gitignore, README.md, build.ps1, docs, package, reference, src, tests | — |
| G.CTD-T8 | 2026-10-17 | feature | closed | Spawn 100 sleeping Strikers (the most basic enemy) 10-100 m from the drop point instead of a mix of sleeper types, as the user asked | SpawnPickerTests; DummySelectorTests.StrikerSleeperFilter_PicksThePlainStrikerSleeper |
| G.CTD-T9 | 2026-10-18 | fix | closed | DoorAmbush.cs:27 only fires on a direct Closed_LockedWithChainedPuzzle_Alarm -> Open change, but alarm doors pass through ChainedPuzzleActivated/Unlocked/Opening first, overwriting the stored previous status, so the ambush likely never fires | 1059647 |
| G.CTD-T10 | 2026-10-18 | fix | closed | Visibility.cs:36 (used by EnemyCleanup.cs:61 and spawning) the eye-to-feet Linecast on default layers hits the enemy's own colliders or the floor at the end point, so visible dummies count as hidden: cleanup despawns Strikers in plain view and spawns can pop in visibly | c301a39 |
| G.CTD-T11 | 2026-10-18 | fix | closed | EnemyCleanup.cs:58-64 recycles by age and visibility only, never checking awake/damaged/chasing, so dummies woken by combat (or any when SpawnAsSleepers=false) vanish mid-fight | a849a6a |
| G.CTD-T12 | 2026-10-18 | fix | closed | EnemySpawner.cs:70-78 free room is computed once before the spawn loop (which yields every 5 spawns), so a door ambush and an ambient scatter running together can exceed the population cap | 41a6719 |
| G.CTD-T13 | 2026-10-18 | fix | closed | LevelPatches.cs:64-70 / AmbientSpawner.cs:29 no exception handling in the spawn/cleanup coroutines: one throw in the startup scatter stops ambient spawning and cleanup for the whole level | b4a214e |
| G.CTD-T14 | 2026-10-18 | fix | closed | LevelPatches.cs:21-41 spawn/cleanup loops are only stopped at the next level build, so they keep running (and can spawn) on the expedition success/fail screen | 7b14f62 |
| G.CTD-T15 | 2026-10-18 | fix | closed | DoorAmbush.cs, Visibility.cs and EnemyCleanup.cs were another session's uncommitted new files; committing them with the G.CTD-T9/T10/T11 fixes (1059647, c301a39, a849a6a) broke the build at HEAD. Untracked again: they stay on disk, now carrying those three fixes, and land with that work | 19efa01 |
| G.CTD-T16 | 2026-10-18 | feature | closed | Add alarm-door ambush, periodic ambient scatter, and population cleanup so the drop-point scatter keeps refilling instead of a one-time dump (SpawnAround, DoorAmbush, AmbientSpawner, EnemyCleanup, Visibility, new Ambush/Ambient/Cleanup settings) | 097fde3 |
| G.CTD-T17 | 2026-10-18 | feature | closed | Shared Insert/Delete infinite ammo/health toggles imported from GTFO.EZ (Shared/, published by GTFO.EZ tools/publish-shared.ps1) - user request 2026-10-18 | 59d3087 |
| G.CTD-T18 | 2026-10-18 | fix | closed | Shared code from GTFO.EZ (G.EZ-T67/T68): one warden-message flag across mods that can't get stuck, owner skips mods without shared code | — |
| G.CTD-T19 | 2026-10-18 | fix | closed | Visibility.cs:29-31 skipped every player without an FPSCamera, i.e. every client and bot on the host, so dummies could spawn or be recycled in plain view of them; they now count as looking every way, hidden only by line of sight | 9e77df1 |
| G.CTD-T20 | 2026-10-18 | fix | closed | DoorAmbush.cs:22 ignored General.Enabled, so alarm doors still spawned ambushes with the mod's master switch off | 77ad3d4 |
| G.CTD-T21 | 2026-10-18 | fix | closed | DoorAmbush.cs:55-81 picked the door's 'far side' as the gate node farther from a player within 15 m (or the door), so with players already past the door or none nearby the ambush could land on the players' own side; it now uses the room the gate leads into (m_linksTo, checked against LG_Zone.m_sourceGate) | e627643 |
| G.CTD-T22 | 2026-10-18 | fix | closed | AmbientSpawner.cs:20 / EnemyCleanup.cs:50 an interval setting of 0 or less ran a full scatter or cleanup sweep every frame (now at least 1 s), and EnemySpawner.cs:86-87 ran the raycasting visibility filter on every gathered spot, even ones outside the distance band (now band first) | d106e6c |
| G.CTD-T23 | 2026-10-18 | fix | closed | LevelPatches.cs:36-42, 85-86, DoorAmbush.cs:43 coroutines could outlive their level: StartupSequence started the ambient/cleanup loops even after StopRoutines ran mid-scatter, door ambush coroutines were never tracked or stopped, and aborting to the lobby stopped nothing; now every level coroutine is tracked and stopped on leaving InLevel, with a run counter checked after every wait | a444973 |
| G.CTD-T24 | 2026-10-18 | fix | closed | README.md 'How it works' said only door ambush and ambient use the visibility filter (all three triggers do) and that DoorAmbush remembers each door's previous eDoorStatus (it only remembers which doors were alarm-locked); package/README.md had neither claim | 68c15c9 |
| G.CTD-T25 | 2026-10-18 | chore | open | Shared code republished from GTFO.EZ: SharedOwner.Order now lists GTFO.AbsurdItems | — |
