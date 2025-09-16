# ![logo](images/Fire%20Elemental.png) Waypoint Creator VMaNGOS

Fork of Waypoint Creator with main focus on handling large sniffs better.

Creates NPC paths using parsed packet files and visualizes them on the game map with SkiaSharp rendering.

## Features

- **Map Visualization**: View waypoints on game maps using SkiaSharp
- **Multi-Server Support**: VMaNGOS, TrinityCore, and CMaNGOS SQL output
- **Database Integration**: Auto-fetch creature names from your database
- **Search**: Find NPCs by searching for names or NPC entries
- **Map Navigation**: Zoom and pan around the game world
- **Multi-file Support**: Process multiple sniff files simultaneously

## Requirements

- Parsed packet files from WowPacketParser
- Map files in same directory as executable, download maps here: https://drive.google.com/uc?export=download&id=1fFRgY7gVEbK2bwNQA8RTooE6BI9kydsi
- Database connection (optional, for creature names)

## Output Support

- VMaNGOS (`creature_movement`)
- TrinityCore (`waypoint_path`)
- CMaNGOS (`creature_movement`)
- C++ code generation

![main_window](images/main_window.png)
