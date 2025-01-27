local BackupMenuCommand = {}

-- File paths
local jsonFilePath = "./server/scripts/custom/backup_manager/command.json"
local backupJsonFilePath = "./server/scripts/custom/backup_manager/backups.json"

-- Function to write to JSON file
local function writeToJson(data)
    local file = io.open(jsonFilePath, "w")
    if file then
        file:write(data)
        file:close()
        return true
    else
        return false
    end
end

-- Function to parse backups.json into a Lua table
local function getBackupList(filePath)
    local file = io.open(filePath, "r")
    if file then
        local content = file:read("*a")
        file:close()
        local backupArray = {}
        for line in content:gsub("[%[%]\"]", ""):gmatch("[^,]+") do
            table.insert(backupArray, line:match("^%s*(.-)%s*$")) -- Trim whitespace
        end

        return backupArray
    else
        return nil
    end
end

-- Function to show the main menu
local function showMainMenu(pid)
    local menuId = 1001 -- Unique menu ID for the main menu
    local label = "Select an action:"
    local buttons = "Rollback;Shutdown;Restart;Cancel"
    
    tes3mp.CustomMessageBox(pid, menuId, label, buttons)
end

-- Function to show the backup ListBox menu
local function showBackupListBox(pid, backups)
    if backups and #backups > 0 then
        local menuId = 1002 -- Unique menu ID for the ListBox
        local label = "Select a backup to rollback:"
        local options = table.concat(backups, "\n") .. "\nCancel" -- Add "Cancel" as the last option
        
        tes3mp.ListBox(pid, menuId, label, options)
    else
        tes3mp.SendMessage(pid, "No backups available.\n", false)
    end
end

-- Show rollback range buttons after selecting backup
local function showOptionButtons(pid)
    local menuId = 1003 -- Unique menu ID for the CustomMessageBox
    local label = "Choose an option for the selected backup:"
    local buttons = "Everything;Cell;Player;World;Cancel"

    tes3mp.CustomMessageBox(pid, menuId, label, buttons)
end

-- Function to handle ListBox menu responses
local function handleBackupListBoxResponse(pid, selectedIndex, backups)
    if selectedIndex >= 0 and selectedIndex < #backups then
        local selectedBackup = backups[selectedIndex + 1] -- Convert 0-based index to 1-based

        -- Store the selected backup for this player session
        BackupMenuCommand.selectedBackup[pid] = selectedBackup

        showOptionButtons(pid)
    end
    -- No action needed for "Cancel" button or invalid selection
end

-- Function to handle CustomMessageBox button responses
local function handleOptionButtonsResponse(pid, buttonIndex)
    local selectedBackup = BackupMenuCommand.selectedBackup[pid]
    local options = { "Everything", "Cell", "Player", "World", "Cancel" }

    if selectedBackup and buttonIndex >= 0 and buttonIndex < #options - 1 then
        local selectedOption = options[buttonIndex + 1] -- Convert 0-based index to 1-based

        -- Write both selections to command.json
        local jsonData = "{ \"backup\": \"" .. selectedBackup .. "\", \"option\": \"" .. selectedOption .. "\" }\n"
        local success = writeToJson(jsonData)

        if success then
            tes3mp.SendMessage(pid, "Performing rollback...\n", false)
        else
            tes3mp.SendMessage(pid, "Failed to send command.\n", false)
        end

        -- Clear the stored backup for this player session
        BackupMenuCommand.selectedBackup[pid] = nil
    end
    -- No action needed for "Cancel" button
end

-- Function to handle main menu responses
local function handleMainMenuResponse(pid, buttonIndex)
    if buttonIndex == 0 then
        -- Rollback selected
        local backups = getBackupList(backupJsonFilePath)
        if backups then
            BackupMenuCommand.activeBackups[pid] = backups -- Store backups for this player session
            showBackupListBox(pid, backups)
        else
            tes3mp.SendMessage(pid, "No backups available.\n", false)
        end
    elseif buttonIndex == 1 then
        -- Shutdown selected
        local jsonData = "{ \"option\": \"shutdown\" }\n"
        if writeToJson(jsonData) then
            tes3mp.SendMessage(pid, "Shutdown command sent.\n", false)
        else
            tes3mp.SendMessage(pid, "Failed to send shutdown command.\n", false)
        end
    elseif buttonIndex == 2 then
        -- Restart selected
        local jsonData = "{ \"option\": \"restart\" }\n"
        if writeToJson(jsonData) then
            tes3mp.SendMessage(pid, "Restart command sent.\n", false)
        else
            tes3mp.SendMessage(pid, "Failed to send restart command.\n", false)
        end
    end
    -- No action needed for "Cancel" button
end

-- Function to handle the /rollback command
local function handleRollbackCommand(pid, cmd)
    -- Check if the player is an admin
    local staffRank = Players[pid].data.settings.staffRank
    if staffRank == nil or staffRank < 1 then
        tes3mp.SendMessage(pid, "You do not have permission to use this command.\n", false)
        return
    end

    -- Show the main menu
    showMainMenu(pid)
end

-- Hook into the GUI action event to handle menu interactions
function BackupMenuCommand.OnGUIAction(eventStatus, pid, id, data)
    if id == 1001 then
        -- Handle main menu response
        local buttonIndex = tonumber(data)
        handleMainMenuResponse(pid, buttonIndex)

    elseif id == 1002 then
        -- Handle backup ListBox menu response
        local selectedIndex = tonumber(data)
        local backups = BackupMenuCommand.activeBackups[pid]
        if backups then
            handleBackupListBoxResponse(pid, selectedIndex, backups)
            BackupMenuCommand.activeBackups[pid] = nil -- Clear backups to free memory
        end

    elseif id == 1003 then -- CustomMessageBox menu ID
        local buttonIndex = tonumber(data)
        handleOptionButtonsResponse(pid, buttonIndex)
    end
end

-- Initialize tables to store active backups
BackupMenuCommand.activeBackups = {}
BackupMenuCommand.selectedBackup = {}

-- Register the /rollback command
customCommandHooks.registerCommand("rollback", handleRollbackCommand)

-- Register the script's functions
customEventHooks.registerHandler("OnGUIAction", BackupMenuCommand.OnGUIAction)

return BackupMenuCommand
