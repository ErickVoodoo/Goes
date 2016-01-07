using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace New_Goes.DataModel
{
    public class MenuItem
    {
        public MenuItem(String name, String description, String image, String position)
        {
            this.name = name;
            this.description = description;
            this.image = image;
            this.position = position;
        }

        public string name { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public string position { get; set; }
    }

    public class TemplateSource
    {
        public ObservableCollection<MenuItem> MenuList = new ObservableCollection<MenuItem>();

        public async Task<ObservableCollection<MenuItem>> getMenuItems()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string type = loader.GetString("type");

            Uri menuUri = new Uri("ms-appx:///DataModel/MenuItems-" + type + ".json");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(menuUri);
            /*List<MenuItem> movie1 = JsonConvert.DeserializeObject<List<MenuItem>>(await FileIO.ReadLinesAsync(file));*/

            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText.ToString());
            JsonArray jsonArray = jsonObject["MenuItems"].GetArray();
            
            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                MenuItem _sampleDataSource = new MenuItem(groupObject["name"].GetString(),
                                                       groupObject["description"].GetString(),
                                                       groupObject["image"].GetString(),
                                                       groupObject["position"].GetString());
                MenuList.Add(_sampleDataSource);
            }
            return MenuList;
        }
    }
}
