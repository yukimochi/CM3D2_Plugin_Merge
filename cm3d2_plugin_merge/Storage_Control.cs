using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cm3d2_plugin_merge
{
    static class Storage_Control
    {
        public static async Task<List<Windows.Storage.StorageFolder>> Get_Item(Windows.Storage.StorageFolder folder, bool IsX64)
        {
            var items = await folder.GetFoldersAsync();
            var f = new List<Windows.Storage.StorageFolder>();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path.ToString().Contains("_oh_"))
                {

                }
                else
                {
                    if (await Get_update(items[i], IsX64))
                    {
                        f.Add(items[i]);
                    }
                    else
                    {
                        f.AddRange(await Get_Item(items[i], IsX64));
                    }
                }
            }
            return f;
        }

        public static async Task<bool> Get_update(Windows.Storage.StorageFolder folder, bool IsX64)
        {
            var items = await folder.GetFilesAsync();
            var update = false;

            for (int i = 0; i < items.Count; i++)
            {
                if (IsX64)
                {
                    if (items[i].Name == "update.lst" || items[i].Name == "update_x64.lst")
                    {
                        update = true;
                    }
                }
                else
                {
                    if (items[i].Name == "update.lst" || items[i].Name == "update_x86.lst")
                    {
                        update = true;
                    }
                }
            }
            return update;
        }

        public static async Task<Windows.Storage.StorageFolder> Get_Folder(string path, Windows.Storage.StorageFolder current)
        {
            var branch = path.Split('\\');
            var here = current;
            for (int i = 0; i < branch.Count(); i++)
            {
                if (branch[i] != "")
                {
                    here = await here.CreateFolderAsync(branch[i], Windows.Storage.CreationCollisionOption.OpenIfExists);
                }
            }
            return here;
        }

        public static async Task<string> Open_lst(Windows.Storage.StorageFile file)
        {
            var txt = await Windows.Storage.FileIO.ReadTextAsync(file);

            return txt;
        }

        public static async Task Save_Log(Windows.Storage.StorageFolder folder, string Filename, string data)
        {
            var item = await folder.CreateFileAsync(Filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(item, data);
        }

        public static async Task Place_readme(Windows.Storage.StorageFolder folder)
        {
            var app_root = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var resorce = await app_root.GetFolderAsync("Resource");
            var readme = await resorce.GetFileAsync("readme.txt");

            await readme.CopyAsync(folder);
        }
    }
}
