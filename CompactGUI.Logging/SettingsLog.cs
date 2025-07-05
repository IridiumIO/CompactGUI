using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CompactGUI.Logging;

public static partial class SettingsLog
{

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding to Context Menus")]
    public static partial void AddingToContextMenus(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding to Context Menus: Success")]
    public static partial void AddingToContextMenusSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Adding to Context Menus: Failed")]
    public static partial void AddingToContextMenusFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Removing from Context Menus")]
    public static partial void RemovingFromContextMenus(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Removing from Context Menus: Success")]
    public static partial void RemovingFromContextMenusSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Removing from Context Menus: Failed")]
    public static partial void RemovingFromContextMenusFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding Start Menu Shortcut")]
    public static partial void AddingStartMenuShortcut(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Removing Start Menu Shortcut")]
    public static partial void RemovingStartMenuShortcut(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Settings Saved")]
    public static partial void SettingsSaved(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Setting Environment Variables")]
    public static partial void SettingEnvironmentVariables(ILogger logger);


}

