using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;


namespace DirectoryMultiToolWF
{
	public partial class Form1 : Form
	{
		DirectoryTask dt;

		public Form1 ()
		{
			InitializeComponent ();
		}

		private void Form1_Load (object sender, EventArgs e)
		{
			//	Setzen der Serializer Settings
			JsonSerializerSettings jsonSerializerSettings;

			jsonSerializerSettings = new JsonSerializerSettings ();
			jsonSerializerSettings.Formatting = Formatting.Indented;
			jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
			jsonSerializerSettings.NullValueHandling = NullValueHandling.Include;
			jsonSerializerSettings.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
			jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

			JsonSerializer serializer = JsonSerializer.CreateDefault (jsonSerializerSettings);

			//StreamWriter sw = new StreamWriter (@"data\exampleOut.json");
            //JsonWriter writer = new JsonTextWriter (sw);

			using (StreamReader sr = new StreamReader (@"data\example.json"))
				using (JsonReader reader = new JsonTextReader (sr))
				{
					dt = serializer.Deserialize<DirectoryTask> (reader);
				string output = JsonConvert.SerializeObject (dt, jsonSerializerSettings);

					Debug.WriteLine(output);
			
					// {"ExpiryDate":new Date(1230375600000),"Price":0}
			}
			//string output = JsonConvert.DeserializeObject<DirectoryTask> ();
		}
	}

	/* Klasse zum einlesen der JSON Konfiguration */
	[JsonObject (MemberSerialization.OptIn)]
	public class DirectoryTask
	{
		/* Die Datei die als Konfiguration geladen werden soll */
		string jsonFile { get; set; }
		JsonSerializerSettings jsonSerializerSettings;

		[JsonProperty (PropertyName = "Names", Required = Required.Always)] 
		List<string> targetNames;
		[JsonProperty(PropertyName = "Seperator", DefaultValueHandling = DefaultValueHandling.Populate)]
		List<string> nameSeperator;
		[JsonProperty(Required = Required.AllowNull, PropertyName = "Alias")]
		Dictionary<string, string> nameAliases;
		[JsonProperty(Required = Required.Always)]
		string rootDirectory { get; set; }
		[JsonProperty(Required = Required.Default, PropertyName = "Silent")]
		bool silentTask 
		{ get; set; }
		[JsonProperty(Required = Required.Default, PropertyName = "Log")]
		string logFile
		{ get; set; }
		// Templates
		[JsonProperty (Required = Required.Default, PropertyName = "Source")]
		string tplDirectory
		{ get; set; }
		[JsonProperty(Required = Required.Default, PropertyName = "Filter")]
		List<string> tplFilter;
		[JsonProperty(Required = Required.Default, PropertyName = "Files")]
		List<string> tplFiles;

		public DirectoryTask(string _jsonFile)
		{
			if (File.Exists(_jsonFile))
			{
				jsonFile = _jsonFile;

				//	Setzen der Serializer Settings
				jsonSerializerSettings = new JsonSerializerSettings ();
				jsonSerializerSettings.Formatting = Formatting.Indented;
				jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
				jsonSerializerSettings.NullValueHandling = NullValueHandling.Include;
				jsonSerializerSettings.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
				jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

                //JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(jsonSerializerSettings);
			}
		}
	}
}
