using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SandFoxGUI
{
    public partial class Form1 : Form
    {
        private const string TARGET_PROCESS = "sbox";
        private const string MANAGED_PAYLOAD_FOLDER = "ManagedPayload";

        public Form1()
        {
            InitializeComponent();
        }

        private void OnInjectButtonClick(object sender, EventArgs e)
        {
            try
            {
                var managedPayloadPath = Path.Combine(Environment.CurrentDirectory, MANAGED_PAYLOAD_FOLDER);
                EnsureManagedPayloadCopied(managedPayloadPath);

                var targetProcess = NativeInjector.GetProcessByName(TARGET_PROCESS);
                var processDirectory = Path.GetDirectoryName(targetProcess.MainModule?.FileName
                    ?? throw new InvalidOperationException("Could not get process main module"));

                var bootstrapperDllPath = Path.Combine(Environment.CurrentDirectory, "NativePayload.dll");
                NativeInjector.InjectDLL(targetProcess.Id, bootstrapperDllPath);

                MessageBox.Show("Injection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnUnloadButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show("Unload functionality not implemented yet.", "Information",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void EnsureManagedPayloadCopied(string sourcePath)
        {
            var targetProcess = NativeInjector.GetProcessByName(TARGET_PROCESS);
            if (targetProcess.MainModule?.FileName is null)
                throw new InvalidOperationException("Could not get process main module");

            var processDirectory = Path.GetDirectoryName(targetProcess.MainModule.FileName);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                var fileName = Path.GetFileName(file);
                var destinationPath = Path.Combine(processDirectory!, fileName);
                File.Copy(file, destinationPath, true);
            }
        }
    }
}