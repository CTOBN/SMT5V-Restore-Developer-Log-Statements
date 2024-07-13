using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Pointers;
using Reloaded.Mod.Interfaces;
using smt5v.SMT5V_Logger.Template;
using smt5v.SMT5V_Logger.Utilities;
using smt5v.SMT5V_Logger.Configuration;
using System.Runtime.InteropServices;

namespace smt5v.SMT5V_Logger
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private IHook<PrintGameMessageDelegate> ConsoleLogReverseWrapper;

        private IHook<IsDebugEnabledDelegate> DebugEnabledReverseWrapper;

        private readonly string consoleLogSignature = "40 53 48 83 EC 30 8B 89 ?? ?? ?? ?? 48 89 D3";
        private readonly string debugEnabledSignature = "8B 89 ?? ?? ?? ?? B2 09";

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            CreateTextFile();

            bool initialiseSuccess = SigScanUtil.Initialise(_logger, _configuration, _modLoader);

            if (_hooks == null)
            {
                _logger.WriteLine("hooks is null");
                return;
            }

            SigScanUtil.SigScan(
                consoleLogSignature,
                address =>
                {
                    ConsoleLogReverseWrapper = _hooks.CreateHook<PrintGameMessageDelegate>(PrintGameMessage, address);
                    ConsoleLogReverseWrapper.Activate();
                }
             );

            SigScanUtil.SigScan(
                debugEnabledSignature,
                address =>
                {
                    DebugEnabledReverseWrapper = _hooks.CreateHook<IsDebugEnabledDelegate>(IsDebugEnabled, address);
                    DebugEnabledReverseWrapper.Activate();
                }
             );
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        private delegate void PrintGameMessageDelegate(long firstParamUnknownPointer, Ptr<nint> pointerToPointerToTextString);

        private void PrintGameMessage(long firstParamUnknownPointer, Ptr<nint> pointerToPointerToTextString)
        {
            string? actualString = StringFromPointer(pointerToPointerToTextString.Get());
            if (actualString != null)
            {
                WriteToConsole(actualString);
                WriteTextToFile(actualString);
            }
            else
            {
                SigScanUtil.LogError("String was null");
            }
        }

        private delegate bool IsDebugEnabledDelegate(nint param1);

        private bool IsDebugEnabled(nint param1)
        {
            return true;
        }

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion

        private string? StringFromPointer(nint pointer)
        {
            return Marshal.PtrToStringAuto(pointer);
        }

        private void WriteToConsole(string text)
        {
            if (_configuration.WriteToConsole == false) return;
            SigScanUtil.Log(text);
        }

        private void CreateTextFile()
        {
            if (_configuration.WriteToFile == false || _configuration.LoggingFilepath == null) return;
            File.WriteAllText($"{(_configuration.LoggingFilepath)}.log", "");
        }

        private void WriteTextToFile(string text)
        {
            if (_configuration.WriteToFile == false || _configuration.LoggingFilepath == null) return;
            File.AppendAllText($"{(_configuration.LoggingFilepath)}.log", $"{text}\n");
        }
    }
}