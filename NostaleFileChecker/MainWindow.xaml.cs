using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Security.Cryptography;
using MahApps.Metro.Controls;
using System.Net;

/*
TODO: CHECK AND TEST THE FileList & MD5 Checked .txt Creator 
TODO2: Downloading Clean Client & Uploading The files to git :)
*/
namespace NostaleFileChecker
{
    public partial class MainWindow : MetroWindow
    {
        private string readFileMD5(string path)
        {
            FileStream a = File.OpenRead(path); //open the file with read permissions
            byte[] readed_bytes = new byte[a.Length]; //create a byte array for the file size
            a.Read(readed_bytes, 0, readed_bytes.Length); // read the file bytes
            a.Close();

            // MD5 STUFF
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] md5_bytes = md5.ComputeHash(readed_bytes);
            // MD5 DONE

            //ToString
            StringBuilder strBuilder = new StringBuilder();
            foreach (byte b in md5_bytes)
                strBuilder.Append(b.ToString("x2").ToLower()); //byte to hexa

            //Console.WriteLine("Output:" + strBuilder.ToString());

            return strBuilder.ToString();
        }
        public delegate void UpdateTextCallback(string message);
        public delegate void UpdateProgbar_Callback(int value,int maxValue);
        public delegate void UpdateDownBar_Callback(int value);
        public delegate void UpdateLogBox_Callback(string message);
        bool scanFinished = true,downloadFinished = false;
        int corruptedFiles, fixedFiles = 0;
        WebClient webClient = new WebClient();

        //TRANSLATE STUFF <----------------------------------------------------------------------------------------------------->
        static string[] translates = { "Checking Files", "Current File: ", "Wait for the scan getting finished!", "Scan Finished!", "Downloading File: ", "Corrupted Files Fixed",
        "Wir scannen die daten", "Momentan: ", "Warte bist der scan zu ende ist!", "Scan ist zu ende!", "Downloaden Date: ", "Kaputte Daten Repaired",
        "Dosyalar Taranıyor", "Şuanki Dosya: ", "Taramanın bitmesini bekle!", "Tarama sona erdi!", "Dosya indiriliyor: ", "Bozuk dosya tamir edildi"};

        static string[] button_translates = { "Check","Scan","Tara" };
        string checking_str = translates[0], checking_now_str = translates[1], wait_for_finish_str = translates[2], scan_finished_str = translates[4];
        //TRANSLATE STUFF ENDS HERE <----------------------------------------------------------------------------------------------------->
        
           
        public MainWindow()
        {
            InitializeComponent();
            webClient.DownloadProgressChanged += (s, e) =>
            {
                Dispatcher.Invoke(new UpdateDownBar_Callback(this.UpdateDownBar), new object[] { e.ProgressPercentage});
            };
            webClient.DownloadFileCompleted += (s, e) =>
            {
                downloadFinished = true;
            };
        }
        

        private void Dev_NostaleMD5_Saver(string md5, string name)
        {
            FileStream fS = new FileStream(Directory.GetCurrentDirectory() + "/GeneratedFiles/MD5/" + name + ".txt",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
            StreamWriter SW = new StreamWriter(fS); SW.Write(md5); SW.Close(); fS.Close();
        }
        private void Dev_NostaleListCreator()
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/GeneratedFiles/");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/GeneratedFiles/MD5/");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/GeneratedFiles/Files/");

            FileStream fS = new FileStream(Directory.GetCurrentDirectory() + "/GeneratedFiles/CheckList.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamWriter SW = new StreamWriter(fS);

            DirectoryInfo directory = new DirectoryInfo("E:\\NosTale\\");
            DirectoryInfo[] directories = directory.GetDirectories(); 
            FileInfo[] files = directory.GetFiles();

            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { directory.Name + "\n" });

            foreach(FileInfo f in files)
            {
                Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "Progressing File: " + f.Name });

                Dev_NostaleMD5_Saver(readFileMD5(f.FullName), f.Name);

                try { f.CopyTo(Directory.GetCurrentDirectory() + "/GeneratedFiles/Files/" + f.Name); }
                catch (Exception e0) { Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "Copy Err: " + e0.Message + "\n" }); }
                SW.WriteLine(f.Name);
                System.Threading.Thread.Sleep(1);
            }

            
            foreach (DirectoryInfo d in directories)
            {
                if (d.Name == "NostaleData") 
                {
                    Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { d.Name + "\n" });

                    files = d.GetFiles();
                    DirectoryInfo[] directories2 = d.GetDirectories();

                    foreach (FileInfo f in files)
                    {
                        Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "Progressing File: " + f.Name });

                        Dev_NostaleMD5_Saver(readFileMD5(f.FullName), f.Name);
                        try { f.CopyTo(Directory.GetCurrentDirectory() + "/GeneratedFiles/Files/" + f.Name, true); }
                        catch (Exception e1) { Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "Copy Err: " + e1.Message + "\n" }); }
                        SW.WriteLine('/' + d.Name + '/'+f.Name);      
                        System.Threading.Thread.Sleep(1);
                    }

                    foreach (DirectoryInfo dd in directories2)
                    {
                        if(dd.Name != "yedek")
                        {
                            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { dd.Name + "\n" });
                            SW.Write('/' + dd.Name + '/');
                            files = dd.GetFiles();
                            foreach (FileInfo f in files)
                            {
                                Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "Progressing File: "+f.Name});
                    
                                Dev_NostaleMD5_Saver(readFileMD5(f.FullName), f.Name);

                                try { f.CopyTo(Directory.GetCurrentDirectory() + "/GeneratedFiles/Files/" + f.Name); }
                                catch (Exception e2) { Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "Copy Err: " + e2.Message + "\n" }); }
                                SW.WriteLine('/' + d.Name + '/' + dd.Name + '/' + f.Name);   
                                System.Threading.Thread.Sleep(1);
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(1);
            }
            SW.Close();
            fS.Close();
            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "Generating Finished!" + "\n" });
        }


        private void NostaleFileChecker()
        {
            DirectoryInfo directory = new DirectoryInfo("E:\\NosTale\\");
            string web_md5 = ""; int progression = 1;

            string CheckList = webClient.DownloadString("http://127.0.0.1/checklist.txt");
            string[] files_ToCheck = CheckList.Split('\n');

            
            foreach (string s in files_ToCheck)
            {
                if (s.Length > 1)
                {
                    string tmp = s.Remove(s.Length - 1, 1);
                    FileInfo fI = new FileInfo(directory.FullName + tmp);
                    Dispatcher.Invoke(new UpdateProgbar_Callback(this.UpdateProgbar), new object[] { progression, (files_ToCheck.Length-1) });
                    Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { checking_str + "[" + progression + "/" + (files_ToCheck.Length - 1) + "] | " + checking_now_str + tmp });
                    if (fI.Exists)
                    {
                        try { web_md5 = ""; web_md5 = new WebClient().DownloadString("https://raw.githubusercontent.com/deniz617/nostaletr_checks/master/" + fI.Name + ".txt"); }
                        catch (Exception e) { }
                        if (web_md5.Length > 1)
                        {
                            if (readFileMD5(fI.FullName) != web_md5)
                            {
                                corruptedFiles++;
                                try
                                {
                                    downloadFinished = false;
                                    Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "The file '" + fI.Name + "' is corrupted! | " });
                                    Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "Downloading File: " + fI.Name });
                                    webClient.DownloadFileAsync(new Uri(("https://raw.githubusercontent.com/deniz617/nostaletr_checks/master/Files/" + fI.Name)), fI.FullName);
                                    while (downloadFinished == false)
                                        System.Threading.Thread.Sleep(1);
                                    Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { " - Done \n" });
                                    fixedFiles++;
                                }
                                catch (Exception exc1)
                                {
                                    Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { " - Error: " + exc1.Message + "\n" });
                                }
                            }
                        }
                        else
                            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "Can't get web_md5 for file: " + fI.Name + "\n" });
                    }
                    else
                    {   
                        try
                        {
                            corruptedFiles++;
                            downloadFinished = false;
                            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "The file '" + tmp + "' is missing! | " });
                            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { "Downloading File: " + tmp });
                            webClient.DownloadFileAsync(new Uri(("https://raw.githubusercontent.com/deniz617/nostaletr_checks/master/Files/" + tmp)), directory.FullName + tmp);
                            while (downloadFinished == false)
                                System.Threading.Thread.Sleep(1);
                            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { " - Done \n" });
                            fixedFiles++;
                        }
                        catch(Exception exc2)
                        {
                            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { " - Error: " + exc2.Message + "\n" });
                        }
                    }
                }
                progression++;
            }
            Dispatcher.Invoke(new UpdateLogBox_Callback(this.UpdateLogBox), new object[] { scan_finished_str + " | "+(corruptedFiles>0?("We fixed "+fixedFiles+" of "+corruptedFiles +" files."):"Your game files are wonderfull!")+"\n" });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (scanFinished)
            {
                log_Box.Text = "Started File Checker..\n";
                System.Threading.Thread checkThread = new System.Threading.Thread(NostaleFileChecker);
                checkThread.Start();
            }
            else
                MessageBox.Show(wait_for_finish_str);
        }
        private void UpdateText(string message)
        {
            curFile_label.Content = message;
        }
        private void UpdateProgbar(int value, int maxValue)
        {
            progbar.Maximum = maxValue;
            progbar.Value = value;
        }
        private void UpdateDownBar(int value)
        {
            downloadBar.Value = value;
        }
        private void UpdateLogBox(string message)
        {
            log_Box.AppendText(message);
            log_Box.ScrollToEnd();
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (langBox.SelectedIndex)
                {
                    case 0: //EN
                        checking_str = translates[0];
                        checking_now_str = translates[1];
                        wait_for_finish_str = translates[2];
                        scan_finished_str = translates[3];
                        scanButton.Content = button_translates[0];
                        UpdateLogBox("Switched Language to 'EN'\n");
                        break;
                    case 1: //DE
                        checking_str = translates[6];
                        checking_now_str = translates[7];
                        wait_for_finish_str = translates[8];
                        scan_finished_str = translates[9];
                        scanButton.Content = button_translates[1];
                        UpdateLogBox("Switched Language to 'DE'\n");
                        break;
                    case 2: //TR
                        checking_str = translates[12];
                        checking_now_str = translates[13];
                        wait_for_finish_str = translates[14];
                        scan_finished_str = translates[15];
                        scanButton.Content = button_translates[2];
                        UpdateLogBox("Switched Language to 'TR'\n");
                        break;
                }
            }
            catch(Exception ex)
            {

            }

        }

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            log_Box.Text = "Started File Creator!..\n";
            System.Threading.Thread checkThread = new System.Threading.Thread(Dev_NostaleListCreator);
            checkThread.Start();
        }
    }
}
