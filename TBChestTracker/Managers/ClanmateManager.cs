using Emgu.CV.Features2D;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows;

namespace TBChestTracker
{
    public class ClanmateManager 
    {
        public ClanmatesDatabase Database { get; private set; }

        public void UpdateCount()
        {
            Database.NumClanmates = Database.Clanmates.Count;
        }
        public ClanmateManager() 
        {
            if (Database == null)
            {
                Database = new ClanmatesDatabase();
                Database.Clanmates = new ObservableCollection<Clanmate>();
            }
        }
        public void Add(string name)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Database.Clanmates.Add(new Clanmate(name));
                UpdateCount();
            });
        }
        public void AddAliases(string name, List<string> aliases)
        {
            foreach(var mate in Database.Clanmates.ToList())
            {
                if(mate.Name.ToLower().Equals(name.ToLower()))
                {
                    mate.Aliases.InsertRange(mate.Aliases.Count, aliases);
                }
            }
            UpdateCount();
        }
        public void Remove(string name)
        {
            foreach(var c in Database.Clanmates.ToList())
            {
                if (c.Name == name)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Database.Clanmates.Remove(c);
                    });
                }
            }
            UpdateCount();  
        }
        public async void ImportFromFileAsync(string path)
        {
            using(TextReader  reader = new StreamReader(path)) 
            {
                string data = await reader.ReadToEndAsync();
                if (data.Contains("\r\n"))
                {
                    data = data.Replace("\r\n", ",");
                }
                else
                {
                    data = data.Replace("\n", ",");
                }

                data = data.Substring(0, data.LastIndexOf(","));

                string[] dataCollection = data.Split(',');
                Database.Clanmates.Clear();
                foreach (var c in dataCollection)
                {
                    if(!String.IsNullOrEmpty(c))
                        Database.Clanmates.Add(new Clanmate(c));
                }
                reader.Close();
                UpdateCount();
            }
        }
        public void Load(string path)
        {
            bool requiresUpdate = false;
            string backuptext = String.Empty;

            Database.Clanmates.Clear();
            Database.NumClanmates = Database.Clanmates.Count;


            using (StreamReader sr = File.OpenText(path))
            {
                var data = StringHelpers.ConvertToUTF8(sr.ReadToEnd());
                if (JsonHelper.isJson(data))
                {
                    Database = JsonConvert.DeserializeObject<ClanmatesDatabase>(data);
                    sr.Close();
                }
                else
                {
                    requiresUpdate = true;
                    backuptext = data;
                    //--- we'd need to update it to new format.
                    if (data.Contains("\r\n"))
                        data = data.Replace("\r\n", ",");
                    else
                        data = data.Replace("\n", ",");

                    data = data.Substring(0, data.LastIndexOf(","));
                    string[] dataCollection = data.Split(',');

                    Database.Version = 2;
                    Database.Clanmates.Clear();
                    foreach (var c in dataCollection)
                    {
                        if (!string.IsNullOrEmpty(c))
                            Database.Clanmates.Add(new Clanmate(c));
                    }

                    UpdateCount();
                    sr.Close();
                }
            }

            if (requiresUpdate)
            {
                //-- create back up.

                var backupfile = path.Substring(0, path.LastIndexOf("."));
                backupfile += ".old";

                using (StreamWriter bsw = File.CreateText(backupfile))
                {
                    bsw.Write(backuptext);
                    bsw.Close();
                }
                Save(path);
            }
        }
        
        public void Save(string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                string jsondata = StringHelpers.ConvertToUTF8(JsonConvert.SerializeObject(Database, Formatting.Indented));
                sw.Write(jsondata);
                sw.Close();
            }
        }
    }
}
