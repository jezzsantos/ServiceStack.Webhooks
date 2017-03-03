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
        private const string TestOutputDirectorySubstitution = @"%TestOutDir%";
        private const string AzureEmulatorServiceProcessName = @"DFService";
        private const string AzureDeployToolSettingName = @"AzureIntegrationTestBase.AzureDeployTool";
        private const string AzureStorageToolSettingName = @"AzureIntegrationTestBase.AzureStorageTool";
        private const string StorageResetArgumentsSettingName = @"AzureIntegrationTestBase.AzureStorageResetArguments";
        private const string StorageStartArgumentsSettingName = @"AzureIntegrationTestBase.AzureStorageStartArguments";
        private const string StorageStopArgumentsSettingName = @"AzureIntegrationTestBase.AzureStorageStopArguments";
        private const string ComputeStartupArgumentsSettingName = @"AzureIntegrationTestBase.AzureComputeStartupArguments";
        private const string ComputeShutdownArgumentsSettingName = @"AzureIntegrationTestBase.AzureComputeShutdownArguments";
        private static ILog logger;
        private static readonly IAppSettings Settings = new AppSettings();

        [OneTimeSetUp]
        public void InitializeAllTests()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            logger = LogManager.GetLogger(typeof(AzureIntegrationTestBase));
            logger.Debug("Initialization of azure storage environment for testing");
            StartAzureTestEnvironment();
        }

        [OneTimeTearDown]
        public void CleanupAllTests()
        {
            logger.Debug("Cleanup of azure environment after testing");
            CleanupAzureTestEnvironment();
            KillAzureTestEnvironment();
        }

        protected static void StartAzureTestEnvironment()
        {
            // Cleanup old instances
            if (Process.GetProcessesByName(AzureEmulatorServiceProcessName).Any())
            {
                CleanupAzureTestEnvironment();
            }

            StartupStorage();
            StartupComputeAndDeploy();
        }

        protected static void CleanupAzureTestEnvironment()
        {
            if (Process.GetProcessesByName(AzureEmulatorServiceProcessName).Any())
            {
                ShutdownCompute();
                ShutdownStorage();
            }
        }

        protected static void KillAzureTestEnvironment()
        {
            if (Process.GetProcessesByName(AzureEmulatorServiceProcessName).Any())
            {
                ShutdownCompute();
                ShutdownStorage();
                KillProcesses();
            }
        }

        protected static void StartupComputeAndDeploy()
        {
            logger.Debug("Starting up the emulated compute environment");
            var toolPath = Settings.GetString(AzureDeployToolSettingName);
            var computeArgs = SubstitutePaths(Settings.GetString(ComputeStartupArgumentsSettingName));

            RunCommand(toolPath, computeArgs);
        }

        protected static void ShutdownCompute()
        {
            logger.Debug("Shutting down the emulated compute environment");
            var toolPath = Settings.GetString(AzureDeployToolSettingName);
            var computeArgs = Settings.GetString(ComputeShutdownArgumentsSettingName);

            RunCommand(toolPath, computeArgs);
        }

        protected static void StartupStorage()
        {
            logger.Debug("Starting up the emulated storage environment");
            var toolPath = Settings.GetString(AzureStorageToolSettingName);
            var storageArgs = Settings.GetString(StorageResetArgumentsSettingName);

            RunCommand(toolPath, storageArgs);

            storageArgs = Settings.GetString(StorageStartArgumentsSettingName);

            RunCommand(toolPath, storageArgs);
        }

        protected static void ShutdownStorage()
        {
            logger.Debug("Shutting down emulated storage environment");
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
                    logger.Debug("Failed to run command: {0} {1}".Fmt(toolPath, args));

                    //ignore issue
                }
            }
        }

        protected static void KillProcesses()
        {
            try
            {
                // Kill remaining process(es)
                Process.GetProcessesByName(AzureEmulatorServiceProcessName).ToList().ForEach(p =>
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

        protected static string SubstitutePaths(string path)
        {
            var currentDirectory = TestContext.CurrentContext.TestDirectory;
            return path.Replace(TestOutputDirectorySubstitution, currentDirectory);
        }
    }
}