using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;
namespace iteadTool
{
    public partial class Form1 : Form
    {
        enum eFileType
        {
            eCopTop,
            eCopBot,
            eSolTop,
            eSolBot,
            eSilTop,
            eSilBot,
            eDrill,
            eOutline
        }
        public void loadDir()
        {
            RegistryKey reg_key =
            Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey sub_key = reg_key.CreateSubKey("IteadTool");
            textBox1.Text = (string) sub_key.GetValue("CurrentDirectory", Directory.GetCurrentDirectory());
        }
        public void saveDir()
        {
            RegistryKey reg_key =
            Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey sub_key = reg_key.CreateSubKey("IteadTool");
            sub_key.SetValue("CurrentDirectory", textBox1.Text);
        }
        public Form1()
        {
            InitializeComponent();
            loadDir();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.SelectedPath = textBox1.Text;
            fbd.ShowDialog();
            textBox1.Text = fbd.SelectedPath;
        }
        
        string m_strProjectName;
        private void button2_Click(object sender, EventArgs e)
        {
            saveDir();
            string[] strFiles;
            try
            {

                strFiles = Directory.GetFiles(textBox1.Text, "*.*");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Directory open failed");
                return;
            }
            var strWantedFiles = strFiles.Where(s => s.EndsWith(".gbr") || s.EndsWith(".drl"));
            checkedListBox1.Enabled = false;
            foreach( int i in checkedListBox1.CheckedIndices)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
            
            string strNameFetch = Path.GetFileName(strWantedFiles.First());
            string strProjectName = strNameFetch.Substring(0, strNameFetch.LastIndexOf(" - "));

            m_strProjectName = strProjectName;
            button3.Enabled = true;
            button3.Text = "Prepare " + m_strProjectName;
            foreach (string strFileName in strWantedFiles)
            {
                if (strFileName.Contains("Bottom Copper.gbr"))
                {
                    checkedListBox1.SetItemChecked((int)eFileType.eCopBot, true);
                }
                if (strFileName.Contains("Top Copper.gbr"))
                {
                    checkedListBox1.SetItemChecked((int)eFileType.eCopTop, true);
                }
                if (strFileName.Contains("Bottom Copper (Resist).gbr"))
                {
                    checkedListBox1.SetItemChecked((int)eFileType.eSolBot, true);
                }
                if (strFileName.Contains("Top Copper (Resist).gbr"))
                {
                    checkedListBox1.SetItemChecked((int)eFileType.eSolTop, true);
                }
                if (strFileName.Contains("Top Silkscreen.gbr"))
                {
                    checkedListBox1.SetItemChecked((int)eFileType.eSilTop, true);
                }
                if (strFileName.Contains("Bottom Silkscreen.gbr"))
                {
                    checkedListBox1.SetItemChecked((int)eFileType.eSilBot, true);
                }
                if (strFileName.Contains("[Through Hole].drl"))
                {
                    checkedListBox1.SetItemChecked((int)eFileType.eDrill, true);
                }
                if (strFileName.Contains("Board Outline"))
                {
                    checkedListBox1.SetItemChecked((int)eFileType.eOutline, true);
                }
            }
            checkedListBox1.Enabled = true;
        }
        bool CopyAttempt(string strOld, string strNew)
        {
            try
            {
                File.Copy(strOld, strNew);
                return true;
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string strNewDirectory = Path.Combine(textBox1.Text, m_strProjectName);
            string strOldDirectory = textBox1.Text;
            try
            { Directory.Delete(strNewDirectory, true); }
            catch (Exception ex) { };
            Directory.CreateDirectory(strNewDirectory);

            string strResult = "";
            {
                string strOldPath = Path.Combine(strOldDirectory, m_strProjectName + " - Top Copper.gbr");
                string strNewPath = Path.Combine(strNewDirectory, m_strProjectName + ".GTL");
                if (!CopyAttempt(strOldPath, strNewPath))
                    strResult += "ERROR: NO TOP COPPER\n\r";
            }
            {
                string strOldPath = Path.Combine(strOldDirectory, m_strProjectName + " - Bottom Copper.gbr");
                string strNewPath = Path.Combine(strNewDirectory, m_strProjectName + ".GBL");
                if (!CopyAttempt(strOldPath, strNewPath))
                    strResult += "ERROR: NO BOTTOM COPPER\n\r";
            }
            {
                string strOldPath = Path.Combine(strOldDirectory, m_strProjectName + " - Top Copper (Resist).gbr");
                string strNewPath = Path.Combine(strNewDirectory, m_strProjectName + ".GTS");
                if (!CopyAttempt(strOldPath, strNewPath))
                    strResult += "ERROR: No top resist\n\r";
            }
            {
                string strOldPath = Path.Combine(strOldDirectory, m_strProjectName + " - Bottom Copper (Resist).gbr");
                string strNewPath = Path.Combine(strNewDirectory, m_strProjectName + ".GBS");
                if (!CopyAttempt(strOldPath, strNewPath))
                    strResult += "ERROR: No bottom resist\n\r";
            }
            {
                string strOldPath = Path.Combine(strOldDirectory, m_strProjectName + " - Top Silkscreen.gbr");
                string strNewPath = Path.Combine(strNewDirectory, m_strProjectName + ".GTO");
                if (!CopyAttempt(strOldPath, strNewPath))
                    strResult += "WARNING: No top silkscreen\n\r";
            }
            {
                string strOldPath = Path.Combine(strOldDirectory, m_strProjectName + " - Bottom Silkscreen.gbr");
                string strNewPath = Path.Combine(strNewDirectory, m_strProjectName + ".GBO");
                if (!CopyAttempt(strOldPath, strNewPath))
                    strResult += "WARNING: No bottom silkscreen\n\r";
            }
            {
                string strOldPath = Path.Combine(strOldDirectory, m_strProjectName + " - Drill Data - [Through Hole].drl");
                string strNewPath = Path.Combine(strNewDirectory, m_strProjectName + ".DRL");
                if (!CopyAttempt(strOldPath, strNewPath))
                    strResult += "ERROR: NO DRILL DATA\n\r";
            }
            {
                string strOldPath = Path.Combine(strOldDirectory, m_strProjectName + " - Board Outline.gbr");
                string strNewPath = Path.Combine(strNewDirectory, m_strProjectName + ".GKO");
                if (!CopyAttempt(strOldPath, strNewPath))
                    strResult += "WARNING: No outline file\n\r";
            }
            string strZipName = Path.Combine(strOldDirectory, m_strProjectName + ".zip");
            File.Delete(strZipName);
            if (strResult.Contains("ERROR"))
                strResult += "COMPRESSION SKIPPED DUE TO ERROR";
            else
            {
                try
                {
                    ZipFile.CreateFromDirectory(strNewDirectory, strZipName, CompressionLevel.Optimal, false);
                }
                catch (Exception ex)
                {
                    strResult += "Zip error: " + ex.ToString();
                }

            }
            MessageBox.Show("Attempt complete results below:\n\r" + strResult);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = false;
            checkedListBox1.Enabled = false;

            foreach (int i in checkedListBox1.CheckedIndices)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
            checkedListBox1.Enabled = true;
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if(checkedListBox1.Enabled)
                e.NewValue = e.CurrentValue;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
