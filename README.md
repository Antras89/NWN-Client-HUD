# NWN Client HUD

A standalone Windows overlay for tracking active effects in Neverwinter Nights: Enhanced Edition on SWLOR.

## Features

- Reads the NWN chat log and tracks effect applications, removals, expirations, and status-effect timer reports.
- Organizes active effects into **Stances**, **Healing**, **Defensive**, **Buffs**, **Debuffs**, and **Other Effects**.
- Shows effect icons and remaining-time counters, with optional duration bars and full-row timer bars.
- Uses a duration catalog checked against SWLOR server definitions. Known fixed durations start immediately; log reports refine variable or estimated timers; unknown durations are left blank.
- Includes defensive Ward skills and related effects such as Saber Ward, Sentinel Guard, Danger Sense, and Perfect Aegis. Saber Ward and Perfect Aegis replace each other in the display.
- Tracks Mimicry effects, including Snap Rush’s 15-second Hasten I, Warden Sweep, and Warden Wall Stance Aura.
- Handles permanent toggle stances and effects without showing fabricated countdowns.
- Detects the logged-in character name from the game log through **Check character name**.
- Offers compact mode, the **Small panel** preset, collapse and expand, repositioning, resizing, and transparency controls.
- Lets players show or hide stances and debuffs, hide stale unconfirmed debuffs, and estimate silent buff refreshes.
- Includes HUD refresh, log diagnostics, and an option to start with Windows/NWN.
- Installer upgrades preserve `settings.json` and `learned.json`, create a timestamped backup, and restart the HUD.

## Installation

Extract the complete package and run `install.ps1` in Windows PowerShell. The installer places the HUD under `%LOCALAPPDATA%\Programs\NWN Client HUD`.

The installer package is available as `NWN-HUD-Defensive-installer.zip` in the project release/output package.

## Project contents

- `NWN.ClientHUD.exe` and its runtime files
- `icons/`, `icons.json`, and `buff-durations.json`
- `install.ps1`
- `source/` with the application source
- English installer instructions

