# TES3MP Manager

Bring Save and Load functionality to your TES3MP server.

# Installation

- Download and extract the .zip file. 
- Copy backup_manager.lua to server\scripts\custom
- Edit file server\scripts\customScripts.lua and add the following line
  require("custom/backup_manager")
- Run the executable


## Key Features

-   **Automatic Backups:** Creates scheduled zip archives of your server data.
-   **Rollback:** Restores your server to a previous state from a backup.
-   **In-game control:** Trigger a rollback using the `/backup` in-game command (via a `command.json`). You can also shutdown and restart.
-   **Server Management:** Start, stop, and restart the server from the app.

## Usage

1.  Set server and backup paths.
2.  Configure backup interval and compression level.
3.  Start the backup process.
4.  Use the app to restore from backups or use `/backup <timestamp>` in-game (refer to the `backups.json` file to see available timestamps). Alternatively `/shutdown` and `/restart` will perform the action.
5.  Manage the server using the start, shutdown and restart buttons.

![image](https://github.com/user-attachments/assets/782ee370-012d-4808-a5e2-220b1391119e)
