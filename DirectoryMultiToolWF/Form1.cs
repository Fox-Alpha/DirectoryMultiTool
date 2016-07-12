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

				//	Root des Zielverzeichnisses prüfen und bei bedarf anlegen
				if (!Directory.Exists (dt.rootDirectory))
				{
					if (writeLog)
					{
						//Log Datei schreiben das Verzeichnis noch nicht existierte
						Debug.WriteLine ("Das Verzeichnis {0} wurde angelegt", dt.rootDirectory);
						WriteToLogFile ("Das Zielverzeichnis {0} wurde angelegt", dt.rootDirectory);
					}
					Directory.CreateDirectory (dt.rootDirectory);
				}
				else
				{
					WriteToLogFile ("Das Zielverzeichnis {0} ist bereit", dt.rootDirectory);
				}

				//	Verzeichnisbaum zusammenbauen
				List<string> expDirList;
                if ((expDirList = ExpandDirectoryList ()) != null)
				{
					//	Verzeichnisse anlegen
					CreateDirectoryTree (expDirList);
				}

				foreach (string str in expDirList)
				{
					CopyTemplateFiles (dt.rootDirectory + str);
				}
				//if (!dt.silentTask)
				//{
				//}
				//else
				//	Debug.WriteLine ("Keine GUI Anzeige");
			}
		}

        void CopyTemplateFiles (string target)
		{
			if(Directory.Exists(dt.vorlagen.tplDirectory) && !string.IsNullOrWhiteSpace(target))
			{
				List<string> files = new List<string>();

				foreach(string str in dt.vorlagen.tplFilter)
				{
					files.AddRange(Directory.GetFiles (dt.vorlagen.tplDirectory, str));
				}			

				foreach (string file in files)
				{
					if (dt.vorlagen.OverwriteTpl)
					{
						File.Copy (file, target + Path.GetFileName (file), true);
						WriteToLogFile ("Die Datei {0} wurde {1} kopiert", new string [] { file, target + file });
					}
					else
						WriteToLogFile ("Die Datei {0} exitiert bereits", target + file);
				}
			}
		}

		void CreateDirectoryTree (List<string> dtree)
		{
			foreach (string dir in dtree)
			{
				if (!Directory.Exists (dt.rootDirectory + dir))
				{
					Directory.CreateDirectory (dt.rootDirectory + dir);
					WriteToLogFile ("Das Zielverzeichnis {0} wurde angelegt", dt.rootDirectory + dir);
				}
				else
				{
					WriteToLogFile ("Das Zielverzeichnis {0} exitiert bereits", dt.rootDirectory + dir);
				}
			}
		}

		List<string> ExpandDirectoryList()
		{
			WriteToLogFile ("Erstellen der Verzeichnisbaumstruktur in {0}", dt.rootDirectory);
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
				return DirName;
			}
			return null;
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

		void WriteToLogFile(string MessageFormat, params string[] vals)
		{
			if (!string.IsNullOrWhiteSpace (dt.logFile))
			{
				if(!dt.appenLog)
				{
					if (File.Exists(dt.logFile))
					{
						File.Delete (dt.logFile);
						dt.appenLog = true;
					}
				}

				using (StreamWriter sw = File.AppendText (dt.logFile))
				{
					if (vals.Length > 0)
					{
						sw.Write (string.Format ("{0}: {1}\r\n", DateTime.Now.ToString (), string.Format (MessageFormat, vals)));
					}
					else
						sw.Write (string.Format ("{0}: {1}\r\n", DateTime.Now.ToString (), MessageFormat));
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
		[JsonProperty (Required = Required.Default, PropertyName = "CreateNameDir")]
		public bool createNameDir
		{ get; set; }
		[JsonProperty (Required = Required.Default, PropertyName = "UseNameFile")]
		public bool useNameFile
		{ get; set; }
		[JsonProperty (Required = Required.Default, PropertyName = "NameFile")]
		public string nameFile
		{ get; set; }
		[JsonProperty(PropertyName = "Seperator", DefaultValueHandling = DefaultValueHandling.Populate)]
		public List<string> nameSeperator;
		[JsonProperty(Required = Required.AllowNull, PropertyName = "Alias")]
		public Dictionary<string, string> nameAliases;
		[JsonProperty(Required = Required.Always)]
		public string rootDirectory { get; set; }
		[JsonProperty(Required = Required.Default, PropertyName = "Silent")]
		public bool silentTask 
		{ get; set; }
		[JsonProperty (Required = Required.Default, PropertyName = "AppendLog")]
		public bool appenLog
		{ get; set; }
		[JsonProperty (Required = Required.Default, PropertyName = "Log")]
		public string logFile
		{ get; set; }
		[JsonProperty (Required = Required.Default, PropertyName = "LogPrefix")]
		public string logPrefix
		{ get; set; }
		[JsonProperty (Required = Required.Default, PropertyName = "LogSuffix")]
		public string logSuffix
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
		public string tplDirectory
		{ get; set; }
		[JsonProperty (Required = Required.Default, PropertyName = "Filter")]
		public List<string> tplFilter;
		[JsonProperty (Required = Required.Default, PropertyName = "Files")]
		public List<string> tplFiles;
		[JsonProperty (Required = Required.Default, PropertyName = "OverwriteTarget")]
		public bool OverwriteTpl;

		public Vorlagen ()
		{

		}
	}
}
