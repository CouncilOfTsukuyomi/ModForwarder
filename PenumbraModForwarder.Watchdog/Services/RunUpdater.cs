﻿using System.Diagnostics;
using NLog;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Watchdog.Interfaces;

namespace PenumbraModForwarder.Watchdog.Services;

public class RunUpdater : IRunUpdater
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly IDownloadUpdater _downloadUpdater;

    public RunUpdater(IDownloadUpdater downloadUpdater)
    {
        _downloadUpdater = downloadUpdater;
    }

    /// <summary>
    /// Downloads and extracts the updater, then runs the downloaded file
    /// with the specified arguments.
    /// 
    /// The programToRunAfterInstallation parameter is optional. If null or empty,
    /// no additional program is specified for launch after the update process.
    /// </summary>
    public async Task<bool> RunDownloadedUpdaterAsync(
        string versionNumber,
        string gitHubRepo,
        string installationPath,
        bool enableSentry,
        string? programToRunAfterInstallation = null)
    {
        _logger.Info("Starting updater retrieval...");
        var ct = CancellationToken.None;

        var updaterExePath =
            await _downloadUpdater.DownloadAndExtractLatestUpdaterAsync(ct);

        if (updaterExePath == null)
        {
            _logger.Fatal("Updater retrieval failed. No file to run.");
            return false;
        }

        static string EscapeForCmd(string argument)
        {
            // Simple approach to escape double quotes so they stay intact if needed
            return argument.Replace("\"", "\\\"");
        }

        // Escape and quote all arguments
        var escapedVersion = EscapeForCmd(versionNumber);
        var escapedRepo = EscapeForCmd(gitHubRepo);
        var escapedPath = EscapeForCmd(installationPath);
        var escapedSentry = EscapeForCmd(enableSentry.ToString().ToLowerInvariant());

        var arguments = $"\"{escapedVersion}\" \"{escapedRepo}\" \"{escapedPath}\" \"{escapedSentry}\"";

        if (!string.IsNullOrWhiteSpace(programToRunAfterInstallation))
        {
            var escapedProgram = EscapeForCmd(programToRunAfterInstallation);
            arguments += $" \"{escapedProgram}\"";
        }

        _logger.Info("Updater retrieved at {updaterExePath}. Attempting to run.", updaterExePath);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = updaterExePath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            Process.Start(processStartInfo);
            _logger.Info("Updater is now running.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error while attempting to run the updater file.");
            return false;
        }
    }
}