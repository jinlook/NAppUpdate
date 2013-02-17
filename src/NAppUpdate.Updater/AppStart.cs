using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AppUpdate;
using AppUpdate.Common;
using AppUpdate.Tasks;
using AppUpdate.Utils;

namespace NAppUpdate.Updater
{
    internal static class AppStart
    {
        private static ArgumentsParser _args;

        private static void Main()
        {
            string tempFolder = string.Empty;
            string logFile = string.Empty;
            _args = ArgumentsParser.Get();
            _args.ParseCommandLineArgs();

            var workingDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            try
            {
                // Get the update process name, to be used to create a named pipe and to wait on the application
                // to quit
                string syncProcessName = _args.ProcessName;
                if (string.IsNullOrEmpty(syncProcessName)) //Application.Exit();
                    throw new ArgumentException("The command line needs to specify the mutex of the program to update.", "ar" + "gs");

                // Load extra assemblies to the app domain, if present
                var availableAssemblies = FileSystem.GetFiles(workingDir, "*.exe|*.dll", SearchOption.TopDirectoryOnly);
                foreach (var assemblyPath in availableAssemblies)
                {
                    if (assemblyPath.Equals(System.Reflection.Assembly.GetEntryAssembly().Location, StringComparison.InvariantCultureIgnoreCase)
                        || assemblyPath.EndsWith("AppUpdate.dll"))
                    {
                        continue;
                    }

                    try
                    {
                        var assembly = System.Reflection.Assembly.LoadFile(assemblyPath);
                    }
                    catch (System.BadImageFormatException ex)
                    {
                    }
                }

                // Connect to the named pipe and retrieve the updates list
                var dto = NauIpc.ReadDto(syncProcessName) as NauIpc.NauDto;

                // Make sure we start updating only once the application has completely terminated
                Thread.Sleep(1000); // hell, let's even wait a bit
                bool createdNew;
                using (var mutex = new Mutex(false, syncProcessName + "Mutex", out createdNew))
                {
                    try
                    {
                        if (!createdNew) mutex.WaitOne();
                    }
                    catch (AbandonedMutexException)
                    {
                        // An abandoned mutex is exactly what we are expecting...
                    }
                    finally
                    {
                    }
                }

                bool updateSuccessful = true;

                if (dto == null || dto.Configs == null) throw new Exception("Invalid DTO received");

                if (dto.LogItems != null) // shouldn't really happen
                {
                }

                // Get some required environment variables
                string appPath = dto.AppPath;
                string appDir = dto.WorkingDirectory ?? Path.GetDirectoryName(appPath) ?? string.Empty;
                tempFolder = dto.Configs.TempFolder;
                string backupFolder = dto.Configs.BackupFolder;
                bool relaunchApp = dto.RelaunchApplication;

                if (dto.Tasks == null || dto.Tasks.Count == 0)
                    throw new Exception("Could not find the updates list (or it was empty).");

                //This can be handy if you're trying to debug the updater.exe!
                //#if (DEBUG)
                //{  
                //                if (_args.ShowConsole) {
                //                    _console.WriteLine();
                //                    _console.WriteLine("Pausing to attach debugger.  Press any key to continue.");
                //                    _console.ReadKey();
                //                }

                //}
                //#endif

                // Perform the actual off-line update process
                foreach (var t in dto.Tasks)
                {
                    if (t.ExecutionStatus != TaskExecutionStatus.RequiresAppRestart
                        && t.ExecutionStatus != TaskExecutionStatus.RequiresPrivilegedAppRestart)
                    {
                        continue;
                    }

                    // TODO: Better handling on failure: logging, rollbacks
                    try
                    {
                        t.ExecutionStatus = t.Execute(true);
                    }
                    catch (Exception ex)
                    {
                        updateSuccessful = false;
                        t.ExecutionStatus = TaskExecutionStatus.Failed;
                    }

                    if (t.ExecutionStatus == TaskExecutionStatus.Successful) continue;
                    updateSuccessful = false;
                    break;
                }

                if (updateSuccessful)
                {
                    if (Directory.Exists(backupFolder))
                        FileSystem.DeleteDirectory(backupFolder);
                }
                else
                {
                    if (Directory.Exists(backupFolder))
                    {
                        foreach (string backFiles in Directory.GetFiles(backupFolder, "*.*", SearchOption.AllDirectories))
                        {
                            File.Copy(backFiles, backFiles.Replace(backupFolder, appDir));
                        }
                        FileSystem.DeleteDirectory(backupFolder);
                    }
                    MessageBox.Show("更新失败");
                }

                // Start the application only if requested to do so
                if (relaunchApp)
                {
                    var info = new ProcessStartInfo
                                {
                                    UseShellExecute = true,
                                    WorkingDirectory = appDir,
                                    FileName = appPath,
                                };

                    var p = NauIpc.LaunchProcessAndSendDto(dto, info, syncProcessName);
                    if (p == null) throw new UpdateProcessFailedException("Unable to relaunch application and/or send DTO");
                }
            }
            catch (Exception ex)
            {
                // supressing catch because if at any point we get an error the update has failed
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFolder)) SelfCleanUp(tempFolder);
                Application.Exit();
            }
        }

        private static void SelfCleanUp(string tempFolder)
        {
            // Delete the updater EXE and the temp folder
            try
            {
                var info = new ProcessStartInfo
                            {
                                Arguments =
                                    string.Format(@"/C ping 1.1.1.1 -n 1 -w 3000 > Nul & echo Y|del ""{0}\*.*"" & rmdir ""{0}""",
                                                  tempFolder),
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                                FileName = "cmd.exe"
                            };

                Process.Start(info);
            }
            catch
            {
                /* ignore exceptions thrown while trying to clean up */
            }
        }
    }
}