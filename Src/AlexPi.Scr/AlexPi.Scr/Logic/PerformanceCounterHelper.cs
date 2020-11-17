using System;
using System.Diagnostics;
using System.Linq;
using AAV.Sys.Ext;
using AsLink;

namespace AlexPi.Scr.Logic
{
    public class PerformanceCounterHelper
    {
        public static void L1()
        {
            while (true)
            {
                try
                {
                    foreach (var category in PerformanceCounterCategory.GetCategories()
                        .Where(r =>
                        r.CategoryName.ToLower().Contains("her")
                        //&& r.CategoryName.Equals("GPU Engine")
                        //&& !r.CategoryName.Equals("GPU Process Memory")
                        )
                        )
                    {
                        Debug.WriteLine($"{category.CategoryName,-64}=> {category.GetInstanceNames().Count()}:"); // continue;
                        foreach (var instanceName in category.GetInstanceNames()
                            //.Where(r => r.ToLower().Contains("her"))
                            )
                        {
                            try
                            {
                                Debug.WriteLine($"    {instanceName,62} -> {category.GetCounters(instanceName).Count(),4}:");
                                foreach (var counter in category.GetCounters(instanceName))
                                {
                                    var pc = new PerformanceCounter(category.CategoryName, counter.CounterName, instanceName).NextValue();
                                    if (pc != 0)
                                        Debug.WriteLine($"        {counter.CounterName,-60} = {pc,18:N0}");
                                }
                            }
                            catch (Exception ex) { ex.Log(); }
                        }
                    }
                }
                catch (Exception ex) { ex.Log(); }

                System.Threading.Thread.Sleep(333); //                if (Debugger.IsAttached) Debugger.Break();
                Debug.WriteLine($"\r\n\n\n");
            }
        }
    }
}
