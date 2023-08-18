using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Web;
using System.Threading.Tasks;
using WebApplication5.Models;

namespace WebApplication5.Hubs
{
    public class MyHub : Hub
    {
        static int CountUsers { get; set; }
        static  PerformanceCounter cpuCounter { get; set; } = new PerformanceCounter("Processor", "% Processor Time", "_Total");
       
        static DateTime begintime { get; set; }

        static Dictionary<string, List<int>> memory { get; set; } = new Dictionary<string, List<int>>();
        static Dictionary<string, List<int>> cpu { get; set; } = new Dictionary<string, List<int>>();
        static Loading nowloading { get; set; }
        static int   nowmemo { get; set; }
        static int nowcpu { get; set; }

        static LoadingContext context = new LoadingContext();

        public async Task GetMainLoadings(string type)
        {

            Dictionary<string, List<int>> loadings = await MyHub.context.GetMainLoadings();
            Clients.Caller.showloadings(loadings  , type );
        }
        public async Task RequestGraphPoints(int id, string type , int leftborder, int rightborder)
        {

            Dictionary<string, List<int>> points = await MyHub.context.GetChanges(id, type, leftborder, rightborder);
            Clients.Caller.updatepoints(points);
        }
        public async Task Load()
        {
            while (MyHub.CountUsers > 0)
            {
                
                Random rnd = new Random();
                DateTime date = DateTime.Now;
                MyHub.nowcpu = Convert.ToInt32(MyHub.cpuCounter.NextValue());
                MyHub.nowmemo = this.GetValue("Memory", "Available MBytes");
                await MyHub.context.AddChange(new Change
                {
                    Percent = MyHub.nowcpu,
                    Loading = MyHub.nowloading,
                    Type = "CPU",
                    Seconds = Convert.ToInt32((date - MyHub.begintime).TotalSeconds)
                });
                await MyHub.context.AddChange(new Change
                {
                    Percent = MyHub.nowmemo,
                    Loading = MyHub.nowloading,
                    Type = "Memory",
                    Seconds = Convert.ToInt32((date - MyHub.begintime).TotalSeconds)
                });
                MyHub.cpu["X"].Add(Convert.ToInt32((date - MyHub.begintime).TotalSeconds));
                MyHub.memory["X"].Add(Convert.ToInt32((date - MyHub.begintime).TotalSeconds));
                MyHub.cpu["Y"].Add(MyHub.nowcpu);
                MyHub.memory["Y"].Add(MyHub.nowmemo);

                await context.ChangeLoadTime(MyHub.nowloading.Id, Convert.ToInt32((date - MyHub.begintime).TotalSeconds));
                await Task.Delay(1000);
            }
            

        }
        public int GetValue(string category, string counter, string instance )
        {
            using (PerformanceCounter pc =
          new PerformanceCounter(category, counter, instance))
               return Convert.ToInt32(pc.NextValue());
        }
        public int GetValue(string category, string counter)
        {
            using (PerformanceCounter pc =
          new PerformanceCounter(category, counter))
                return Convert.ToInt32(pc.NextValue());
        }
        public void Printvalues()
        {
           
            Clients.Caller.printPercent(MyHub.nowcpu, MyHub.nowmemo, MyHub.cpu, MyHub.memory);

        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            MyHub.CountUsers -= 1;

            return base.OnDisconnected(stopCalled);
        }
        public  async Task Upload()
        {

            MyHub.CountUsers += 1;
            int newnumb = await MyHub.context.GetCountOfChanges() + 1;

            if (MyHub.CountUsers != 1) {
                Clients.Caller.begindraw();
                return; 
            }
            
            MyHub.nowcpu = Convert.ToInt32( MyHub.cpuCounter.NextValue());
            MyHub.nowmemo = this.GetValue("Memory", "Available MBytes");
            MyHub.cpu["X"] = new List<int> { 0 };
            MyHub.memory["X"] = new List<int> { 0 };
            MyHub.cpu["Y"] = new List<int> { MyHub.nowcpu };
            MyHub.memory["Y"] = new List<int> { MyHub.nowmemo };
            MyHub.begintime = DateTime.Now;
            MyHub.nowloading = await MyHub.context.AddLoading(new Loading {Number = newnumb });
            Clients.Caller.begindraw();
            await this.Load();


        }
    }
}