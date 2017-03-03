using System;
using System.Diagnostics;
using System.IO;
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
        private const string TestDeploymentConfigurationDir = @"..\..\TestContent\Cloud\bin";
        private static ILog logger;
        private static readonly IAppSettings Settings = new AppSettings();

        [OneTimeSetUp]
        public void InitializeAllTests()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            logger = LogManager.GetLogger(typeof(AzureIntegrationTestBase));
            logger.Debug("Initialization of azure storage environment for testing");
            VerifyAzureTestEnvironment();
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

        private static void VerifyAzureTestEnvironment()
        {
            logger.Debug(@"Verifying roles test environment");

            // Ensure we have a packaged Azure application
            var testDir = TestContext.CurrentContext.TestDirectory;
            var cloudConfigDir = Path.Combine(testDir, TestDeploymentConfigurationDir);
            var cloudConfigDirCsx = Path.Combine(cloudConfigDir, @"csx");

            if (!Directory.Exists(cloudConfigDirCsx))
            {
                throw new InvalidOperationException(@"The azure test environment is not configured correctly! 
The 'Webhooks.Azure.Cloud' project needs to have been packaged for the current configuration (Local|Debug), and copied (deployed) for integration testing (by the 'AfterBuild' MSBUILD Target).

Check the following: 
    (1) That the 'Webhooks.Azure.Cloud' project has been manually 'packaged' in Debug|Local mode, 
    (2) that the integration test project (Webhooks.Azure.IntTests) has been configured (by the 'AfterBuild' MSBUILD Target) to copy the packaged cloud configuration files (def, csx, rcf directories) into its own project directory (i.e. under TestContent\Cloud\bin) on every build,
    (3) that the integration test project (Webhooks.Azure.IntTests) has a build dependency (in solution settings) on the cloud project (Webhooks.Azure.Cloud).");
            }
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