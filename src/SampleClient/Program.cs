

namespace SampleClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var c = new Models.Cls()
                {
                    Name = "Customer"
                };

                c.Properties.Add(new() { Name = "Id", Type = "string" });
                c.Properties.Add(new() { Name = "Name", Type = "string" });
                c.Properties.Add(new() { Name = "Created", Type = "DateTime" });
                c.Properties.Add(new() { Name = "Modified", Type = "DateTime" });

                var template = new Templates.Hello();
                template.Model = c;

                var output = await template.RenderAsync();
                Console.WriteLine(output);
            }
            catch (Exception err) { 
                PrintError(err);  
            }
         }

        static void PrintError(Exception? err)
        {
            while(null != err)
            {
                Console.WriteLine($"[{err.GetType().Name}] {err.Message}");
                err = err.InnerException;
            }
        }
    }
}
