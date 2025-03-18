using Micromed.ExternalCalculation.Common;
using Micromed.ExternalCalculation.Common.Dto;
using System;
using System.Diagnostics;
using System.Linq;

namespace Micromed.ExternalCalculation.MockExternalCalculation
{
    public class MockPlugin : IExternalCalculationPlugin
    {
        public Guid Guid { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Author { get; private set; }
        public string Version { get; private set; }

        private bool isRunning = false;

        private void runCommand(string exeCommand)
        {
            ProcessStartInfo cmdsi = new ProcessStartInfo(exeCommand);

            cmdsi.Arguments = "";

            Process cmd = Process.Start(cmdsi);
            cmd.WaitForExit();
        }

        public MockPlugin()
        {
            Guid = Guid.Parse("1BC8E16D-3C09-40BE-8EC2-F9D7E4F0111C");
            Name = "HFO Engine Calculation";
            Description = "HFO Engine External Calculation plugin";
            Author = "Micromed";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fvi.FileVersion;
        }


        public int Start(PluginParametersDto pluginParameters)
        {
            if (isRunning)
                return 1; // Already running
            // Get the TRC file path from Micromed's plugin parameters
            string trcFilePath = pluginParameters.TraceFilePathList?.FirstOrDefault();
            if (string.IsNullOrEmpty(trcFilePath))
            {
                OnError("No TRC file detected in Micromed Brain Quick.");
                return -1;
            }
            // Specify the full path to HFO_ENGINE executable
            string hfoEnginePath = @"C:\Program Files\Micromed\BrainQuickEEG\Plugins\hfo_engine\HFO_ENGINE.exe";

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = hfoEnginePath,
                    Arguments = "", // Optional: Add command-line arguments if needed
                    WindowStyle = ProcessWindowStyle.Normal // Optional: Set window style (Normal, Hidden, etc.)
                };

                // Start the HFO_ENGINE executable
                Process cmd = Process.Start(startInfo);
                cmd.WaitForExit(); // Optional: Wait for the executable to exit before continuing, if necessary.

                isRunning = true;
                OnProgress(100); // Update progress after execution
            }
            catch (Exception ex)
            {
                // Handle any errors that occur when launching the executable
                OnError($"Error opening HFO_ENGINE executable: {ex.Message}");
                return -1; // Indicating an error
            }

            return 0;
        }


        public bool Stop()
        {
            if (!isRunning)
                return false;

            isRunning = false;

            OnCancelled();
            return true;
        }


        public event EventHandler Completed;
        protected void OnCompleted()
        {
            EventHandler handler = Completed;
            if (handler != null) handler(this, null);
        }

        public event EventHandler Cancelled;
        protected void OnCancelled()
        {
            EventHandler handler = Cancelled;
            if (handler != null) handler(this, null);
        }

        public event EventHandler<string> Error;
        protected void OnError(string e)
        {
            EventHandler<string> handler = Error;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<int> Progress;
        protected void OnProgress(int e)
        {
            EventHandler<int> handler = Progress;
            if (handler != null) handler(this, e);
        }


        public bool NeedTraceFilePathList
        {
            get { return false; }
        }

        public bool NeedExchangeTraceFilePathList
        {
            get { return true; }
        }

        public bool NeedExchangeEventFilePath
        {
            get { return false; }
        }

        public bool NeedExchangeReportFilePath
        {
            get { return false; }
        }

        public bool NeedExchangeTrendFilePathList
        {
            get { return false; }
        }

        public bool NeedFilteredData
        {
            get { return true; }
        }

        public bool DerivationOptionEnabled
        {
            get { return false; }
        }

        public bool TraceSelectionOptionEnabled
        {
            get { return false; }
        }
    }
}
