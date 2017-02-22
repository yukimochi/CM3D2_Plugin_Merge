﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace cm3d2_plugin_merge
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool P_R_Selected = false, E_R_Selected = false;
        Windows.Storage.StorageFolder Plugin_Root;
        Windows.Storage.StorageFolder Export_Root;
        List<Lst_File> Lst_Backet;
        Dictionary<string, CM3D2_PluginFile> File_Backet;
        ResourceLoader View = ResourceLoader.GetForCurrentView();

        public MainPage()
        {
            bool result = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryResizeView(new Size { Width = 1280, Height = 720 });
            this.InitializeComponent();
        }

        private async void Set_Import_Dir_Click(object sender, RoutedEventArgs e)
        {
            this.Folder_Path.Text = "";
            this.Package_List.Items.Clear();
            this.File_List.Items.Clear();

            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".");

            Plugin_Root = await picker.PickSingleFolderAsync();

            if (Plugin_Root != null)
            {
                this.Folder_Path.Text = Plugin_Root.Path;

                Lst_Backet = new List<Lst_File>();
                var Plugin_Folder = await Storage_Control.Get_Item(Plugin_Root);
                for (int i = 0; i < Plugin_Folder.Count; i++)
                {
                    Lst_Backet.Add(new Lst_File(Plugin_Folder[i]));
                    await Lst_Backet.Last().Get_update_lst();
                }
                foreach (var lst in Lst_Backet)
                {
                    this.Package_List.Items.Add(lst);
                }

                File_Backet = new Dictionary<string, CM3D2_PluginFile>();
                foreach (var Lst in Lst_Backet)
                {
                    foreach (var File in Lst.File_List)
                    {
                        if (File_Backet.ContainsKey(File.Path))
                        {
                            File_Backet[File.Path].File_Add(File);
                        }
                        else
                        {
                            File_Backet.Add(File.Path, new CM3D2_PluginFile(File.Path, File));
                            this.File_List.Items.Add(File_Backet[File.Path]);
                        }
                    }
                }
                this.Status_Bar.Text = Lst_Backet.Count.ToString() + View.GetString("PluginLoaded");
                P_R_Selected = true;
                if (E_R_Selected)
                    this.Do_Merge.IsEnabled = true;
            }
            else
            {
                this.Status_Bar.Text = View.GetString("PluginFolderCenceled");
                P_R_Selected = false;
                this.Do_Merge.IsEnabled = false;
            }
        }

        private async void Set_Export_Dir_Click(object sender, RoutedEventArgs e)
        {
            this.Export_Path.Text = "";

            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".");

            Export_Root = await picker.PickSingleFolderAsync();

            if (Export_Root != null)
            {
                this.Export_Path.Text = Export_Root.Path;
                E_R_Selected = true;
                if (P_R_Selected)
                    this.Do_Merge.IsEnabled = true;
            }
            else
            {
                this.Status_Bar.Text = View.GetString("ExportFolderCanceled");
                E_R_Selected = false;
                this.Do_Merge.IsEnabled = false;
            }
        }

        private async void Newest_Files_Click(object sender, RoutedEventArgs e)
        {
            Prog.Value = 0;
            Prog.Maximum = File_Backet.Count;
            string update_lst = "";
            this.Status_Bar.Text = View.GetString("CreateExportDir");
            var installed = await Export_Root.CreateFolderAsync("marged_plugin", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var data_dir = await installed.CreateFolderAsync("data", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var Sorted_List = new SortedDictionary<string, CM3D2_PluginFile>(File_Backet);
            foreach (var item in Sorted_List)
            {
                var branch = item.Value.File_Name.Split('\\');
                string dirstr = "";
                for (int i = 0; i < branch.Count() - 1; i++)
                {
                    dirstr += branch[i] + "\\";
                }
                this.Status_Bar.Text = View.GetString("Copying") + " : " + item.Key;
                var dir = await Storage_Control.Get_Folder(dirstr, data_dir);
                await (await item.Value.File_List.First().SourceFile()).CopyAsync(dir);
                update_lst += item.Value.File_List.First().Lst_Line + Environment.NewLine;
                Prog.Value++;
            }
            await Storage_Control.Place_readme(installed);
            await Storage_Control.Save_Log(installed, "update.lst", update_lst);
            this.Status_Bar.Text = View.GetString("CreatedUpdatelst");

            var message = new MessageDialog(View.GetString("FinishMsg") ,View.GetString("FinishMsgTitle"));
            await message.ShowAsync();
        }

        private void Package_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Package_Path.Text = ""; this.Package_Version.Text = "";
            if (Package_List.SelectedItem != null)
            {
                var Selected = (Lst_File)Package_List.SelectedItem;
                this.Package_Name.Text = View.GetString("PluginNameCS") + " : " + Selected.Plugin_Name;
                foreach (var Package in Selected.File_List)
                {
                    this.Package_Path.Text += Package.Path + Environment.NewLine;
                    this.Package_Version.Text += Package.Version + Environment.NewLine;
                }
            }

        }

        private async void CopyLight_Click(object sender, RoutedEventArgs e)
        {
            var message = new MessageDialog(View.GetString("Copylight"), "CM3D2 Plugin Marge Tool");
            await message.ShowAsync();
        }

        private void File_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.File_Version.Text = ""; this.File_Parent.Text = "";
            if (File_List.SelectedItem != null)
            {
                var Selected = (CM3D2_PluginFile)File_List.SelectedItem;
                this.File_Name.Text = View.GetString("FileNameCS") + " : " + Selected.File_Name;
                foreach (var File in Selected.File_List)
                {
                    this.File_Version.Text += File.Version + Environment.NewLine;
                    this.File_Parent.Text += File.Parent + Environment.NewLine;
                }
            }
        }
    }
}
