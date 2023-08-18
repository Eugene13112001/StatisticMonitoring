using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using System.Data.Entity;

namespace WebApplication5.Models
{
    public class LoadingContext : DbContext
    {
        public DbSet<Change> Changes { get; set; }
        public DbSet<Loading> Loadings { get; set; }

        public async Task AddChange(Change loading)
        {
            this.Changes.Add(loading);
            await this.SaveChangesAsync();
        }
        public async Task<Change> GetLastChange(int loadingid)
        {
          return await this.Changes.Where(p => p.LoadingId == loadingid).OrderByDescending(u => u.Seconds).FirstAsync();
        }
        public async Task ChangeLoadTime(int id, int seconds)
        {
            Loading main =await Loadings.FirstOrDefaultAsync<Loading>(p => p.Id == id );
            main.Seconds = seconds;
           
            await this.SaveChangesAsync();
        }
        public async Task<Loading> AddLoading(Loading loading)
        {
             this.Loadings.Add(loading);
             await this.SaveChangesAsync();
             return loading;
        }
        public async Task<int> GetCountOfChanges()
        {
            int count = await  this.Changes.CountAsync();
            if (count == 0) return 0;
            return await this.Changes.MaxAsync(p => p.Loading.Number);
           
        }
        
        public async Task<Dictionary<string, List<int>>> GetMainLoadings()
        {
            Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();
            var list = this.Loadings.OrderBy(p => p.Number);
            dict["Id"] = await list.Select(p => 
                p.Id 
            ).ToListAsync();
            dict["Number"] = await list.Select(p =>
                p.Number
            ).ToListAsync();
            
            dict["Seconds"] = await list.Select(p =>
                p.Seconds
            ).ToListAsync();
            return dict;
        }
        public async Task<Loading> GetMainLoading(int id)
        {

            return await this.Loadings.FirstOrDefaultAsync<Loading>(p => p.Id == id);
        }
        public async Task<Loading> GetLoadingByNumb(int numb)
        {

            return await this.Loadings.FirstOrDefaultAsync<Loading>(p => p.Number == numb);
        }
        public async Task<Dictionary<string, List<int>>> GetChanges(int id, string type,  int leftborder, int rightborder)
        {

            Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();
            var list =  this.Changes.Where(p => (p.Type == type) && (p.LoadingId == id) && (p.Seconds >= leftborder) && (p.Seconds <= rightborder));      
            dict["X"] = await list.Select(p =>
                p.Seconds
            ).ToListAsync();
            dict["Y"] = await list.Select(p =>
                p.Percent
            ).ToListAsync();
            return dict;
        }

    }
    public class Change
    {
        public int Id { get; set; }
        
        public int Seconds { get; set; }
        public int Percent { get; set; }
        public int LoadingId { get; set; }
        public Loading Loading { get; set; }
        public string Type { get; set; }
    }

    public class Loading
    {
        public int Id { get; set; }

        public List<Change> Changes { get; set; } = new List<Change>();
        public int Number { get; set; }
        public int Seconds { get; set; }
        
    }
   

}