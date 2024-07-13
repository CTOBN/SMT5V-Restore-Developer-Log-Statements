using Reloaded.Mod.Interfaces;
using smt5v.SMT5V_Logger.Configuration;
using System.Diagnostics;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace smt5v.SMT5V_Logger.Utilities
{
    internal class SigScanUtil
    {
        private static ILogger _logger;
        private static Config _config;
        private static IStartupScanner _startupScanner;

        internal static nint BaseAddress { get; private set; }

        internal static bool Initialise(ILogger logger, Config config, IModLoader modLoader)
        {
            _logger = logger;
            _config = config;
            using var thisProcess = Process.GetCurrentProcess();
            BaseAddress = thisProcess.MainModule!.BaseAddress;

            var startupScannerController = modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
            {
                LogError($"Unable to get controller for Reloaded SigScan Library and I am furious");
                return false;
            }

            return true;
        }

        internal static void LogDebug(string message)
        {
            _logger.WriteLine($"[SMTV Logger] {message}");
        }

        internal static void Log(string message)
        {
            _logger.WriteLine($"[SMTV Logger] {message}");
        }

        internal static void LogError(string message, Exception e)
        {
            _logger.WriteLine($"[SMTV Logger] {message}: {e.Message}", System.Drawing.Color.Red);
        }

        internal static void LogError(string message)
        {
            _logger.WriteLine($"[SMTV Logger] {message}", System.Drawing.Color.Red);
        }

        internal static void SigScan(string pattern, Action<long> action)
        {
            _startupScanner.AddMainModuleScan(pattern, result =>
            {
                if (!result.Found)
                {
                    LogError($"Scan was a horrible failure, and I am furious");
                    return;
                }

                action(result.Offset + BaseAddress.ToInt64());
            });
        }

        /// <summary>
        /// Gets the address of a global from something that references it
        /// </summary>
        /// <param name="ptrAddress">The address to the pointer to the global (like in a mov instruction or something)</param>
        /// <returns>The address of the global</returns>
        internal static unsafe nuint GetGlobalAddress(nint ptrAddress)
        {
            return (nuint)(*(int*)ptrAddress + ptrAddress + 4);
        }
    }
}
