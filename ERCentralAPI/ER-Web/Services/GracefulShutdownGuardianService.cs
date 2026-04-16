using BusinessLibrary.Data;
using System.Data.SqlClient;

namespace ER_Web.Services
{
    /// <summary>
    /// This service has one role: to keep ER alive while long-lasting-tasks are shutting down. This is done via two mechanisms:
    /// 1. Makes Program.CancelToken.IsCancellationRequested flip to true, so that fire-and-forget tasks in LongLastingFireAndForgetCommands can notice and stop gracefully
    /// 2. Monitor TB_REVIEW_JOB for fired and forgotten tasks - GracefulShutdownGuardianService will only finish "stopping" when either:
    /// 2.1 No review_jobs report themselves as "running" in TB_REVIEW_JOB (i.e. SUCCESS field is null)
    /// 2.2 28 seconds passed since we first checked for running jobs.
    /// IOW, GracefulShutdownGuardianService is supposed to stop only when everything else has stopped gracefully, or when it's too late and we're giving up.
    /// </summary>
    internal class GracefulShutdownGuardianService : IHostedService, IDisposable
    {
        //private Timer _timer;
        private readonly int ID = new Random().Next();
        private readonly Microsoft.Extensions.Hosting.IHostApplicationLifetime _appLifetime;
        private readonly Serilog.ILogger Logger;

        public GracefulShutdownGuardianService(Microsoft.Extensions.Hosting.IHostApplicationLifetime appLifetime, Serilog.ILogger logger)
        {
            _appLifetime = appLifetime;
            Logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ff") + " Timed Background Service is starting, with ID: " + ID);
            Logger.Information("GracefulShutdownGuardianService is starting, with ID: " + ID);
            //Logger.Information("CT ID: " + cancellationToken.GetHashCode().ToString());
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
            //_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            //Task.Run(() =>
            //{
            //    Thread.Sleep(30000);
            //    Program.AppIsShuttingDown = false;
            //});
            return Task.CompletedTask;
        }
        //private void DoWork(object? state)
        //{
        //    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ff") + " Timed Background Service is working, with ID: " + ID);
        //    Logger.Information(DateTime.Now.ToString("HH:mm:ss:ff") + " Timed Background Service is working, with ID: " + ID);
        //}
        public Task StopAsync(CancellationToken cancellationToken)
        {
            //Program.TokenSource.Cancel();
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ff") + " Timed Background Service is stopping, with ID: " + ID);
            //Logger.Information("CT ID: " + cancellationToken.GetHashCode().ToString() + " please make sense! " + cancellationToken.IsCancellationRequested.ToString());
            Logger.Information("GracefulShutdownGuardianService is stopping, with ID: " + ID);
            bool checkResult = CheckForRunningReviewJobs();
            if (checkResult) Logger.Information("GracefulShutdownGuardianService is stopping and has checked for running jobs");
            else Logger.Information("GracefulShutdownGuardianService is stopping after FAILING to check for running jobs");
            //_timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        private bool CheckForRunningReviewJobs()
        {
            Logger.Information("GracefulShutdownGuardianService is Checking ForRunningReviewJobs");
            bool ActiveJobsExist = true;
            DateTime GiveUpTime = DateTime.Now.AddSeconds(28);
            string HowFarBack = "'" + GiveUpTime.AddMinutes(-15).ToString("yyyy-MM-dd HH:mm:ss") + "'";
            try
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    string SQLcmd = "SELECT * from TB_REVIEW_JOB where SUCCESS is null AND END_TIME > " + HowFarBack;
                    while (ActiveJobsExist == true)
                    {
                        using (SqlCommand command = new SqlCommand(SQLcmd, connection))
                        {
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                if (!reader.Read())
                                {
                                    ActiveJobsExist = false;
                                    Logger.Information("GracefulShutdownGuardianService found that no job is running - graceful shutdown is OK");
                                    return true;
                                }
                                else
                                {
                                    Logger.Information("GracefulShutdownGuardianService found that some job is running - DELAYING shutdown");
                                    Thread.Sleep(500);
                                    if (DateTime.Now >= GiveUpTime)
                                    {
                                        Logger.Error("GracefulShutdownGuardianService ran out of time - running jobs will be KILLED");
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GracefulShutdownGuardianService error in CheckForRunningReviewJobs");
                return false;
            }
            return true;
        }
        public void Dispose()
        {
            //_timer?.Dispose();
        }
        private void OnStarted()
        {
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ff") + " Timed Background Srv OnStarted(), with ID: " + ID);
            Logger.Information("GracefulShutdownGuardianService OnStarted(), with ID: " + ID);
        }
        private void OnStopping()
        {
            Program.TokenSource.Cancel(); //we do this here, which ensures fire-and-forget tasks will be notified immediately
                                          //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ff") + " Timed Background Srv OnStopping(), with ID: " + ID);
            Logger.Information("GracefulShutdownGuardianService OnStopping(), with ID: " + ID);
        }
        private void OnStopped()
        {
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ff") + " Timed Background Srv OnStopped(), with ID: " + ID);
            Logger.Information("GracefulShutdownGuardianService OnStopped(), with ID: " + ID);
        }
    }

}
