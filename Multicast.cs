using System;
using GeniePlugin.Interfaces;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Standalone_Multicast
{
    public class Multicast : IPlugin
    {
        // Constant variable for the Properties of the plugin
        // At the top for easy changes.
        string _NAME = "Multicast";
        string _VERSION = "0.1";
        string _AUTHOR = "digitalnyc1";
        string _DESCRIPTION = "Send/receive multicast messages between Genie client instances running on the same network.";

        public IHost _host;                             //Required for plugin
        public System.Windows.Forms.Form _parent;       //Required for plugin

        bool _debug = false;

        private bool _enabled = true;

        private Guid _multicastGuid = Guid.NewGuid();

        private UdpClient _udpClient;
        private IPAddress _multicastAddress;
        private IPEndPoint _multicastEndpoint;
        #region IPlugin Properties

        //Required for Plugin - Called when Genie needs the name of the plugin (On menu)
        //Return Value:
        //              string: Text that is the name of the Plugin
        public string Name
        {
            get { return _NAME; }
        }

        //Required for Plugin - Called when Genie needs the plugin version (error text
        //                      or the plugins window)
        //Return Value:
        //              string: Text that is the version of the plugin
        public string Version
        {
            get { return _VERSION; }
        }

        //Required for Plugin - Called when Genie needs the plugin Author (plugins window)
        //Return Value:
        //              string: Text that is the Author of the plugin
        public string Author
        {
            get { return _AUTHOR; }
        }

        //Required for Plugin - Called when Genie needs the plugin Description (plugins window)
        //Return Value:
        //              string: Text that is the description of the plugin
        //                      This can only be up to 200 Characters long, else it will appear
        //                      "truncated"
        public string Description
        {
            get { return _DESCRIPTION; }
        }

        //Required for Plugin - Called when Genie needs disable/enable the plugin (Plugins window,
        //                      or when Gneie needs to know the status of the plugin (???)
        //Get:
        //      Not Known what it is used for
        //Set:
        //      Used by Plugins Window 
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }

        }

        #endregion
        #region IPlugin Methods
        // Required for Plugin - Called on first load
        // Parameters:
        //     IHost Host:  The host (instance of Genie) making the call
        public void Initialize(IHost Host)
        {
            // Set Decimal Seperator to a period (.) if not set that way
            if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            }

            // Set _host variable to the Instance of Genie that started the plugin (so can call host API commands)
            _host = Host;

            // Set Genie Variables if not already set
            //if (_host.get_Variable("Multicast.Debug") == "")
            //    _host.SendText("#var Multicast.Debug 0");

            // Initialize multicast
            InitializeMulticast();
        }

        // Required for Plugin - Called when user enters text in the command box
        // Parameters:
        //     string Text:  The text the user entered in the command box
        // Return Value:
        //     string: Text that will be sent to the game
        public string ParseInput(string Text)
        {
            if (Text.StartsWith("/multicast "))
            {
                // Cleam up leading/trailing spaces and remove command name
                Text = Text.Trim().Replace("/multicast ", "");

                /*
                // Check for proper syntax
                Regex msg = new Regex(" ");
                int space = msg.Matches(Text).Count;
                if (space > 2)
                {
                    DisplaySyntax();
                    return "";
                }
                */

                // Add this client's GUID to beginning of message
                Text = _multicastGuid.ToString() + ":" + Text;

                // Send multicast message
                byte[] _textBytes = Encoding.UTF8.GetBytes(Text);
                _udpClient.Send(_textBytes, Text.Length, _multicastEndpoint);
                DebugOutput("Multicast: Sent: " + Text);
                return "";
            }
            else if (Text == "/multicast")
            {
                DisplaySyntax();
                return "";
            }

            return Text;
        }

        // Required for Plugin - 
        // Parameters:
        //     string Text:  That DIRECT text comes from the game (non-"xml")
        //Return Value:
        //     string: Text that will be sent to the to the windows as if from the game
        public string ParseText(string Text, string Window)
        {
            try
            {
                if (_host != null)
                {
                    /*
                    if (_calculating == true && Text.StartsWith("Name: ") && Text.Contains("Guild: "))
                    {
                        _calcGuildName = Text.Substring(Text.IndexOf("Guild: ") + 7).Trim();
                        if (!GetGuild(_calcGuildName))
                        {
                            DisplaySyntax();
                            _calculating = false;
                            return Text;
                        }
                        _host.SendText("exp 0");
                    }
                    */
                }
            }
            catch
            {
            }
            return Text;
        }

        // Required for Plugin - 
        // Parameters:
        //     string Text:  That "xml" text comes from the game
        public void ParseXML(string XML)
        {
        }

        // Required for Plugin - Opens the settings window for the plugin
        public void Show()
        {
            OpenSettingsWindow(_host.ParentForm);
        }

        public void VariableChanged(string Variable)
        {

        }

        public void ParentClosing()
        {
        }

        public void OpenSettingsWindow(System.Windows.Forms.Form parent)
        {
            frmMulticast form = new frmMulticast(ref _host);

            //if (_host.get_Variable("Multicast.Debug") == "1")
            //    form.debug.Text = "Bottom";
            //else
            //    form.debug.Text = "Top";

            if (parent != null)
               form.MdiParent = parent;

            form.Show();
        }

        #endregion

        #region Custom Parse/Display methods
        /// <summary>
        /// Callback which is called when UDP packet is received
        /// </summary>
        /// <param name="ar"></param>
        private void ReceivedCallback(IAsyncResult ar)
        {
            // Get received data
            IPEndPoint _sender = new IPEndPoint(0, 0);
            Byte[] _receivedBytes = _udpClient.EndReceive(ar, ref _sender);
            if (_receivedBytes.Length > 0)
            {
                String _receivedText = System.Text.Encoding.UTF8.GetString(_receivedBytes, 0, _receivedBytes.Length);
                DebugOutput("Multicast: Received: " + _receivedText);
                if (!_receivedText.StartsWith(_multicastGuid.ToString()))
                {
                    // Strip the sender's GUID from the beginning of the message
                    _receivedText = Regex.Replace(_receivedText, @"^[0-9a-fA-F]{8}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{12}:", "");
                    SendParse(_receivedText); 
                }
            }
           
            // Restart listening for udp data packages
            _udpClient.BeginReceive(new AsyncCallback(ReceivedCallback), null);
        }

        private void InitializeMulticast()
        {
            /*
             NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
             foreach (NetworkInterface adapter in nics)
             {
                 IPInterfaceProperties ip_properties = adapter.GetIPProperties();
                 if (!adapter.GetIPProperties().MulticastAddresses.Any())
                     continue; // most of VPN adapters will be skipped
                 if (!adapter.SupportsMulticast)
                     continue; // multicast is meaningless for this type of connection
                 if (OperationalStatus.Up != adapter.OperationalStatus)
                     continue; // this adapter is off or not connected
                 IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
                 if (null == p)
                     continue; // IPv4 is not configured on this adapter
                 _udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)IPAddress.HostToNetworkOrder(p.Index));
             }
             */
            _udpClient = new UdpClient();
            _multicastAddress = IPAddress.Parse("224.0.0.1");
            _multicastEndpoint = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 12345);

            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 12345));
            _udpClient.JoinMulticastGroup(_multicastAddress);
            _udpClient.MulticastLoopback = true;
            DebugOutput("Multicast: Joined multicast group!");
            _udpClient.BeginReceive(new AsyncCallback(ReceivedCallback), null);
            DebugOutput("Multicast: Begin receiving!");
        }

        private void DisplaySyntax()
        {
            SendOutput("");
            SendOutput("Multicast (Ver:" + _VERSION + ") Usage:");
            SendOutput("/multicast [message]");
        }

        private void DebugOutput(string output)
        {
            if (!_debug) return;
            _host.SendText("#echo red \"DEBUG: " + output + "\"");
        }

        private void SendOutput(string output)
        {
            _host.SendText("#echo " + output);
        }

        private void SendParse(string output)
        {
            if (_debug) _host.SendText("#echo red \"DEBUG: #parse multicast " + output + "\"");
            _host.SendText("#parse multicast " + output);
        }

        #endregion
    }
}