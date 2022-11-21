﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics;
using System.IO;

namespace Skoruba.Duende.IdentityServer.Shared.Configuration.Helpers
{
    public static class ShellHelpers
    {
        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            if (File.Exists("/bin/bash"))
            {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                var result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result;
            }
            return string.Empty;
        }
    }
}