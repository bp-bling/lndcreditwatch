using lndapi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lndcreditwatch
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
            if (Debugger.IsAttached) Console.ReadKey();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine(" lnd credit watch starting up");

            if (DateTime.Now.Hour == 0)
            {
                Console.WriteLine(" midnight hour, alerting previous day's usage");
                string DailyUsageFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dailyusage.txt");
                if (File.Exists(DailyUsageFilename))
                {
                    // Alert daily usage
                    string[] DailyUsage = File.ReadAllLines(DailyUsageFilename);
                    // TODO Send an email with DailyUsage

                    // And reset
                    File.Delete(DailyUsageFilename);
                }
            }

            Console.WriteLine($" acceptable cost per interval: ${Config.Default.AcceptableCostPerInterval}");

            using (LNDynamic client = new LNDynamic(Config.Default.APIID.GetPlainText(), Config.Default.APIKey.GetPlainText()))
            {
                // Get current credit balance
                double CurrentCreditBalance = await client.BillingCreditAsync();
                Console.WriteLine($" current credit balance: ${CurrentCreditBalance}");

                // Compare to previous credit balance
                if (Config.Default.PreviousCreditBalance > double.MinValue)
                {
                    Console.WriteLine($" previous credit balance: ${Config.Default.PreviousCreditBalance}");

                    double CostForThisInterval = Config.Default.PreviousCreditBalance - CurrentCreditBalance;
                    Console.WriteLine($" cost for this interval: ${CostForThisInterval}");

                    if (CostForThisInterval > Config.Default.AcceptableCostPerInterval)
                    {
                        // We spent too much during this last interval!
                        Console.WriteLine($" OVERSPEND by ${CostForThisInterval - Config.Default.AcceptableCostPerInterval}");
                        Console.WriteLine($" in other words, {(CostForThisInterval - Config.Default.AcceptableCostPerInterval) / Config.Default.AcceptableCostPerInterval:P} extra was spent");
                        // TODO Send an eail alert
                    }
                    else
                    {
                        // We were within our spending limit
                        Console.WriteLine($" UNDERSPEND by ${Config.Default.AcceptableCostPerInterval - CostForThisInterval}");
                        Console.WriteLine($" in other words, {(Config.Default.AcceptableCostPerInterval - CostForThisInterval) / Config.Default.AcceptableCostPerInterval:P} was saved");
                    }
                }
                else
                {
                    // No previous balance, so send an email alert saying watch has been setup
                    Console.WriteLine(" previous credit balance: N/A (looks like this is our first run)");
                    // TODO Send an email alert
                }

                // Store current credit as previous credit
                Config.Default.PreviousCreditBalance = CurrentCreditBalance;
                Config.Default.Save();

                Console.WriteLine(" done");
            }
        }
    }
}
