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
        public Windows.Storage.StorageFolder Path { get; private set; }
        public Windows.Storage.StorageFile Update_lst { get; private set; }
        public List<CM3D2_File> File_List = new List<CM3D2_File>();

        public Lst_File(Windows.Storage.StorageFolder current)
        {
            Path = current;
            Plugin_Name = Path.Path.Split('\\').Last();
        }

        public async Task Get_update_lst()
        {
            Update_lst = await Path.GetFileAsync("update.lst");
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
        public string Path { get; private set; }
        public Windows.Storage.StorageFolder SourceFolder { get; private set; }
        public int Version { get; private set; }
        public string Parent { get; private set; }

        private Windows.Storage.StorageFile _SourceFile;

        public CM3D2_File(string parent, string lst_raw, Windows.Storage.StorageFolder folder)
        {
            Parent = parent;
            Lst_Line = lst_raw;
            Path = lst_raw.Split(',')[2];
            Version = int.Parse(lst_raw.Split(',')[5]);
            SourceFolder = folder;
        }
        public async Task<Windows.Storage.StorageFile> SourceFile()
        {
            if (_SourceFile == null)
            {
                _SourceFile = await SourceFolder.GetFileAsync("data\\" + Path.ToString());
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
