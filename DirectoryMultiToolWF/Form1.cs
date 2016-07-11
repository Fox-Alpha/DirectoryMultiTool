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

		bool writeLog = false;
		string jsonKonfig = "";

		public Form1 ()
		{
			InitializeComponent ();
		}

		private void Form1_Load (object sender, EventArgs e)
		{
			string [] cmdLine;

			if ((cmdLine = Environment.GetCommandLineArgs()).Length > 1)
			{
				
				ReadJSonKonfiguration (cmdLine[1]);

				if (!Directory.Exists (dt.rootDirectory))
				{
					if (writeLog)
					{
						//Log Datei schreiben das Verzeichnis noch nicht existierte
						Debug.WriteLine ("Das Verzeichnis {0} wurde angelegt", dt.rootDirectory);
						WriteToLogFile ("Das Zielverzeichnis wurde angelegt", dt.rootDirectory);
					}
					Directory.CreateDirectory (dt.rootDirectory);
				}
				else
				{
					WriteToLogFile ("Das Zielverzeichnis ist bereit", dt.rootDirectory);
				}

				ExpandDirectoryList ();

				//if (!dt.silentTask)
				//{
				//}
				//else
				//	Debug.WriteLine ("Keine GUI Anzeige");
			}
		}

		void ExpandDirectoryList()
		{
			WriteToLogFile ("Erstellen der Verzeichnisbaumstruktur", dt.rootDirectory);
			List<string> DirName = new List<string> ();
			List<string> strTemp;
			string dirTemp = "";
			string val;

			if (dt.targetNames.Count > 0)
			{
				strTemp = new List<string> ();
				foreach (string str in dt.targetNames)
				{
					strTemp.Clear();

					foreach(string sep in dt.nameSeperator)
					{
						strTemp.AddRange (str.Split (sep.ToCharArray ()));
					}

					dirTemp = "";
					foreach (string split in strTemp)
					{
						if (dt.nameAliases.ContainsKey(split))
						{
							if (dt.nameAliases.TryGetValue (split, out val))
							{
								dirTemp += val+Path.DirectorySeparatorChar;
							}
						}
					}
					DirName.Add (dirTemp);
				}
			}
		}

		private void ReadJSonKonfiguration(string JSonFile)
		{
			if (!File.Exists(JSonFile))
			{
				Debug.WriteLine ("Fehlende Angabe oder Angegebene Konfiguration ungültig.");
				WriteToLogFile ("Fehlende Angabe oder Angegebene Konfiguration ungültig.", "");
				return;
			}

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

			using (StreamReader sr = new StreamReader (JSonFile))
			{
				using (JsonReader reader = new JsonTextReader (sr))
				{
					dt = serializer.Deserialize<DirectoryTask> (reader);
#if (DEBUG)
					string output = JsonConvert.SerializeObject (dt, jsonSerializerSettings);
					Debug.WriteLine (output);
#endif
					if (!string.IsNullOrWhiteSpace (dt.logFile))
					{
						writeLog = true;
						WriteToLogFile ("Start Operations", "");
					}
				}
			}
		}

		void WriteToLogFile(string Message, string AdditionalInfo)
		{
			if (!string.IsNullOrWhiteSpace (dt.logFile))
			{
				if (!File.Exists (dt.logFile))
				{
					using (StreamWriter sw = File.AppendText (dt.logFile))
					{
						sw.Write (string.Format ("{0}: {1} / {2}", DateTime.Now.ToString (), Message, AdditionalInfo));
					}
				}
			}
		}

		private void beendenToolStripMenuItem_Click (object sender, EventArgs e)
		{
			Application.Exit ();
		}
	}

	/* Klasse zum einlesen der JSON Konfiguration */
	[JsonObject (MemberSerialization.OptIn)]
	public class DirectoryTask
	{
		/* Die Datei die als Konfiguration geladen werden soll */
		public string jsonFile { get; set; }
		JsonSerializerSettings jsonSerializerSettings;

		[JsonProperty (PropertyName = "Names", Required = Required.Always)] 
		public List<string> targetNames;
		[JsonProperty(PropertyName = "Seperator", DefaultValueHandling = DefaultValueHandling.Populate)]
		public List<string> nameSeperator;
		[JsonProperty(Required = Required.AllowNull, PropertyName = "Alias")]
		public Dictionary<string, string> nameAliases;
		[JsonProperty(Required = Required.Always)]
		public string rootDirectory { get; set; }
		[JsonProperty(Required = Required.Default, PropertyName = "Silent")]
		public bool silentTask 
		{ get; set; }
		[JsonProperty(Required = Required.Default, PropertyName = "Log")]
		public string logFile
		{ get; set; }

		[JsonProperty (Required = Required.Default, PropertyName = "Vorlagen")]
		public Vorlagen vorlagen;	

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

	public class Vorlagen
	{
		[JsonProperty (Required = Required.Default, PropertyName = "Source")]
		string tplDirectory
		{ get; set; }
		[JsonProperty (Required = Required.Default, PropertyName = "Filter")]
		List<string> tplFilter;
		[JsonProperty (Required = Required.Default, PropertyName = "Files")]
		List<string> tplFiles;

		public Vorlagen ()
		{

		}
	}
}
