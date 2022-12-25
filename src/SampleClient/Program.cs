

namespace SampleClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var template = new Templates.Hello();
                template.Model = Mock.MakePets();
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
