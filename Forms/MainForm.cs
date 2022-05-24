using FileManager.DTO;
using FileManager.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FileManager
{

    public partial class MainForm : Form
    {
        TreeNode _currentPath;
        int _numberToCompute = 0;
        List<double> _sizes = new();
        int _globalStep;
        int _highestPercentageReached = 0;
        int _sortColumn = -1;

        public MainForm()
        {
            InitializeComponent();
            InitDisplay();
        }

        public void InitDisplay()
        {
            treeView.TabStop = false;
            
            backgroundWorker1.WorkerReportsProgress = true;
            
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            
            label1.Visible = false;

            FillDriveNodes();
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(backgroundWorker1_DoWork);
            
            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            
            backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DirectoryInfo dir = new(_currentPath.Tag.ToString());
                DirectoryInfo[] subDirs = dir.GetDirectories();
                _numberToCompute = subDirs.Length;
                int step = 0;
                _sizes = new();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    double size = 0;
                    _sizes.Add(DirectorySizeCalculationService.SizeOfDirectory(dirInfo.FullName, ref size));
                    step++;

                    int progressPercentage = Convert.ToInt32(((double)step / _numberToCompute) * 100);
                    _globalStep = step;

                    if (progressPercentage > _highestPercentageReached)
                    {
                        _highestPercentageReached = progressPercentage;
                        (sender as BackgroundWorker).ReportProgress(progressPercentage);
                    }
                }
            }
            catch
            {
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 100)
                progressBar1.Value = 100;
            else
                progressBar1.Value = e.ProgressPercentage;
            
            if (progressBar1.Value == 100)
                label1.Text = "Загружаем данные...";
            else
                label1.Text = "Подождите, выполняется анализ " + progressBar1.Value.ToString() + "%";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowFilesAndDirs(_sizes, _currentPath.Tag.ToString());
            progressBar1.Value = 0;
            label1.Visible = false;
            _globalStep = 0;
            progressBar1.Visible = false;
            treeView.Enabled = true;
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            label1.Visible = true;
            listView.Items.Clear();
            _currentPath = e.Node;
            _highestPercentageReached = 0;
            treeView.Enabled = false;
            
            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            progressBar1.Visible = true;
            treeView.SelectedNode = null;
        }

        private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            e.Node.Expand();
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            LoadChildNodes(e.Node);
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView.Sorting == SortOrder.Ascending)
                    listView.Sorting = SortOrder.Descending;
                else
                    listView.Sorting = SortOrder.Ascending;
            }
            listView.Sort();
            listView.ListViewItemSorter = new ListViewItemComparerService(e.Column, listView.Sorting);
        }

        private void FillDriveNodes()
        {
            try
            {
                treeView.Nodes.Clear();
                TreeNode driveNode = new();
                string systemDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).Root.FullName;
                DriveInfo systemDrive = new(systemDir);

                driveNode = treeView.Nodes.Add("Системный диск" + "(" + systemDrive.Name.Split('\\')[0] + ")");
                driveNode.Tag = systemDrive.Name;
                driveNode.ImageIndex = (int)IconsIndexes.SystemDisk;
                driveNode.SelectedImageIndex = (int)IconsIndexes.SystemDisk;

                foreach (var drive in DriveInfo.GetDrives())
                {
                    driveNode = new();
                    
                    switch (drive.DriveType)
                    {
                        case DriveType.Fixed:
                            {
                                if (!drive.Name.Equals(systemDrive.Name))
                                {
                                    var volumeLabel = drive.VolumeLabel.Equals("") ? "Локальный диск" : drive.VolumeLabel;
                                    driveNode = treeView.Nodes.Add(volumeLabel + "(" + drive.Name.Split('\\')[0] + ")");
                                    driveNode.Tag = drive.Name;
                                    driveNode.ImageIndex = (int)IconsIndexes.Disk;
                                }
                                break;
                            }
                        case DriveType.CDRom:
                            {
                                driveNode = treeView.Nodes.Add("Дисковод (" + drive.Name.Split('\\')[0] + ")");
                                driveNode.Tag = drive.Name;
                                driveNode.ImageIndex = (int)IconsIndexes.Disk;
                                break;
                            }
                        case DriveType.Removable:
                            {
                                driveNode = treeView.Nodes.Add("USB (" + drive.Name.Split('\\')[0] + ")");
                                driveNode.Tag = drive.Name;
                                driveNode.ImageIndex = (int)IconsIndexes.Disk;
                                break;
                            }
                        default:
                            {
                                driveNode = treeView.Nodes.Add("Неизвестное устройство (" + drive.Name.Split('\\')[0] + ")");
                                driveNode.Tag = drive.Name;
                                driveNode.ImageIndex = (int)IconsIndexes.Disk;
                                break;
                            }
                    }
                }

                foreach (TreeNode node in treeView.Nodes)
                {
                    LoadChildNodes(node);
                }
            }
            catch
            {
            }
        }

        private static void LoadChildNodes(TreeNode driveNode)
        {
            try
            {
                driveNode.Nodes.Clear();
                DirectoryInfo driveName = new (driveNode.Tag.ToString());
                DirectoryInfo[] subDirs = driveName.GetDirectories();
                
                foreach (var dir in subDirs)
                {
                    TreeNode childNode = driveNode.Nodes.Add(dir.Name);
                    childNode.Tag = dir.FullName;
                    childNode.ImageIndex = (int)IconsIndexes.Folder;
                    childNode.SelectedImageIndex = (int)IconsIndexes.Folder;
                    childNode.Nodes.Add("");
                }
            }
            catch
            {
            }
        }

        private void ShowFilesAndDirs(List<double> subDirsWithSize, string path)
        {
            try
            {
                listView.Items.Clear();

                DirectoryInfo dir = new(path);
                DirectoryInfo[] subDirs = dir.GetDirectories();
                FileInfo[] files = dir.GetFiles();

                int index = 0;
                foreach (var dirInfo in subDirs)
                {

                    ListViewItem item = listView.Items.Add(dirInfo.Name, (int)IconsIndexes.Folder);
                    item.Tag = dirInfo.FullName;
                    item.SubItems.Add("Папка с файлами");
                    double size = subDirsWithSize.ElementAt(index);
                    var sizeDto = DirectorySizeCalculationService.DefineMeasurement(size);
                    item.SubItems.Add(((int)sizeDto.Size).ToString() + $" {sizeDto.Name}");
                    index++;
                }
                
                foreach (var file in files)
                {
                    ListViewItem item = listView.Items.Add(file.Name, (int)IconsIndexes.File);
                    item.Tag = file.FullName;
                    item.SubItems.Add(file.Extension);
                    var sizeDto = DirectorySizeCalculationService.DefineMeasurement(file.Length);
                    item.SubItems.Add((int)sizeDto.Size + $" {sizeDto.Name}");
                }
            }
            catch
            {
            }
        }
    }
}
