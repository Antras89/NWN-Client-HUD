# NWN Client HUD

A standalone Windows overlay for tracking active effects in Neverwinter Nights: Enhanced Edition on SWLOR.

## Download and install

Download [**Setup.exe**](https://github.com/Antras89/NWN-Client-HUD/releases/latest/download/Setup.exe) and run it. Setup installs the HUD and required assets under `%LOCALAPPDATA%\Programs\NWN Client HUD`, creates the installation folders, and adds a **NWN Client HUD** desktop shortcut. It does not require administrator rights.

## Update

Download [**Update.exe**](https://github.com/Antras89/NWN-Client-HUD/releases/latest/download/Update.exe) and run it. The updater checks for an existing installation, downloads the newest published HUD package, compares installed files, and replaces only changed application files and icons. It keeps `settings.json` and `learned.json`, backs up replaced files under `%LOCALAPPDATA%\NWN Client HUD Backups`, and restarts the HUD.

The latest release also includes `NWN-HUD-Defensive-installer.zip` for users who prefer to install manually with PowerShell.

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
- Installer upgrades preserve settings and learned data and create a timestamped backup of changed files.

## Build and releases

Pushing to `add-hud-installer` builds the Windows x64 `Setup.exe` and `Update.exe` and publishes them, together with the installer package, to the latest GitHub Release. Pushing a `v*` tag publishes versioned release assets. Pull requests build both executables without publishing a release.

The self-contained Windows x64 installers use .NET 8 and do not require a separate runtime installation.
