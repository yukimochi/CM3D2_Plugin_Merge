using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cm3d2_plugin_merge
{
    class Lst_File
    {
        public string Plugin_Name { get; private set; }
        public string Path { get; private set; }
        public Windows.Storage.StorageFile Update_lst { get; private set; }
        public List<CM3D2_File> File_List = new List<CM3D2_File>();

        public Lst_File(Windows.Storage.StorageFolder current, Windows.Storage.StorageFolder root)
        {
            Path = current.Path.Replace(root.Path + "\\", "");

            Plugin_Name = Path.Split('\\').Last();
        }

        public async Task Get_update_lst(Windows.Storage.StorageFolder current,bool IsX64)
        {
            if ((Update_lst = (Windows.Storage.StorageFile)await current.TryGetItemAsync(Path + "\\update.lst")) ==null)
            {
                if (IsX64)
                {
                    Update_lst = (Windows.Storage.StorageFile)await current.TryGetItemAsync(Path + "\\update_x64.lst");
                }
                else
                {
                    Update_lst = (Windows.Storage.StorageFile)await current.TryGetItemAsync(Path + "\\update_x86.lst");
                }
            }
            using (var lst_stream = new StringReader(await Windows.Storage.FileIO.ReadTextAsync(Update_lst)))
            {
                string lst_line;
                while ((lst_line = lst_stream.ReadLine()) != null)
                {
                    File_List.Add(new CM3D2_File(Plugin_Name, lst_line, Path));
                }
            }
        }

    }

    class CM3D2_PluginFile
    {
        public string File_Name { get; private set; }
        public List<CM3D2_File> File_List { get; private set; }

        public CM3D2_PluginFile(string File_Name, CM3D2_File File)
        {
            File_List = new List<CM3D2_File>();
            this.File_Name = File_Name;
            File_List.Add(File);
        }
        public void File_Add(CM3D2_File File)
        {
            File_List.Add(File);
            File_List.Sort();
        }
    }

    class CM3D2_File : IComparable
    {
        public string Lst_Line { get; private set; }
        public string Plg_Path { get; private set; }
        public string Ex_Path { get; private set; }
        public string SourceFolder { get; private set; }
        public int Version { get; private set; }
        public string Parent { get; private set; }

        private bool NeedUp;

        private Windows.Storage.StorageFile _SourceFile;

        public CM3D2_File(string parent, string lst_raw, string folder)
        {
            Parent = parent;
            Lst_Line = lst_raw;
            Ex_Path = lst_raw.Split(',')[2];
            Version = int.Parse(lst_raw.Split(',')[5]);
            SourceFolder = folder;
            if (lst_raw.Split(',')[0] == "share")
            {
                Plg_Path = lst_raw.Split(',')[1];
                if(Plg_Path.Contains("../"))
                {
                    Plg_Path = Plg_Path.Replace("../", "");
                    NeedUp = true;
                }
            }
        }
        public async Task<Windows.Storage.StorageFile> SourceFile(Windows.Storage.StorageFolder current)
        {
            if (Plg_Path == null)
            {
                _SourceFile = await current.GetFileAsync(SourceFolder + "\\data\\" + Ex_Path);
            }
            else
            {
                var Dirs = SourceFolder.Split('\\');
                string New_Path = "";
                if (NeedUp)
                {
                    for (int i = 0; i < Dirs.Count() - 1; i++)
                    {
                        New_Path += Dirs[i] + "\\";
                    }
                }
                else
                {
                    New_Path = SourceFolder + "\\";
                }
                _SourceFile = await current.GetFileAsync(New_Path + Plg_Path);
                var New_Lst = Lst_Line.Split(',');
                Lst_Line = "0,0," + New_Lst[2] + "," + New_Lst[3] + "," + New_Lst[4] + "," + New_Lst[5];
            }
            return _SourceFile;
        }
        public int CompareTo(object o)
        {
            int another = ((CM3D2_File)o).Version;
            if (this.Version == another) return 0;
            if (this.Version < another) return 1;
            else return -1;
        }
    }
}
