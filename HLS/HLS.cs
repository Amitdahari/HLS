using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.IO.IsolatedStorage;
using System.Threading;

namespace HierarchicalLocation
{
    public partial class HLS : Form
    {
        static string port;
        static string father_port;
        static string this_ip;
        static string father_ip;
        static DataTable dtLocalInfo;
    
        
        

        /// <summary>
        /// Whether the instance is a root or has a parent
        /// </summary>
        public bool IsRoot
        {
            get { return cbRoot.Checked; }
            set
            {
                cbRoot.Checked = value;
            }
        }

        public HLS()
        {
            InitializeComponent();
            initiliazeDataTable();
        }

        /// <summary>
        /// Automatically fills in the local IP address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bAuto_Click(object sender, EventArgs e)
        {
            // try to find a local IPv4 address
            IPAddress[] localIP = Dns.GetHostAddresses("");
            foreach (IPAddress add in localIP)
            {
                if (add.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(add))
                {
                    this.tbIP.Text = add.ToString();
                    return;
                }
            }
            // if we get here there are no IPv4 addresses, just use the localhost one
            tbIP.Text = "127.0.0.1";
            return;
        }


        private void initiliazeDataTable() 
        {
            dtLocalInfo = new DataTable();
            dtLocalInfo.Columns.Add(new DataColumn("file_name"));
            dtLocalInfo.Columns.Add(new DataColumn("ip_target"));
            dgvLocal.DataSource = dtLocalInfo;
          

        }

        private void bListen_Click(object sender, EventArgs e)
        {
            port = nudMyPort.Text;
            father_port = nudFatherPort.Text;
            father_ip = tbFatherIP.Text;
            this_ip = tbIP.Text;
            Thread listenerTCPThread = new Thread(new ThreadStart(listenerTCP));
            listenerTCPThread.Start();
            //HERE LISTENER

        }
        
        private void listenerTCP() 
        {
            TcpListener server = null;
            Int32 node_port = Int32.Parse(port);           
            IPAddress localAddr = IPAddress.Parse(this_ip);

            try
            {
                server = new TcpListener(localAddr, node_port);
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Thread client_thread = new Thread(new ParameterizedThreadStart(clientListener));
                    client_thread.Start(client);
                    client_thread.Join();
                }

            }
            catch (SocketException e) 
            { 
            
            }
        }

        private void clientListener(object obj)
        {
            TcpClient client = (TcpClient)obj;
            Byte[] bytes = new Byte[256];
            String data = null;
            NetworkStream stream = client.GetStream();
            int i;

            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                //we check if the file exists in our datatable
                DataRow[] fileRows = dtLocalInfo.Select("file_name");
                string respons="";
                if(fileRows.Length != 0 )
                {
                    for (int j = 0; j < fileRows.Length; j++) 
                    {
                        if (fileRows[j].ItemArray[0].ToString().Equals(data)) 
                        {
                            if (fileRows[j].ItemArray[1].Equals(this_ip + ":" + port))
                            {
                                respons = fileRows[j].ItemArray[0].ToString();
                                break;

                            }
                        }
                    }
                    if (respons.Equals(""))
                    {
                        respons = father_ip + ":" + father_port;
                    
                    }
                }

                // Process the data sent by the client.
                data = respons;

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);

                client.Close();
            }
        }

        private void senderTCP() 
        {
        
        }

        private void bAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbAdd.Text))
            {
                dtLocalInfo.Rows.Add(new Object[] { "", "ip" });
            }
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;
            var fileName = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    
                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }

            
                string results  =  "";
                results = Path.GetFileName(filePath);
                

                dtLocalInfo.Rows.Add(new object[] { results, "test" });
            }
        }
    }
       
}
