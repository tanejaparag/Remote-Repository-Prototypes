////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Has definition for all main window events        //
// ver 1.0                                                                //
// Language:    C#, Visual Studio 2017                                  //
// Parag Taneja, CSE687 - Object Oriented Design, Spring 2018          //
///////////////////////////////////////////////////////////////////////////


/*
Package Operations:
* -------------------
* This package provides 2 class:
*
- Window : where all the controls are drawn.
- FileDetails : It has fields to get and set Lst Item used in Check In Tab


Event Functions :
private void browse_Click(object sender, RoutedEventArgs e) - Create message and send it to Server to fetch all the files which belongs to specific category
private void browse_ListBox_doubleClick(object sender, RoutedEventArgs e) - Opens client file into new Pop Up with all its content.
private void movingbtncode(string param1,string param2,string param3) - Moved code of checkin_AddFolder_Click to this functipon to handle folder creation during automation 
private void checkin_AddFolder_Click(object sender, RoutedEventArgs e) - Calls movingbtncode with default values on click of Add Folder button
private void checkin_AddFile_Click(object sender, RoutedEventArgs e) - Open File Dialog window which helps users to navigate to the file and selcted file is added to File List Box in checkIn Tab
private void checkin_Checkin_Click(object sender, RoutedEventArgs e) - Creates message with file attribute and send the message to Client senders queue which will will inturn send the file to server in form of bytes.
private void checkOut_Open_doubleClick(object sender, RoutedEventArgs e) - open client file into new pop up
private void checkout_Checkout_Click(object sender, RoutedEventArgs e) - Creates a message and send it to server by providing filename and namespace , server will inturn sends the file to client through handler mentioned in serverprototype.cpp
private void filecontent_Open_doubleClick(object sender, RoutedEventArgs e) - Creates a message and send it to server for the file is selected in List View , once the file is downloaded it opens in new pop up which is handled by dispacther for clientaskingfile
private void metadata_Open_doubleClick(object sender, RoutedEventArgs e) - Creates a message and send a message to server , once the server replies reach to client reply_metadata delgate is ivokded which open metadata in new pop up
private void connect_Click(object sender, RoutedEventArgs e) - Cretaes a connection message as per user input and sends it to server , with a server reply connection is successfully established
private void filecontent_DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e) - File List and directory List should chnage in File Content tab when any folder is selected.
private void metadata_DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e) - File List and directory List should chnage in Metadata tab when any folder is selected.
private void DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e) - espond to mouse double-click on dir name in check out tab
private void checkin_DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e) - 	File List and directory List should chnage in CheckIn tab when any folder is selected.
private void window_Load(object sender, RoutedEventArgs e) - Creates default setting and excutes automation Test Cases for 2 clients	
private void Click_Diconnect(object sender, RoutedEventArgs e) - Create and send a message about diconnection to Server.
private async void myWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) - Creates and send a diconnection message to server and close the client window.


Private Functions :
private void processMessages() - It deque the message from receviing queue and invokes the dispacther for the command field in that message.
private void addClientProc() - Adding the delegate in dispacther with command name as key
private void clearDirs(string tabname) - Clear the directory List Box in specific tab
private void addDir(string dir, string tabname) - Add item in Directory Listbox with folder name in specific tab
private void insertParent(string tabname) - Always add a item (..) in Directory Listbox which helps users in navigating back in specific tab
private void clearFiles(string tabname) - Clear the File list box in specific tab
private void addFile(string file,string tabname) - Add File name as list item in File List Box in specific tab.
private void DispatcherLoadGetDirs() - load getDirs processing into dispatcher dictionar
private void DispatcherLoadGetFiles() - load getFiles processing into dispatcher dictionary
private void connectServer() - load reply_connect processing into dispatcher dictionary
private void threadQuit() - load reply_threadquit processing into dispatcher dictionary
private void metadatahandling() - load reply_metadata processing into dispatcher dictionary
private void addFolder() - load reply_foldername processing into dispatcher dictionary
private void clientfileaskingreply() - load clientfileaskingreply processing into dispatcher dictionary
private void clientfilesendingreply() - oad clientfilesendingreply processing into dispatcher dictionary
private void browseCategory_filename_transfer_feply() - load reply_browseaskingfile processing into dispatcher dictionary
private void browseCategory_filename_reply() - load reply_categoryByBrowse processing into dispatcher dictionary
private void loadDispatcher() - Loads all the delegates above into Dispatcher dictionary
private string removeFirstDir(string path) - Removes the first directory from Stack used to stire the current path of directory
private void update_dir_again(string tabname, string currentpath) - With user selction of Different Folder in Directory List Item , a new message is sent to get new Folders in that selected folder
async Task PutTaskDelay(int i) - Put async wait of entered second for automation puprpose
private async void unitTestCaseAsync() - A unit test case to demonstarte all the requirement which will run without human intervention.
private async void unitTestCaseClient2Async(string portnumber) - A unit test case for Client 2 to demonstarte second client can also be contributor to Code Repo.
private void switchTabs(bool value) - Generic functions to disable/enable tabs as per user actions.
private async void connectTestCase() - An automation test case of connect for Client 1

* Required Files:
* ---------------
* Translater.dll


* Maintenance History:
* --------------------
* ver 1.0 : 7th , Apr 2018

*/

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
using System.Threading;
using MsgPassingCommunication;
using Microsoft.Win32;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Stack<string> pathStackBrowse_ = new Stack<string>();
        private Stack<string> pathStackCheckIn_ = new Stack<string>();
        private Stack<string> pathStackFileContent_ = new Stack<string>();
        private Stack<string> pathStackMetadata_ = new Stack<string>();
        private Translater translater;
        private CsEndPoint endPoint_;
        private CsEndPoint serverPoint_;
        private Thread rcvThrd = null;
        AddFolder inputDialog;
        private Dictionary<string, Action<CsMessage>> dispatcher_
          = new Dictionary<string, Action<CsMessage>>();

        //It deque the message from receviing queue and invokes the dispacther for the command field in that message.
        private void processMessages()
        {
            ThreadStart thrdProc = () => {
                while (true)
                {
                    CsMessage msg = translater.getMessage();
                    string msgId = msg.value("command");
                    
                    if (dispatcher_.ContainsKey(msgId))
                        dispatcher_[msgId].Invoke(msg); //invokes the command stored in dispacther dictionary

                    if (msgId == "reply_threadquit")
                    {
                        Console.WriteLine("quiting thread child");
                        translater.close();
                        break;

                    }
                }
            };
            rcvThrd = new Thread(thrdProc); //initiating a new thread to which will constand deque the receivers queue.
            rcvThrd.IsBackground = true;
            rcvThrd.Start();
        }

        //Adding the delegate in dispacther with command name as key
        private void addClientProc(string key, Action<CsMessage> temp_Action_Name)
        {
            dispatcher_[key] = temp_Action_Name; //Adding command delagate to dispacther dictionary
        }

        //Clear the directory List Box in specific tab 
        private void clearDirs(string tabname)
        {
            if (tabname == "checkout")
                DirList.Items.Clear(); //Clears List
            else if (tabname == "checkin")
                checkin_DirList.Items.Clear();
            else if (tabname == "metadata")
                metadata_DirList.Items.Clear();
            else if (tabname == "filecontent")
                filecontent_DirList.Items.Clear();
        }
        //----< function dispatched by child thread to main thread >-------

        // Add item in Directory Listbox with folder name in specific tab
        private void addDir(string dir, string tabname)
        {
            if (tabname == "checkout")
                DirList.Items.Add(dir); //Add Item to List
            else if (tabname == "checkin")
                checkin_DirList.Items.Add(dir);
            else if (tabname == "metadata")
                metadata_DirList.Items.Add(dir);
            else if (tabname == "filecontent")
                filecontent_DirList.Items.Add(dir);
        }
        //----< function dispatched by child thread to main thread >-------

        //Always add a item (..) in Directory Listbox which helps users in navigating back in specific tab
        private void insertParent(string tabname)
        {
            if (tabname == "checkout")
                DirList.Items.Insert(0, ".."); //Add Item to List at 0 location
            else if (tabname == "checkin")
                checkin_DirList.Items.Insert(0, "..");
            else if (tabname == "metadata")
                metadata_DirList.Items.Insert(0, "..");
            else if (tabname == "filecontent")
                filecontent_DirList.Items.Insert(0, "..");
        }
        //----< function dispatched by child thread to main thread >-------

        //Clear the File list box in specific tab
        private void clearFiles(string tabname)
        {
            if (tabname == "checkout")
                FileList.Items.Clear(); //Clears List
            else if (tabname == "checkin")
                checkIn_FileList.Items.Clear();
            else if (tabname == "metadata")
                metadata_FileList.Items.Clear();
            else if (tabname == "filecontent")
                filecontent_FileList.Items.Clear();
        }
        //----< function dispatched by child thread to main thread >-------

        // add File name as list item in File List Box in specific tab.
        private void addFile(string file,string tabname)
        {
            if (tabname == "checkout")
                FileList.Items.Add(file); //Add Item in List
            else if (tabname == "checkin")
                checkIn_FileList.Items.Add(new FileDetails { FileName = file });
            else if (tabname == "metadata")
                metadata_FileList.Items.Add(file);
            else if (tabname == "filecontent")
                filecontent_FileList.Items.Add(file);

        }

    
        //----< load getDirs processing into dispatcher dictionary >-------
        private void DispatcherLoadGetDirs()
        {
            Action<CsMessage> getDirs = (CsMessage rcvMsg) =>
            {
                Action clrDirs = () =>
                {
                    clearDirs(rcvMsg.value("tab")); //Clear List
                };
                Dispatcher.Invoke(clrDirs, new Object[] { }); //invoke clear Directory Delegate
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("dir"))
                    {
                        Action<string> doDir = (string dir) =>
                        {
                            addDir(dir,rcvMsg.value("tab"));
                        };
                        Dispatcher.Invoke(doDir, new Object[] { enumer.Current.Value });
                    }
                }
                Action insertUp = () =>
                {
                    insertParent(rcvMsg.value("tab"));
                };
                Dispatcher.Invoke(insertUp, new Object[] { });
            };
            addClientProc("getDirs", getDirs);
        }

        //----< load getFiles processing into dispatcher dictionary >------
        private void DispatcherLoadGetFiles()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                Action clrFiles = () =>
                {
                    clearFiles(rcvMsg.value("tab"));
                };
                Dispatcher.Invoke(clrFiles, new Object[] { }); //invoke clear files from dispacther
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("file"))
                    {
                        Action<string> doFile = (string file) =>
                        {
                            addFile(file, rcvMsg.value("tab"));
                        };
                        Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("getFiles", getFiles); //adding delegate to dispatcher
        }

        //----< load reply_connect processing into dispatcher dictionary >------
        private void connectServer()
        {

            Action<CsMessage> reply_Connect = (CsMessage rcvMsg) =>
            {
                Action<CsMessage> reply_Connect_Msg = (CsMessage msgcontent) =>
                  {
                      string content = rcvMsg.value("content");
                      txt_connect_msg.Text = txt_connect_msg.Text + "\n" + content + "\n";
                      btn_Connect.IsEnabled = false;
                      btn_Disconnect.IsEnabled = true;
                      switchTabs(true);

                  };
                Dispatcher.Invoke(reply_Connect_Msg, new Object[] {rcvMsg}); //invoking delegate

            };
            addClientProc("reply_connect",reply_Connect); //ading delegate
        }

        //----< load reply_threadquit processing into dispatcher dictionary >------
        private void threadQuit()
        {

            Action<CsMessage> reply_threadquit = (CsMessage rcvMsg) =>
            {
                Action<CsMessage> reply_Connect_Msg = (CsMessage msgcontent) =>
                {
                    string content = rcvMsg.value("content");
                    txt_connect_msg.Text = txt_connect_msg.Text + "\n" + content + "\n";
                    tabControl.SelectedIndex = 0;
                    switchTabs(false);
                    btn_Connect.IsEnabled = true;
                    btn_Disconnect.IsEnabled = false;

                };
                Dispatcher.Invoke(reply_Connect_Msg, new Object[] { rcvMsg }); //invoking delegate

            };
            addClientProc("reply_threadquit", reply_threadquit); //adding delegate
        }

        //----< load reply_metadata processing into dispatcher dictionary >------
        private void metadatahandling()
        {

            Action<CsMessage> reply_metadata = (CsMessage rcvMsg) =>
            {
                Action<CsMessage> reply_Connect_Msg = (CsMessage msgcontent) =>
                {
                    string content = rcvMsg.value("content");
                    txt_metadata_msg.Text = txt_metadata_msg.Text + "\n" + content + "\n";
                    txt_metadata_msg.Text = txt_metadata_msg.Text + rcvMsg.value("metadata_name") + "\n";
                    txt_metadata_msg.Text = txt_metadata_msg.Text + rcvMsg.value("metadta_description") + "\n";
                    txt_metadata_msg.Text = txt_metadata_msg.Text + rcvMsg.value("metadata_date") + "\n";
                    txt_metadata_msg.Text = txt_metadata_msg.Text + rcvMsg.value("metadata_children") + "\n";
                    Popup p = new Popup();
                    p.BrowseFile.Text = "Name -> " + rcvMsg.value("metadata_name") + "\n" + "Decription -> " + rcvMsg.value("metadta_description") + "\n" + "Date -> " + rcvMsg.value("metadata_date") + "\n" + "Children -> " + rcvMsg.value("metadata_children");
                    p.Title = "Metadata - " + rcvMsg.value("filename");
                    p.Show();
                    
                };
                Dispatcher.Invoke(reply_Connect_Msg, new Object[] { rcvMsg }); //invoking delegate 

            };
            addClientProc("reply_metadata", reply_metadata); //adding delegate to dispacther
        }

        //----< load reply_foldername processing into dispatcher dictionary >------
        private void addFolder()
        {
            Action<CsMessage> reply_foldername = (CsMessage rcvMsg) =>
            {
                Action<CsMessage> reply_Connect_Msg = (CsMessage msgcontent) =>
                {
                    string content = rcvMsg.value("content");
                    txt_checkin_msg.Text = txt_checkin_msg.Text + "\n" + content + "\n";

                };
                Dispatcher.Invoke(reply_Connect_Msg, new Object[] { rcvMsg }); //invoking delegate

            };
            addClientProc("reply_foldername", reply_foldername); //adding delegate to dispatcher

        }

        //----< load clientfileaskingreply processing into dispatcher dictionary >------
        private void clientfileaskingreply()
        {
            Action<CsMessage> client_file_asking_reply = (CsMessage rcvMsg) =>
            {
                Action<CsMessage> reply_Connect_Msg = (CsMessage msgcontent) =>
                {
                    string content = rcvMsg.value("content");
                    string tab = rcvMsg.value("tab");
                    
                    if (tab == "connect")
                    {
                        if (Int32.Parse(rcvMsg.value("content-length")) > 0)
                            txt_connect_msg.Text = txt_connect_msg.Text + "\n" + content + "\n";
                    }else if(tab == "checkout")
                    {
                        if (Int32.Parse(rcvMsg.value("content-length")) > 0)
                        {
                            txt_checkout_msg.Text = txt_checkout_msg.Text + "\n" + content + "\n";
                            string test = rcvMsg.value("file");
                            int index = test.IndexOf('/');
                            string nmspc = "";
                            if (index > 0)
                                nmspc = test.Substring(index + 1);
                            txt_checkout_msg.Text = txt_checkout_msg.Text + " File Chunk Client Location -> " + System.IO.Path.GetFullPath("../../../../ClientFiles/"+ nmspc) + "\n";
                        }
                    }else if(tab == "filecontent")
                    {
                        if (Int32.Parse(rcvMsg.value("content-length")) > 0)
                        {
                            txt_filecontent_msg.Text = txt_filecontent_msg.Text + "\n" + content + "\n";
                            string test = rcvMsg.value("file");
                            int index = test.IndexOf('/');
                            string nmspc = "";
                            if (index > 0)
                                nmspc = test.Substring(index + 1);
                            string text = System.IO.File.ReadAllText("../../../../ClientFiles/" + nmspc);
                            txt_filecontent_msg.Text = txt_filecontent_msg.Text + " File Chunk Client Location -> " + System.IO.Path.GetFullPath("../../../../ClientFiles/" + nmspc) + "\n";
                            Popup p = new Popup();
                            p.BrowseFile.Text = text;
                            p.Title = nmspc;
                            p.Show();
                        }
                    }
                };
                Dispatcher.Invoke(reply_Connect_Msg, new Object[] { rcvMsg }); //invoking delegate
            };
            addClientProc("clientfileaskingreply", client_file_asking_reply); //adding delegate to dispacther
        }

        //----< load clientfilesendingreply processing into dispatcher dictionary >------
        private void clientfilesendingreply()
        {
            Action<CsMessage> client_file_sending_reply = (CsMessage rcvMsg) =>
            {
                Action<CsMessage> reply_Connect_Msg = (CsMessage msgcontent) =>
                {
                    string content = rcvMsg.value("content");
                    string tabname = rcvMsg.value("tab");
                    //rcvMsg.show();
                    if (tabname == "connect")
                        txt_connect_msg.Text = txt_connect_msg.Text + "\n" + content + "\n";
                    else if (tabname == "checkin")
                    {
                        if (Int32.Parse(rcvMsg.value("content-length")) > 0)
                        {
                            txt_checkin_msg.Text = txt_checkin_msg.Text + "\n" + content + "\n";
                            txt_checkin_msg.Text = txt_checkin_msg.Text + "Server Files location -> ../ServerFiles/" + rcvMsg.value("nmspc") + "/" + rcvMsg.value("file_name") + "\n";
                        }
                    }

                };
                Dispatcher.Invoke(reply_Connect_Msg, new Object[] { rcvMsg }); //invoking delegate

            };
            addClientProc("clientfilesendingreply", client_file_sending_reply); //addinfg delegate to dispacther
        }

        //----< load reply_browseaskingfile processing into dispatcher dictionary >------
        private void browseCategory_filename_transfer_feply()
        {
            Action<CsMessage> reply_browseaskingfile = (CsMessage rcvMsg) =>
            {
                Action<CsMessage> reply_Connect_Msg = (CsMessage msgcontent) =>
                {
                    string content = rcvMsg.value("content");
                    txt_browse_msg.Text = txt_browse_msg.Text + "\n" + content + "\n";
                    string test = rcvMsg.value("file");
                    int index = test.IndexOf('/');
                    string nmspc = "";
                    if (index > 0)
                    {
                        nmspc = test.Substring(index + 1);
                    }
                    txt_browse_msg.Text = txt_browse_msg.Text + "File chunk successfully saved at -> " + System.IO.Path.GetFullPath("../../../../" + nmspc) + "\n";

                };
                Dispatcher.Invoke(reply_Connect_Msg, new Object[] { rcvMsg }); //invoking delegate

            };
            addClientProc("reply_browseaskingfile", reply_browseaskingfile); //adding delegate to dispacther
        }

        //----< load reply_categoryByBrowse processing into dispatcher dictionary >------
        private void browseCategory_filename_reply()
        {
            Action<CsMessage> reply_categoryByBrowse = (CsMessage rcvMsg) =>
            {
                Action<CsMessage> reply_Connect_Msg = (CsMessage msgcontent) =>
                {
                    string filenames = rcvMsg.value("filenames");
                    string content = rcvMsg.value("content");
                    string nmspc = rcvMsg.value("nmspcs");
                    CsMessage msg2 = new CsMessage(); //cretaing a message and setting ots fields.
                    msg2.add("to", CsEndPoint.toString(serverPoint_));
                    msg2.add("from", CsEndPoint.toString(endPoint_));
                    msg2.add("command", "req_browseaskingfile");
                    msg2.add("filename", filenames);
                    msg2.add("nmspc", nmspc);
                    translater.postMessage(msg2);
                    txt_browse_msg.Text = txt_browse_msg.Text + "\n" + content + "- " + filenames + "\n";
                    Browse_FileList.Items.Add(filenames);
                };
                Dispatcher.Invoke(reply_Connect_Msg, new Object[] { rcvMsg }); //invoking delegate

            };
            addClientProc("reply_categoryByBrowse", reply_categoryByBrowse); //adding delegate to dispacther 

        }

        // Loads all the delegates above into Dispatcher dictionary
        private void loadDispatcher()
        {
            //all function calls to load all the delegates into dispatcher dictionary 
            connectServer();
            clientfilesendingreply();
            clientfileaskingreply();
            browseCategory_filename_reply();
            browseCategory_filename_transfer_feply();
            DispatcherLoadGetFiles();
            DispatcherLoadGetDirs();
            addFolder();
            metadatahandling();
            threadQuit();
        }

        //Create message and send it to Server to fetch all the files which belongs to specific category
        private void browse_Click(object sender, RoutedEventArgs e)
        {
            txt_browse_msg.Text = "Client -> Asking server for files having category - " + txt_browse_filename.Text;
            CsMessage msg = new CsMessage(); //creating a message
            msg.add("to", CsEndPoint.toString(serverPoint_));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "req_categoryByBrowse");
            msg.add("categoryvalue", txt_browse_filename.Text);
            translater.postMessage(msg); //posting it to client senders queue

        }

        //Opens client file into new Pop Up with all its content.
        private void browse_ListBox_doubleClick(object sender, RoutedEventArgs e)
        {
            var filename = Browse_FileList.SelectedItem;
            if (filename != null)
            {
                string str_filename = filename.ToString();
                string text = System.IO.File.ReadAllText("../../../../ClientFiles/" + str_filename); //Read file into string
                Popup p = new Popup();
                p.BrowseFile.Text = text;
                p.Title = str_filename;
                p.Show(); //opening pop up to show file content
            }

        }

        //Moved code of checkin_AddFolder_Click to this functipon to handle folder creation during automation 
        private void movingbtncode(string param1,string param2,string param3)
        {
            string foldername = "";
            inputDialog = new AddFolder(param1, param2);
                if (inputDialog.ShowDialog() == true)
                {
                    foldername = inputDialog.Answer;
                    checkin_DirList.Items.Add(foldername);
                    txt_checkin_msg.Text = "Client -> Requester server to create namespace " + foldername;
                    CsMessage msg2 = new CsMessage(); //Creating a new message
                    msg2.add("to", CsEndPoint.toString(serverPoint_));
                    msg2.add("from", CsEndPoint.toString(endPoint_));
                    msg2.add("command", "req_foldername");
                    msg2.add("nmspc", foldername);
                    msg2.add("tab", "checkin");
                    translater.postMessage(msg2); //Sending a message to Clinet Sender Queue
                }
  
        }

        //Calls movingbtncode with default values on click of Add Folder button
        private void checkin_AddFolder_Click(object sender, RoutedEventArgs e)
        {
            movingbtncode("Enter namespace name","","");    
        }

       //Open File Dialog window which helps users to navigate to the file and selcted file is added to File List Box in checkIn Tab
        private void checkin_AddFile_Click(object sender, RoutedEventArgs e)
        {
            string path;
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == true)
            {
                path = file.FileName;
                string test = path;
                string filename = "";
                int index = test.LastIndexOf('\\'); //Finding File Name
         
                if (index > 0)
                {
                    filename = test.Substring(index + 1);
                }
                checkIn_FileList.Items.Add(new FileDetails { FileName = filename, LocalPath = path }); //Adding item to List
            }

        }

        //Creates message with file attribute and send the message to Client senders queue which will will inturn send the file to server in form of bytes.
        private void checkin_Checkin_Click(object sender, RoutedEventArgs e)
        {
          
            var filename = checkIn_FileList.SelectedItem as FileDetails;
            if (filename != null)
            {
                if (filename.LocalPath != null)
                {
                    string str_filename = filename.FileName.ToString();
                    string str_filepath = System.IO.Path.GetFullPath(filename.LocalPath.ToString());
                    string test = checkin_PathTextBlock.Text;
                    int index = test.IndexOf('/');
                    string str_nmspc = "";
                    if (index > 0)
                    {
                        str_nmspc = test.Substring(index + 1); //Findng namespace name
                    }
                    txt_checkin_msg.Text = txt_checkin_msg.Text + "Client -> Sending File " + str_filename + " to server in namespace " + str_nmspc;
                    CsMessage msg1 = new CsMessage(); //Creating a new message
                    msg1.add("to", CsEndPoint.toString(serverPoint_));
                    msg1.add("from", CsEndPoint.toString(endPoint_));
                    msg1.add("command", "clientsendingfile");
                    msg1.add("nmspc", str_nmspc);
                    msg1.add("file", str_filepath);
                    msg1.add("file_name", str_filename);
                    msg1.add("clientsending", "yes");
                    msg1.add("tab", "checkin");
                    txt_checkin_msg.Text = txt_checkin_msg.Text + "\nClient File Path being uploaded -> " + str_filepath;
                    translater.postMessage(msg1); //Posting a message to client senders queue.
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("File is already checked in !!", "Check In..", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("You forgot to select File !!", "No Selection..", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        //open client file into new pop up
        private void checkOut_Open_doubleClick(object sender, RoutedEventArgs e)
        {
            var filename = FileList.SelectedItem;
            if (filename != null)
            {
                string str_filename = filename.ToString();
                string fullPath = "../../../../ClientFiles/" + str_filename;
                if (System.IO.File.Exists(fullPath))
                {
                    string text = System.IO.File.ReadAllText(fullPath); //Readig file to string
                    Popup p = new Popup();
                    p.BrowseFile.Text = text;
                    p.Title = str_filename;
                    p.Show(); //Showinf file contents in a pop up
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Please check Out File First !!", "Not Checked Out..", MessageBoxButton.OK, MessageBoxImage.Error);
                    //Showing error message
                }
            }

        }

        //Creates a message and send it to server by providing filename and namespace , server will inturn sends the file to client through handler mentioned in serverprototype.cpp
        private void checkout_Checkout_Click(object sender, RoutedEventArgs e)
        {
            var filename = FileList.SelectedItem;
            if (filename != null)
            {
                string str_filename = filename.ToString();
                txt_checkout_msg.Text = txt_checkout_msg.Text + "\nClient->Download File " + str_filename + " from Server ...";
                string test = PathTextBlock.Text;
                int index = test.IndexOf('/');
                string nmspc = "";
                if (index > 0)
                {
                    nmspc = test.Substring(index + 1); //finsing namespace
                }

                CsMessage msg2 = new CsMessage(); //Create new message and set attributes
                msg2.add("to", CsEndPoint.toString(serverPoint_));
                msg2.add("from", CsEndPoint.toString(endPoint_));
                msg2.add("command", "clientaskingfile");
                msg2.add("filename", str_filename);
                msg2.add("nmspc", nmspc);
                msg2.add("tab", "checkout");
                translater.postMessage(msg2); //Posting a message to Clients sender Queue
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("No Selection..", "You forgot to select File !!", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        //Creates a message and send it to server for the file is selected in List View , once the file is downloaded it opens in new pop up which is handled by dispacther for clientaskingfile
        private void filecontent_Open_doubleClick(object sender, RoutedEventArgs e)
        {
            var filename = filecontent_FileList.SelectedItem;
            string str_filename = filename.ToString();
            txt_filecontent_msg.Text = txt_filecontent_msg.Text + "\nClient->Download File " + str_filename + " from Server ...";
            string test = filecontent_PathTextBlock.Text;
            int index = test.IndexOf('/');
            string nmspc = "";
            if (index > 0)
            {
                nmspc = test.Substring(index + 1); //Finsing namespace
            }

            CsMessage msg2 = new CsMessage(); //Create a new message and set attributes
            msg2.add("to", CsEndPoint.toString(serverPoint_));
            msg2.add("from", CsEndPoint.toString(endPoint_));
            msg2.add("command", "clientaskingfile");
            msg2.add("filename", str_filename);
            msg2.add("nmspc", nmspc);
            msg2.add("tab", "filecontent");
            translater.postMessage(msg2); //Posting a message to Clients sender Queue
        }

        //Creates a message and send a message to server , once the server replies reach to client reply_metadata delgate is ivokded which open metadata in new pop up
        private void metadata_Open_doubleClick(object sender, RoutedEventArgs e)
        {
            string test = metadata_PathTextBlock.Text;
            int index = test.IndexOf('/');
            string nmspc = "";
            if (index > 0)
            {
                nmspc = test.Substring(index + 1); //finsng namespace
            }
            CsMessage msg2 = new CsMessage(); //create a new message and set its attributes 
            msg2.add("to", CsEndPoint.toString(serverPoint_));
            msg2.add("from", CsEndPoint.toString(endPoint_));
            msg2.add("command", "req_metadata");
            msg2.add("filename", (string)metadata_FileList.SelectedItem);
            msg2.add("nmspc", nmspc);
            msg2.add("tab", "metadata");
            txt_metadata_msg.Text = txt_metadata_msg.Text + "Client -> Requesting server for metadata of " + (string)metadata_FileList.SelectedItem;
            translater.postMessage(msg2); //Posting a message to Clients sender Queue
        }
        
        //Downloading Directory and File Structure for Tabs Check In ,  File Content , File Metadata
        private void DownloadDirectory()
        {
            //Once connection is successful , fectchign directory and file structure for CheckIn tab
            checkin_PathTextBlock.Text = "ServerFiles";
            pathStackCheckIn_.Push("../ServerFiles");
            CsMessage msg4 = new CsMessage();
            msg4.add("to", CsEndPoint.toString(serverPoint_));
            msg4.add("from", CsEndPoint.toString(endPoint_));
            msg4.add("command", "getDirs");
            msg4.add("tab", "checkin");
            msg4.add("path", pathStackCheckIn_.Peek());
            translater.postMessage(msg4);
            msg4.remove("command");
            msg4.add("command", "getFiles");
            translater.postMessage(msg4);
            //Once connection is successful , fectchign directory and file structure for View File Content tab
            filecontent_PathTextBlock.Text = "ServerFiles";
            pathStackFileContent_.Push("../ServerFiles");
            CsMessage msg5 = new CsMessage();
            msg5.add("to", CsEndPoint.toString(serverPoint_));
            msg5.add("from", CsEndPoint.toString(endPoint_));
            msg5.add("command", "getDirs");
            msg5.add("tab", "filecontent");
            msg5.add("path", pathStackFileContent_.Peek());
            translater.postMessage(msg5);
            msg5.remove("command");
            msg5.add("command", "getFiles");
            translater.postMessage(msg5);
            //Once connection is successful , fectchign directory and file structure for View Metadata tab
            metadata_PathTextBlock.Text = "ServerFiles";
            pathStackMetadata_.Push("../ServerFiles");
            CsMessage msg6 = new CsMessage();
            msg6.add("to", CsEndPoint.toString(serverPoint_));
            msg6.add("from", CsEndPoint.toString(endPoint_));
            msg6.add("command", "getDirs");
            msg6.add("tab", "metadata");
            msg6.add("path", pathStackMetadata_.Peek());
            translater.postMessage(msg6);
            msg6.remove("command");
            msg6.add("command", "getFiles");
            translater.postMessage(msg6);
        }
        //Cretaes a connection message as per user input and sends it to server , with a server reply connection is successfully established
        private void connect_Click(object sender, RoutedEventArgs e)
        {
            txt_connect_msg.Text = txt_connect_msg.Text + "Client at Port " + txt_client_port.Text + " ->Establishing connecttion with server ...";
            endPoint_ = new CsEndPoint();
            endPoint_.machineAddress = txt_client_addr.Text;
            endPoint_.port = Int32.Parse(txt_client_port.Text);
            translater = new Translater(); //comm package is accessibe through translater.dll
            translater.listen(endPoint_); //Initiating CLient Receiver Queue
            serverPoint_ = new CsEndPoint();
            serverPoint_.machineAddress = txt_server_address.Text;
            serverPoint_.port = Int32.Parse(txt_server_port.Text);
            processMessages(); //Initiating a new thread in while loop to deque recievers queue
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverPoint_));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "req_connect");
            translater.postMessage(msg); //Posting a message to clients senders queue
            TimeSpan ts = TimeSpan.FromMilliseconds(1000);
            //Once connection is successful , fectchign directory and file structure for Check Out tab
            PathTextBlock.Text = "ServerFiles";
            pathStackBrowse_.Push("../ServerFiles");
            CsMessage msg3 = new CsMessage();
            msg3.add("to", CsEndPoint.toString(serverPoint_));
            msg3.add("from", CsEndPoint.toString(endPoint_));
            msg3.add("command", "getDirs");
            msg3.add("tab", "checkout");
            msg3.add("path", pathStackBrowse_.Peek());
            translater.postMessage(msg3);
            msg3.remove("command");
            msg3.add("command", "getFiles");
            translater.postMessage(msg3);
            TimeSpan ts1 = TimeSpan.FromMilliseconds(1000);
            DownloadDirectory();
        }

        //Removes the first directory from Stack used to stire the current path of directory
        private string removeFirstDir(string path)
        {
            string modifiedPath = path;
            int pos = path.IndexOf("/");
            modifiedPath = path.Substring(pos + 1, path.Length - pos - 1);
            return modifiedPath; //returning string before last /
        }

        //With user selction of Different Folder in Directory List Item , a new message is sent to get new Folders in that selected folder
        private void update_dir_again(string tabname, string currentpath)
        {
            // build message to get dirs and post it
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverPoint_));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("tab", tabname);
            msg.add("path", currentpath);
            translater.postMessage(msg);

            // build message to get files and post it
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg); //Posting a message to Client Sender Queue
        }

        //File List and directory List should chnage in File Content tab when any folder is selected.
        private void filecontent_DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // build path for selected dir
            string selectedDir = (string)filecontent_DirList.SelectedItem;
            string path;
            if (selectedDir != null)
            {
                if (selectedDir == "..")
                {
                    if (pathStackFileContent_.Count > 1)  // don't pop off "Storage"
                        pathStackFileContent_.Pop();
                    else
                        return;
                }
                else
                {
                    path = pathStackFileContent_.Peek() + "/" + selectedDir;
                    pathStackFileContent_.Push(path);
                }
                // display path in Dir TextBlcok
                filecontent_PathTextBlock.Text = removeFirstDir(pathStackFileContent_.Peek());
                update_dir_again("filecontent", pathStackFileContent_.Peek());
            }

        }

        //File List and directory List should chnage in Metadata tab when any folder is selected.
        private void metadata_DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // build path for selected dir
            string selectedDir = (string)metadata_DirList.SelectedItem;
            string path;
            if (selectedDir != null)
            {
                if (selectedDir == "..")
                {
                    if (pathStackMetadata_.Count > 1)  // don't pop off "Storage"
                        pathStackMetadata_.Pop();
                    else
                        return;
                }
                else
                {
                    path = pathStackMetadata_.Peek() + "/" + selectedDir;
                    pathStackMetadata_.Push(path);
                }
                // display path in Dir TextBlcok
                metadata_PathTextBlock.Text = removeFirstDir(pathStackMetadata_.Peek());
                update_dir_again("metadata", pathStackMetadata_.Peek());
            }
        }

        //----< respond to mouse double-click on dir name >----------------
        private void DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // build path for selected dir
            string selectedDir = (string)DirList.SelectedItem;
            string path;
            if (selectedDir != null)
            {
                if (selectedDir == "..")
                {
                    if (pathStackBrowse_.Count > 1)  // don't pop off "Storage"
                        pathStackBrowse_.Pop();
                    else
                        return;
                }
                else
                {
                    path = pathStackBrowse_.Peek() + "/" + selectedDir;
                    pathStackBrowse_.Push(path);
                }
                // display path in Dir TextBlcok
                PathTextBlock.Text = removeFirstDir(pathStackBrowse_.Peek());
                update_dir_again("checkout", pathStackBrowse_.Peek());
            }

        }

        //File List and directory List should chnage in CheckIn tab when any folder is selected.
        private void checkin_DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string path;
            string selectedDir = (string)checkin_DirList.SelectedItem;
            if (selectedDir != null)
            {
                if (selectedDir == "..")
                {
                    checkin_AddFolder.IsEnabled = true;
                    checkin_AddFile.IsEnabled = false;
                    if (pathStackCheckIn_.Count > 1)  // don't pop off "Storage"
                        pathStackCheckIn_.Pop();
                    else
                        return;
                }
                else
                {
                    checkin_AddFolder.IsEnabled = false;
                    checkin_AddFile.IsEnabled = true;
                    path = pathStackCheckIn_.Peek() + "/" + selectedDir;
                    pathStackCheckIn_.Push(path);
                }
                // display path in Dir TextBlcok
                checkin_PathTextBlock.Text = removeFirstDir(pathStackCheckIn_.Peek());

                // build message to get dirs and post it
                CsMessage msg = new CsMessage();
                msg.add("to", CsEndPoint.toString(serverPoint_));
                msg.add("from", CsEndPoint.toString(endPoint_));
                msg.add("command", "getDirs");
                msg.add("tab", "checkin");
                msg.add("path", pathStackCheckIn_.Peek());
                translater.postMessage(msg);
                // build message to get files and post it
                msg.remove("command");
                msg.add("command", "getFiles");
                translater.postMessage(msg);
            }
        }

        //Put async wait of entered second for automation puprpose
        async Task PutTaskDelay(int i)
        {
            int microseconds = i * 1000;
            await Task.Delay(microseconds); // aync wait 
        }
   
        //An automation test case of connect for Client 1
        private void connectTestCase()
        {
            Console.WriteLine("*Output* -> ---------------Connect-----------\n*Output* -> Starting with connection Part ,  client is assigned port 8081 and server is assigned port 8080");
            Console.WriteLine("*Output* -> In connect box , there is a text box where Client request and server reply is visible");
            RoutedEventArgs e = new RoutedEventArgs();
            connect_Click(this, e); //Making connection
            Console.WriteLine("*Output* -> As connection is extablished , Directories and corresponding Files Structured is loaded in Check In , Check Out , View File && View File Metadata");
            Console.WriteLine("*Output* -> Check In Tab - Directory List , there is  a CR4c1 folder created and synced with server which can be found at ../ServerFiles/CR4c1");
        }

        //A unit test case for Client 1 to demonstarte all the requirement which will run without human intervention.
        private async void unitTestCaseAsync()
        {
            txt_connect_msg.Text = "Automation of Test cases will take around 45 second , Wait till second client opens and run its connect test case. Client 2 will notify when you can resume.\n";await PutTaskDelay(2);
            this.Title = "CR4c - Client 1 - 8081"; //changing title of window
            connectTestCase(); await PutTaskDelay(4);
            RoutedEventArgs e = new RoutedEventArgs();
            tabControl.SelectedIndex = 1;
            movingbtncode("Enter namespace name", "CR4c1", "automate"); //Creatig a folder with name CR4c1 
            checkin_DirList.SelectedIndex = checkin_DirList.Items.IndexOf("CR4c1");
            checkin_DirList_MouseDoubleClick(this, null); await PutTaskDelay(4); //Double click created folder
            Console.WriteLine("*Output* -> ---------------CheckIn-----------\n*Output* -> Check In Tab - Check There is no file in CR4c1 folder , Adding a file in Files List Director ");
            this.checkIn_FileList.Items.Add(new FileDetails { FileName = "Logger.cpp", LocalPath = "..\\..\\..\\..\\CppCommWithFileXfer\\Logger\\Logger.cpp", ServerPath = "", Children = "" });
            checkIn_FileList.SelectedIndex = 0; //Select File 
            checkin_Checkin_Click(this, e); await PutTaskDelay(4);//Click on check In button
            Console.WriteLine("*Output* -> Check In Tab , Client and server messages are shown in checkin Text Box on the right \n *Output* -> Check In Tab - Logger.cpp is successfully checked into CR4c1 folder , at location ../ServerFiles/CR4c1/ \n *Output* -> Check Out Tab - Check out file from CR4c2 folder (../ServerFiles/CR4c2) and it does not exist with Client at (../ClientFiles) \n *Output* -> Selection Check Out Tab -> Double clicking Directory CR4c2  -> Select File FileSystem.h \n *Output* -> click on Checkout to Donwload File and then double click to open the file in new window");
            tabControl.SelectedIndex = 2; await PutTaskDelay(2);
            DirList.SelectedIndex = DirList.Items.IndexOf("CR4c2"); await PutTaskDelay(1);
            DirList_MouseDoubleClick(this, null); await PutTaskDelay(3); //Docuble click folder
            FileList.SelectedIndex = FileList.Items.IndexOf("FileSystem.h");//chnage Tab
            checkout_Checkout_Click(this, e); await PutTaskDelay(3);// Click checkout
            Console.WriteLine("*Output* -> ---------------CheckOut-----------\n*Output* -> Checkout Tab -  File Filesystem.h is downloaded at ../../../../ClientFiles ");
            checkOut_Open_doubleClick(this, null); await PutTaskDelay(2);
            Console.WriteLine("*Output* -> Checkout Tab - File is opened in new pop up check popup with title FileSystem.h ");
            this.Focus(); //getting focus back to window
            Console.WriteLine("*Output* -> Checkout Tab - Check client and server messages in the text box on the right \n Output* -> Test Completed for client1 \n *Output* -> Browse Tab -> Download File Message.h from server location ../ServerFiles/CR4c3 to client loaction ../../../../ClienFiles\n*Output* -> Browse Tab - Select Tab -> enter category which is category 1 by default -> Click Browse button -> Docuble Click file to view file");
            tabControl.SelectedIndex = 3; await PutTaskDelay(2);
            browse_Click(this, e); await PutTaskDelay(3);//Click browse button
            Console.WriteLine("*Output* -> ---------------Browse-----------\n*Output* -> Browse Tab - Fetched files for category 1 is shown in List View like file Message.h");
            Browse_FileList.SelectedIndex = Browse_FileList.Items.IndexOf("Message.h"); await PutTaskDelay(1);
            browse_ListBox_doubleClick(this, null); await PutTaskDelay(2);
            Console.WriteLine("*Output* -> Browse Tab - After double clicking file , check pop up with title Message.h which has content of file \n *Ouput* -> Browse Tab -> Check server and client messages on the Browse box on the right ");
            this.Focus();
            Console.WriteLine("*Output* -> ---------------File Content-----------\n*Output* -> View File Content -> Double CLick File downloads file test.cpp from server at location  ../ServerFiles/CR4c4 to client loaction ../../../../ClienFiles/\n*Output* -> View File Content - Select folder (CRc4) -> Select File (test.cpp) -> Doucble Click File -> Check Pop Up");
            tabControl.SelectedIndex = 4;await PutTaskDelay(1);
            filecontent_DirList.SelectedIndex = filecontent_DirList.Items.IndexOf("CR4c4");await PutTaskDelay(1);
            filecontent_DirList_MouseDoubleClick(this, null); await PutTaskDelay(3);//Docuble click file
            Console.WriteLine("*Output* -> View File Content - Into Folder CRc4");
            filecontent_FileList.SelectedIndex = filecontent_FileList.Items.IndexOf("test.cpp"); await PutTaskDelay(1);
            filecontent_Open_doubleClick(this, null); await PutTaskDelay(2);
            Console.WriteLine("*Output* -> View File Content - check pop with title name test.cpp to see file content \n *Output* -> File Content - View server and client message in File Content text box on thr right");this.Focus();
            Console.WriteLine("*Output* -> ---------------File Metadata-----------\n*Output* -> View File Metadata -> Double CLick File downloads file IComm.h from server at location  ../ServerFiles/CR4c5 to client loaction ../../../../ClienFiles/ \n *Output* -> View File Metadata - Select folder (CRc5) -> Select File (IComm.h) -> Doucble Click File -> Check Pop up with file metadata");
            tabControl.SelectedIndex = 5; await PutTaskDelay(1);
            metadata_DirList.SelectedIndex = metadata_DirList.Items.IndexOf("CR4c5");  await PutTaskDelay(1);
            metadata_DirList_MouseDoubleClick(this, null); await PutTaskDelay(3);
            metadata_FileList.SelectedIndex = metadata_FileList.Items.IndexOf("IComm.h"); await PutTaskDelay(1);
            metadata_Open_doubleClick(this, null); await PutTaskDelay(2);
            Console.WriteLine("*Output* -> View File Metadata -> Double CLicked File IComm.h metadata can be seen in pop up windows with name IComm.h \n *Output* -> View File Metadta -> Server and client messages are shown in Text box on the right \n *Output* -> automation test case is completed for Client 1 , now Client 2 will open..");
            this.Focus();
        }

        //A unit test case for Client 2 to demonstarte second client can also be contributor to Code Repo.
        private async void unitTestCaseClient2Async(string portnumber)
        {
            await PutTaskDelay(2);
            this.Title = "CR4c - Client 2 - 8082";
            Console.WriteLine("*Output* -> Client 2 - 8082 - Connect Test Case ");

            this.txt_client_port.Text = portnumber;
            await PutTaskDelay(1);
            RoutedEventArgs e = new RoutedEventArgs();
            connect_Click(this, e); //Call connect click event
            await PutTaskDelay(3);
            Console.WriteLine("*Output* -> Client 2 - 8082 - Connection Successful ");
            Console.WriteLine("*Output* -> Client 2 - 8082 - Test Case completed for Client 2 ");
            Console.WriteLine("*Output* -> Client 2 - 8082 - Either you can proceed testing on this Client (Client 2) or on the clien 1 where all the requirements is shown !! ");
            this.txt_connect_msg.Text = this.txt_connect_msg.Text + "\n" + "Either you can proceed testing on this Client (Client 2) or on the clien 1 where all the requirements is shown !!";
        }

        //Generic functions to disable/enable tabs as per user actions.
        private void switchTabs(bool value)
        {
            Tab_Checkin.IsEnabled = value; //Enable/Diable Tabs
            Tab_Checkout.IsEnabled = value;
            Tab_Browse.IsEnabled = value;
            Tab_ViewFile.IsEnabled = value;
            Tab_Viewmetadta.IsEnabled = value;
        }

        //Creates default setting and excutes automation Test Cases for 2 clients
        private void window_Load(object sender, RoutedEventArgs e)
        {
            loadDispatcher(); //load dispacther
            this.checkin_AddFile.IsEnabled = false;
            switchTabs(false);
            btn_Disconnect.IsEnabled = false;
            Dictionary<string, string> dc = new Dictionary<string, string>();
            string[] args = Environment.GetCommandLineArgs();
            Console.WriteLine("-----------" + args.Length);
            for (int index = 1; index < args.Length; index += 2)
            {
                dc.Add(args[index], args[index + 1]);
                Console.WriteLine(args[index] + "------------" + args[index + 1]);
            }
            if (args.Length == 3)
            {
               unitTestCaseClient2Async(args[2]); //Client 2 automation test case
            }
            else
            {
                unitTestCaseAsync(); //Client 1 automation test case
            }
        }

        //Create and send a message about diconnection to Server.
        private void Click_Diconnect(object sender, RoutedEventArgs e)
        {
            CsMessage msg2 = new CsMessage(); //Create a new message for disconnect
            msg2.add("to", CsEndPoint.toString(serverPoint_));
            msg2.add("from", CsEndPoint.toString(endPoint_));
            msg2.add("command", "req_threadquit");
            msg2.add("tab", "connect");
            txt_connect_msg.Text = txt_connect_msg.Text + "Client -> Sending disconnect request to server !!\n";
            translater.postMessage(msg2); //Posting a message to Client Sender Queue
            txt_browse_msg.Text = "";
            txt_checkin_msg.Text = "";
            txt_checkout_msg.Text = "";
            txt_filecontent_msg.Text = "";
            txt_metadata_msg.Text = "";
        }
        
        //Creates and send a diconnection message to server and close the client window.
        private async void myWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (btn_Disconnect.IsEnabled == true)
            {
                CsMessage msg2 = new CsMessage();
                msg2.add("to", CsEndPoint.toString(serverPoint_));
                msg2.add("from", CsEndPoint.toString(endPoint_));
                msg2.add("command", "req_threadquit");
                msg2.add("tab", "connect");
                txt_connect_msg.Text = txt_connect_msg.Text + "Client -> Sending disconnect request to server !!\n";
                translater.postMessage(msg2); //Posting a message to Client Sender Queue
            }
            await PutTaskDelay(1);
            e.Cancel = false; // closing client
        }
    }

    //File Details which has all member variables to get and set List Item Template used in Check In Tab
    public class FileDetails
    {
        public string FileName { get; set; }
        public string LocalPath { get; set; }
        public string ServerPath { get; set; }
        public string Children { get; set; }
    }
}
