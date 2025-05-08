using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReceiptorCZCOICOP.Db;
using ReceiptorCZCOICOP.Services.ClassificationServices;
using ReceiptorCZCOICOP.Services.DataExportServices;
using ReceiptorCZCOICOP.Services.DataExtractionServices;
using ReceiptorCZCOICOP.Services.OcrServices;
using ReceiptorCZCOICOP.ViewModels;
using System.IO;
using System.Windows;

namespace ReceiptorCZCOICOP
{
    public partial class App : Application
    {

        private readonly IHost _host;

        /// <summary>
        /// Application entry point.
        /// </summary>
        public App()
        {
            // define host
            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    // remove default providers
                    logging.ClearProviders();
                })
                .ConfigureServices((ctx, services) =>
                {
                    // db (exaple with registered SQLite db)
                    services.AddDbContext<Db.ReceiptDbContext>(opts => opts.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "receipts.db")}"));

                    // services
                    services.AddSingleton<IOcrService, TesseractOcrService>();
                    services.AddSingleton<IDataExtractionService, GroqCloudDataExtractionService>();
                    services.AddSingleton<IClassificationService, MainModelClassificationService>();
                    services.AddSingleton<IDataExportService, CsvDataExportService>();

                    // main view model
                    services.AddSingleton<MainViewModel>();

                    // main window view
                    services.AddSingleton(s => new MainWindow()
                    {
                        DataContext = s.GetRequiredService<MainViewModel>(),
                    });
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _host.Start();

            // create database if not exists (example)
            using var scope = _host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ReceiptDbContext>();
            db.Database.EnsureCreated();

            // show main window view
            var mw = _host.Services.GetRequiredService<MainWindow>();
            mw.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }

}
