using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace RollupTaskRunner
{
    /// <summary>
    /// This class will be called by Visual Studio automatically when a project
    /// is being opened that contains myconfig.json in the project- or solution root folder.
    /// </summary>
    [TaskRunnerExport("rollup.config.js")]
    public class TaskRunnerProvider : ITaskRunner
    {
        private static ImageSource _icon = new BitmapImage(new Uri(@"pack://application:,,,/RollupTaskRunner;component/Resources/logo.png"));
        private List<ITaskRunnerOption> _options;

        public List<ITaskRunnerOption> Options
        {
            get
            {
                if (_options == null)
                {
                    _options = new List<ITaskRunnerOption>();
                    _options.Add(new TaskRunnerOption("Sourcemap", PackageIds.cmdSourcemap, PackageGuids.guidVSPackageCmdSet, false, "--sourcemap"));
                    _options.Add(new TaskRunnerOption("No Strict", PackageIds.cmdNoStrict, PackageGuids.guidVSPackageCmdSet, false, "--no-strict"));
                    _options.Add(new TaskRunnerOption("No Indentation", PackageIds.cmdNoIndent, PackageGuids.guidVSPackageCmdSet, false, "--no-indent"));
                    _options.Add(new TaskRunnerOption("No Conflict", PackageIds.cmdConflict, PackageGuids.guidVSPackageCmdSet, false, "--no-conflict"));
                }

                return _options;
            }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            return await Task.Run(() =>
            {
                var hierarchy = LoadHierarchy(configPath);
                return new TaskRunnerConfig(hierarchy, _icon);
            });
        }

        /// <summary>
        /// Construct any task hierarchy that you need.
        /// Task Runner Explorer will automatically have node.exe and npm.cmd on the PATH
        /// and you can control that in Tools -> Options -> Projects & Solutions -> External Web Tools
        /// </summary>
        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            string cwd = Path.GetDirectoryName(configPath);
            var configFiles = GetConfigFileNames(cwd);

            var rollup = new TaskRunnerNode("Rollup")
            {
                Description = "Executes the \"rollup\" command",
            };

            foreach (var config in configFiles)
            {
                rollup.Children.Add(new TaskRunnerNode($"Rollup {config}", true)
                {
                    Description = $"Execute the \"rollup\" for the {config} configuration file",
                    Command = new TaskRunnerCommand(cwd, "cmd.exe", $"/c rollup -c {config}")
                });
            }

            var watch = new TaskRunnerNode("Watch")
            {
                Description = "Executes the \"rollup -w\" command",
            };

            foreach (var config in configFiles)
            {
                watch.Children.Add(new TaskRunnerNode($"Watch {config}", true)
                {
                    Description = $"Execute the \"rollup -w\" for the {config} configuration file",
                    Command = new TaskRunnerCommand(cwd, "cmd.exe", $"/c rollup -w -c {config}")
                });
            }

            var root = new TaskRunnerNode(Vsix.Name);
            root.Children.Add(rollup);
            root.Children.Add(watch);

            return root;
        }

        private IEnumerable<string> GetConfigFileNames(string cwd)
        {
            var configFiles = Directory.EnumerateFiles(cwd, "rollup.config.*");

            return configFiles.Select(file => Path.GetFileName(file)).Where(file => Path.GetExtension(file) == ".js");
        }
    }
}
