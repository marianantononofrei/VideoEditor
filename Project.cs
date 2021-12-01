using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace VideoEditor
{
    public class Project
    {
        public string projectsPath = Constants.exePath + @"\projects\";
        public string projectName = "";
        public string projectDirectory = "";

        Dictionary<int, Item> projectContent { get; set; }

        public Project(bool firstProject = true)
        {
            if (!Directory.Exists(projectsPath))
            {
                Directory.CreateDirectory(projectsPath);
            }
            var files = Directory.GetFiles(projectsPath, "*.json").ToList();
            if (files.Count == 0)
            {
                var result = CreateNewProject();
                while (result == "")
                {
                    result = CreateNewProject();
                }
                files.Add(result);
            }
            if (!firstProject)
            {
                var result = CreateNewProject();
                if (result != "")
                {
                    files.Add(result);
                }
                else
                {
                    return;
                }
            }
            var minFA = files.Min(x => File.GetLastWriteTime(x));
            var file = files.First(x => File.GetLastWriteTime(x) == minFA);
            projectName = VideoProcessing.FileNameWithoutExtension(Path.GetFileName(file));
            if (projectDirectory == "")
            {
                BuildProjectDirectory();
            }
            LoadProject();
        }
        private string CreateNewProject()
        {
            CreateNewProject np = new CreateNewProject();
            np.ShowDialog();
            while (np.DialogResult != System.Windows.Forms.DialogResult.OK)
            {
                if (np.DialogResult == System.Windows.Forms.DialogResult.No || np.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return "";
                }
                np.ShowDialog();
            }
            projectName = np.textBox1.Text;
            projectName = VideoProcessing.FileNameWithoutExtension(projectName);
            BuildProjectDirectory();
            projectContent = new Dictionary<int, Item>();
            SaveProject();
            return (projectsPath + projectName);
        }
        public Project(string _projectName)
        {
            projectName = VideoProcessing.FileNameWithoutExtension(Path.GetFileName(_projectName));
            LoadProject();
            BuildProjectDirectory();
        }
        void BuildProjectDirectory()
        {
            projectDirectory = projectsPath + projectName + @"\";
        }
        void LoadProject()
        {
            var projectFinalPath = projectsPath + projectName + ".json";
            projectContent = Json.LoadFromFile<Dictionary<int, Item>>(projectFinalPath);
        }
        void SaveProject()
        {
            var projectFinalPath = projectsPath + projectName + ".json";
            Json.WriteToFile(projectContent, projectFinalPath);
        }
    }
}
