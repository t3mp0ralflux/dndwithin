using System.Diagnostics;

namespace DNDWithin.Application.Tests.Integration;

public sealed class SkipIfEnvironmentMissingFact : FactAttribute
{
    public SkipIfEnvironmentMissingFact()
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