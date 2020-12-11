using System;
using System.Diagnostics;

namespace AlexPi.Scr.Logic
{
  class CpuUse
  {
    void BindToRunningProcesses()
    {
      var currentProcess = Process.GetCurrentProcess();
      var localAll = Process.GetProcesses();      // Get all processes running on the local computer.
      var localByName = Process.GetProcessesByName("notepad"); // Get all instances of Notepad running on the local computer. This will return an empty array if notepad isn't running.
      var localById = Process.GetProcessById(1234);      // Get a process on the local computer, using the process id. This will throw an exception if there is no such process.
      // Get processes running on a remote computer. Note that this
      // and all the following calls will timeout and throw an exception
      // if "myComputer" and 169.0.0.0 do not exist on your local network.
      var remoteAll = Process.GetProcesses("myComputer");      // Get all processes on a remote computer.
      var remoteByName = Process.GetProcessesByName("notepad", "myComputer");       // Get all instances of Notepad running on the specific computer, using machine name.
      var ipByName = Process.GetProcessesByName("notepad", "169.0.0.0");      // Get all instances of Notepad running on the specific computer, using IP address.
      var remoteById = Process.GetProcessById(2345, "myComputer");       // Get a process on a remote computer, using the process id and machine name.
    }
    public static void Main__() // https://msdn.microsoft.com/en-us/library/system.diagnostics.process.totalprocessortime.aspx?f=255&MSPPError=-2147217396
    {
      long peakPagedMem = 0, peakWorkingSet = 0, peakVirtualMem = 0; // Define variables to track the peak memory usage of the process.
      var myProcess = Process.GetCurrentProcess();
      try
      {
        do
        {
          if (!myProcess.HasExited)
          {
            myProcess.Refresh();             // Refresh the current process property values.

            // Display current process statistics.
            Console.WriteLine("{0} -", myProcess.ToString());
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("  physical memory usage: {0}", myProcess.WorkingSet64); Console.WriteLine("  base priority: {0}", myProcess.BasePriority); Console.WriteLine("  priority class: {0}", myProcess.PriorityClass);
            Console.WriteLine("  user processor time: {0}", myProcess.UserProcessorTime);
            Console.WriteLine("  privileged processor time: {0}", myProcess.PrivilegedProcessorTime);
            Console.WriteLine("  total processor time: {0}", myProcess.TotalProcessorTime); Console.WriteLine("  PagedSystemMemorySize64: {0}", myProcess.PagedSystemMemorySize64); Console.WriteLine("  PagedMemorySize64: {0}", myProcess.PagedMemorySize64);

            // Update the values for the overall peak memory statistics.
            peakPagedMem = myProcess.PeakPagedMemorySize64;
            peakVirtualMem = myProcess.PeakVirtualMemorySize64;
            peakWorkingSet = myProcess.PeakWorkingSet64;

            if (myProcess.Responding)
            {
              Console.WriteLine("Status = Running");
            }
            else
            {
              Console.WriteLine("Status = Not Responding");
            }
          }
        }
        while (!myProcess.WaitForExit(1000));

        Console.WriteLine("Process exit code: {0}", myProcess.ExitCode);

        // Display peak memory statistics for the process.
        Console.WriteLine("Peak physical memory usage of the process: {0}", peakWorkingSet);
        Console.WriteLine("Peak paged    memory usage of the process: {0}", peakPagedMem);
        Console.WriteLine("Peak virtual  memory usage of the process: {0}", peakVirtualMem);
      }
      finally
      {
        //if (myProcess != null)
        //{
        //  myProcess.Close ( );
        //}
      }
    }

    //string cpu()
    //  {
    //    var rv = "...";
    //    var pdi = ProcessDiagnosticInfo.GetForCurrentProcess();
    //    var cpu = pdi.CpuUsage.GetReport().UserTime;
    //    var now = DateTimeOffset.Now;
    //    var pst = now - pdi.ProcessStartTime;
    //    if (pst == TimeSpan.Zero) return "now == prcs start";

    //    if (_prevt > DateTimeOffset.MinValue)
    //    {
    //      var t = now - _prevt;
    //      var u = cpu - _prevu;
    //      if (t.TotalMilliseconds > 0)
    //        rv = $"{(100d * u.TotalMilliseconds / t.TotalMilliseconds),6:N1} / {(100d * cpu.TotalMilliseconds / pst.TotalMilliseconds),6:N1} ";
    //    }

    //    _prevt = now;
    //    _prevu = cpu;

    //    return rv;
    //  }
  }
}
