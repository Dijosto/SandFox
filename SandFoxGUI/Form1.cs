using System;
using System.Windows.Forms;

namespace SandFoxGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            try
            {
                var injector = new NativePayloadInjector("sbox");
                injector.Inject();
                MessageBox.Show("Injection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUnload_Click(object sender, EventArgs e)
        {
            try
            {
                var injector = new NativePayloadInjector("sbox");
                
                MessageBox.Show("NOT IMPLEMENTED: Unload successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

