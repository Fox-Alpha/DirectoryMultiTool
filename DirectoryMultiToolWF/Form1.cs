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
		public Form1 ()
		{
			InitializeComponent ();
		}
	}

	/* Klasse zum einlesen der JSON Konfiguration */
	[JsonObject (MemberSerialization.OptIn)]
	public class DirectoryTask
	{
		/* Die Datei die als Konfiguration geladen werden soll */
		string jsonFile	{ get; set;	}

		[JsonProperty(PropertyName ="Names", Required = Required.Always)]
		List<string> targetNames;
		[JsonProperty(PropertyName = "Seperator", DefaultValueHandling = DefaultValueHandling.Populate)]
		List<string> nameSeperator;
		[JsonProperty(Required = Required.AllowNull)]
		Dictionary<string, string> nameAliases;
		[JsonProperty(Required = Required.Always)]
		string rootDirectory { get; set; }
		[JsonProperty(Required = Required.Default)]
		bool silentTask 
		{ get; set; }
		[JsonProperty(Required = Required.Default)]
		string logFile
		{ get; set; }
		// Templates
		[JsonProperty (Required = Required.Default)]
		string tplDirectory
		{ get; set; }
		[JsonProperty(Required = Required.Default)]
		List<string> tplFilter;
		[JsonProperty(Required = Required.Default)]
		List<string> tplFiles;

		public DirectoryTask(string _jsonFile)
		{
			if (File.Exists(jsonFile))
			{
				//	Einlesen der Konfiguration
			}
		}
	}
}
