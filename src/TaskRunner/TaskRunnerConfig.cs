using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace RollupTaskRunner
{
    class TaskRunnerConfig : ITaskRunnerConfig
    {
        private ITaskRunnerNode _hierarchy;
        private ImageSource _icon;

        public TaskRunnerConfig(ITaskRunnerNode hierarchy, ImageSource icon)
        {
            _hierarchy = hierarchy;
            _icon = icon;
        }

        public ImageSource Icon
        {
            get { return _icon; }
        }

        public ITaskRunnerNode TaskHierarchy
        {
            get { return _hierarchy; }
        }

        public string LoadBindings(string configPath)
        {
            string bindingPath = configPath + ".bindings";


            if (File.Exists(bindingPath))
            {
                foreach (var line in File.ReadAllLines(bindingPath))
                {
                    if (line.StartsWith("///<binding"))
                        return line.TrimStart('/').Trim();
                }
            }

            return "<binding />";
        }

        public bool SaveBindings(string configPath, string bindingsXml)
        {
            string bindingPath = configPath + ".bindings";

            try
            {
                var sb = new StringBuilder();

                if (File.Exists(bindingPath))
                {
                    var lines = File.ReadAllLines(bindingPath);

                    foreach (var line in lines)
                    {
                        if (!line.TrimStart().StartsWith("///<binding", StringComparison.OrdinalIgnoreCase))
                            sb.AppendLine(line);
                    }
                }

                if (bindingsXml != "<binding />")
                    sb.Insert(0, "///" + bindingsXml);

                ProjectHelpers.CheckFileOutOfSourceControl(bindingPath);

                if (sb.Length == 0)
                {
                    ProjectHelpers.DeleteFileFromProject(bindingPath);
                }
                else
                {
                    File.WriteAllText(bindingPath, sb.ToString(), Encoding.UTF8);
                    ProjectHelpers.AddNestedFile(configPath, bindingPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        public void Dispose()
        {
            _icon = null;
            _hierarchy = null;
        }
    }
}
