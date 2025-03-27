using System.Diagnostics;
using System.Reflection;
using Org.BouncyCastle.Crypto.Digests;
using Xunit.Sdk;

namespace DNDWithin.Application.Tests.Integration;

public sealed class SkipIfEnivronmentMissingTheory : TheoryAttribute
{
    public SkipIfEnivronmentMissingTheory()
    {
        if (!IsDockerRunning())
        {
            Skip = "Skipping test as Docker isn't running";
        }
    }

    private static bool IsDockerRunning()
    {
        try
        {
            Process process = new Process
                              {
                                  StartInfo = new ProcessStartInfo
                                              {
                                                  FileName = "docker",
                                                  Arguments = "info",
                                                  RedirectStandardOutput = true,
                                                  UseShellExecute = false,
                                                  CreateNoWindow = true
                                              }
                              };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    
}