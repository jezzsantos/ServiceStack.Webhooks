using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Configuration;
using ServiceStack.Logging;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public abstract class AzureIntegrationTestBase
    {
        private const string AzureStorageEmulatorServiceProcessName = @"AzureStorageEmulator";
        private const string AzureStorageToolSettingName = @"AzureIntegrationTestBase.AzureStorageTool";
        private const string StorageResetArgumentsSettingName = @"AzureIntegrationTestBase.AzureStorageResetArguments";
        private const string StorageStartArgumentsSettingName = @"AzureIntegrationTestBase.AzureStorageStartArguments";
        private const string StorageStopArgumentsSettingName = @"AzureIntegrationTestBase.AzureStorageStopArguments";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AzureIntegrationTestBase));
        private static readonly IAppSettings Settings = new AppSettings();

        [OneTimeSetUp]
        public void InitializeAllTests()
        {
            Logger.Debug("Initialization of azure storage environment for testing");
            StartAzureTestEnvironment();
        }

        [OneTimeTearDown]
        public void CleanupAllTests()
        {
            Logger.Debug("Cleanup of azure environment after testing");
            CleanupAzureTestEnvironment();
            KillAzureTestEnvironment();
        }

        protected static void StartAzureTestEnvironment()
        {
            // Cleanup old instances
            if (Process.GetProcessesByName(AzureStorageEmulatorServiceProcessName).Any())
            {
                CleanupAzureTestEnvironment();
            }

            StartupStorage();
        }

        protected static void CleanupAzureTestEnvironment()
        {
            if (Process.GetProcessesByName(AzureStorageEmulatorServiceProcessName).Any())
            {
                ShutdownStorage();
            }
        }

        protected static void KillAzureTestEnvironment()
        {
            if (Process.GetProcessesByName(AzureStorageEmulatorServiceProcessName).Any())
            {
                KillProcesses();
            }
        }

        protected static void StartupStorage()
        {
            Logger.Debug("Starting up the emulated storage environment");
            var toolPath = Settings.GetString(AzureStorageToolSettingName);
            var storageArgs = Settings.GetString(StorageResetArgumentsSettingName);

            RunCommand(toolPath, storageArgs);

            storageArgs = Settings.GetString(StorageStartArgumentsSettingName);

            RunCommand(toolPath, storageArgs);
        }

        protected static void ShutdownStorage()
        {
            Logger.Debug("Shutting down emulated storage environment");
            var toolPath = Settings.GetString(AzureStorageToolSettingName);
            var storageArgs = Settings.GetString(StorageStopArgumentsSettingName);

            RunCommand(toolPath, storageArgs);
        }

        protected static void RunCommand(string toolPath, string args, bool elevated = true)
        {
            var toolPathResolved = Environment.ExpandEnvironmentVariables(toolPath);
            var startInfo = new ProcessStartInfo(toolPathResolved, args)
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            if (elevated)
            {
                startInfo.Verb = "runas";
                startInfo.UseShellExecute = true;
            }

            using (var proc = new Process {StartInfo = startInfo})
            {
                try
                {
                    proc.Start();
                    proc.WaitForExit();
                }
                catch
                {
                    Logger.Debug("Failed to run command: {0} {1}".Fmt(toolPath, args));

                    //ignore issue
                }
            }
        }

        protected static void KillProcesses()
        {
            try
            {
                // Kill remaining process(es)
                Process.GetProcessesByName(AzureStorageEmulatorServiceProcessName).ToList().ForEach(p =>
                {
                    p.Kill();
                    p.WaitForExit();
                });
            }
            catch (Exception)
            {
                //ignore problem
            }
        }
    }
}