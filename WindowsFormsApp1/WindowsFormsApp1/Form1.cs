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
using System.Xml;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        int count_streamer = 0;
        Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();
        public Form1()
        {
            InitializeComponent();
            textBox4_settext();
             
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult.Equals(DialogResult.OK))
            {
                textBox2.Text = folderBrowserDialog.SelectedPath;
                textBox2.Text = textBox2.Text + @"\";

            }
        }




        private void button2_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFileDialog1.FileName;

            }

        }



        private void textBox2_Validating(object sender, CancelEventArgs e1)
        {
            if (textBox2.Text == string.Empty)
            {
                e1.Cancel = true;
                textBox2.Focus();
                this.errorProvider2.SetError(textBox2, "Please enter directory");
            }
            else if (!Directory.Exists(textBox2.Text))
            {
                e1.Cancel = true;
                textBox2.Focus();
                this.errorProvider2.SetError(textBox2, "Directory does not exist");
            }
            else
            {
                if (!textBox2.Text.EndsWith(@"\"))
                {
                    textBox2.Text = textBox2.Text + @"\";
                }
                e1.Cancel = false;
                this.errorProvider2.SetError(textBox2, "");
                this.errorProvider2.Clear();

            }

        }

        private void textBox3_Validating(object sender, CancelEventArgs e2)
        {
            if (string.IsNullOrWhiteSpace(this.textBox3.Text))
            {
                e2.Cancel = true;
                textBox3.Focus();
                this.errorProvider1.SetError(textBox3, "please enter file name");

            }
            else if (!File.Exists(textBox3.Text))
            {
                e2.Cancel = true;
                textBox3.Focus();
                this.errorProvider1.SetError(textBox3, "File does not exist");
            }
            else
            {
                e2.Cancel = false;
                this.errorProvider1.SetError(textBox3, "");
                this.errorProvider1.Clear();

            }

        }
        protected void textBox4_settext()
        {
            textBox4.Text = "192.168.0.0";
            textBox4.ForeColor = Color.Gray;

        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            if (textBox4.ForeColor == Color.Gray)
            {
                textBox4.Text = "";
            }
            textBox4.ForeColor = Color.Black;

        }

     

        private bool IsValidIp(string ip)
        {
            try
            {
                if (ip == null || ip.Length == 0)
                {
                    return false;

                }

                string[] address = ip.Split(new[] { "." }, StringSplitOptions.None);
                if (address.Length != 4)
                {
                    return false;
                }

                foreach (string i in address)
                {
                    int part = Int32.Parse(i);
                    if (part < 0 || part > 255)
                    {
                        return false;
                    }
                }
                if (ip.EndsWith("."))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private void textBox4_Validating(object sender, CancelEventArgs e)
        {
            bool ip = IsValidIp(textBox4.Text);
            if (textBox4.Text == "")
            {
                e.Cancel = true;
                textBox4_settext();
                errorProvider3.SetError(textBox4, "Enter Ip Adress");
            }
            else if (ip == false)
            {
                e.Cancel = true;
                errorProvider3.SetError(textBox4, "please provide valid ip adress");
            }
            else
            {
                e.Cancel = false;
                errorProvider3.SetError(textBox4, "");
                errorProvider3.Clear();
            }

        }

        private void textBox5_Validating(object sender, CancelEventArgs e)
        {
            try
            {

                if (textBox5.Text == string.Empty)
                {
                    e.Cancel = true;
                    errorProvider4.SetError(textBox5, "enter port");

                }
                else
                {
                    if (!Int32.TryParse(textBox5.Text, out int i))
                    {
                        e.Cancel = true;
                        errorProvider4.SetError(textBox5, "enter valid port number");
                    }

                    UInt32 port = UInt32.Parse(textBox5.Text);
                    if (port < 1 || port > 65535)
                    {
                        e.Cancel = true;
                        errorProvider4.SetError(textBox5, "Enter valid port");
                    }
                    else
                    {
                        e.Cancel = false;
                        errorProvider4.SetError(textBox5, "");
                        errorProvider4.Clear();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                // Set focus on control
                control.Focus();
                // Validate causes the control's Validating event to be fired,
                // if CausesValidation is True
                if (!Validate())
                {
                    DialogResult = DialogResult.None;
                    return;
                }
            }
            //copy files
            Copy_Files();
            //Read config xml file
            Read_Xml();
            try
            {
                Create_BatchFiles("VODmanager.bat");
                Create_BatchFiles("LCN_n.bat");
                Create_BatchFiles("execVODManager_n.bat");
                Create_BatchFiles("stopVODManager.bat");
                DialogResult result = MessageBox.Show("Installation Done", "Alert", MessageBoxButtons.OK);
                if (result == DialogResult.OK)
                {
                    Application.Exit();
                }
            }
            catch(Exception ec)
            {
                MessageBox.Show(ec.Message,"Error", MessageBoxButtons.OK);
                Application.Exit();
            }
        }
        public void Copy_Files()
        {
            string path = Directory.GetCurrentDirectory();
            string target = path + "\\file_list.xml";
            if (File.Exists(target))
            {
                XmlReader xmlReader = XmlReader.Create(@target);

                string file = "";
                string source = "";
                int flag = 0;
                int fcount = 0;
                while (xmlReader.Read())
                {
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.Name.ToString())
                        {
                            case "File_Name":
                                path = Directory.GetCurrentDirectory();
                                file = xmlReader.ReadString();
                                source = path + "\\" + file;
                                string destination = textBox2.Text;
                                destination = destination + "\\" + file;
                                try
                                {
                                    if (File.Exists(destination))
                                    {
                                        File.Delete(destination);
                                    }
                                    File.Copy(source, destination);
                                    flag = 1;

                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message,"Error", MessageBoxButtons.OK);
                                    flag = 2;
                                    Application.Exit();
                                }
                                fcount++;
                                break;
                        }


                    }
                }
                if (flag == 1)
                {
                    MessageBox.Show(fcount+" Files Copied","Alert", MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("file_list.xml missing !!!" ,"Error", MessageBoxButtons.OK);
                Application.Exit();

            }

        }

    
        public void Read_Xml()
        {
           
            XmlDocument doc = new XmlDocument();
            doc.Load(textBox3.Text);
            XmlNodeList xnList = doc.SelectNodes("/config/streamer");
           
            foreach (XmlNode xn in xnList)
            {

                if (xn["IP"] != null && xn["IP"].HasChildNodes)
                {
                    List<string> values = new List<string>();
                    if(xn["IP"]["address"] != null)
                    values.Add(xn["IP"]["address"].InnerText);
                    if (xn["IP"]["port"] != null)
                        values.Add(xn["IP"]["port"].InnerText);
                    map.Add(xn["id"].InnerText, values);
                    
                   
                }else if (xn["RF"] != null && xn["RF"].HasChildNodes)
                {
                    List<string> values = new List<string>();
                    if (xn["RF"]["uaddress"] != null)
                        values.Add(xn["RF"]["uaddress"].InnerText);
                    if (xn["RF"]["oport"] != null)
                        values.Add(xn["RF"]["oport"].InnerText);
                    map.Add(xn["id"].InnerText, values);
                }
            }


            foreach (KeyValuePair<string, List<string>> kvp in map)
            {
               
                if(kvp.Value.Count == 2)                  
                count_streamer++;

            }


        }

        public void Create_BatchFiles(string file)
        {
           
            try
            {
                if (File.Exists(textBox2.Text + @"\" + file))
                {
                    File.Delete(textBox2.Text + @"\" + file);
                }
                switch (file)
                {
                    case "VODmanager.bat":
                        StreamWriter sw = new StreamWriter(textBox2.Text + @"\VODmanager.bat");
                        sw.Write("REM VoDManager Version2.0 \r\n start \"VoDManager(2.02)\"");
                        sw.Write(textBox2.Text + "\\VoDManager.exe -a " + textBox4.Text + " -p " + textBox5.Text + " -c " + textBox6.Text + " -i " + textBox3.Text);
                        sw.Close();
                        break;
                    case "execVODManager_n.bat":
                        StreamWriter sw1 = new StreamWriter(textBox2.Text + @"\execVODManager_"+count_streamer+".bat");
                        sw1.Write("REM execVoDManager_"+count_streamer+" Version2.1 \r\n") ;
                        sw1.Write("start cmd.exe /c" + textBox2.Text + "\\VoDManager.bat \r\n timeout1 \r\n");
                      
                        foreach (KeyValuePair<string, List<string>> kvp in map)
                        {

                            if (kvp.Value.Count == 2)
                            {
                                sw1.Write("start cmd.exe /c" + textBox2.Text + @"\Multicastserver-"+kvp.Key+".bat \r\n timeout1 \r\n");
                            }
                        }
                        sw1.Close();
                        break;
                    case "LCN_n.bat":
                       
                        foreach (KeyValuePair<string, List<string>> kvp in map)
                        {

                            if (kvp.Value.Count == 2)
                            {                               
                                StreamWriter sw2 = new StreamWriter(textBox2.Text + @"\LCN_" + kvp.Key + ".bat");
                                sw2.Write("REM Multicastserver  Version 2.02\r\n");
                                sw2.Write("start \"Multicastserver(2.02) -"+kvp.Key+ "\" " + textBox2.Text + @"\multicastserver.exe  -m 0 -v " + textBox4.Text + " -p " + textBox5.Text + " -u ");
                                sw2.Write(kvp.Value[0] + "-o" + kvp.Value[1] + "-s" + kvp.Key+"-c ");
                                sw2.Write(textBox6.Text + "-n 7 -b 0 -l 0");
                                sw2.Close();
                            }
                        }
                        break;

                    case "stopVODManager.bat":
                    StreamWriter sw3 = new StreamWriter(textBox2.Text + @"\stopVODManager.bat");
                    sw3.Write("@echo off \r\n  taskkill /f /im VoDManager.exe \r\n taskkill /f /im multicastserver.exe \r\n :exit");
                        sw3.Close();
                     break;
                }

            }
            catch (Exception e)
            { 
                MessageBox.Show("unable to create batch file","Error", MessageBoxButtons.OK);
                Application.Exit();
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog3.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox6.Text = folderBrowserDialog3.SelectedPath;
                textBox6.Text = textBox6.Text + @"\";
            }

        }

        private void textBox6_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (textBox6.Text == String.Empty)
                {
                    e.Cancel = true;
                    textBox6.Focus();
                    errorProvider5.SetError(textBox6, "provide content path");
                }
                else if (!Directory.Exists(textBox6.Text))
                {
                    e.Cancel = true;
                    textBox6.Focus();
                    errorProvider5.SetError(textBox6, "not valid directory");
                }
                else
                {
                    if (!textBox6.Text.EndsWith(@"\"))
                    {
                        textBox6.Text = textBox6.Text + @"\";
                    }
                    e.Cancel = false;
                    errorProvider5.SetError(textBox6, "");
                    errorProvider5.Clear();

                }

            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message,"Error",MessageBoxButtons.OK);
                Application.Exit();
            }
        }

       
    }
}

