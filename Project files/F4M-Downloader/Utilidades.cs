using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

public class Utilidades
{
	public static int UnixTimestamp()
	{
		return (int)(DateTime.Now - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
	}

	public static string ReemplazaParametro(string start, string input, string parameter, string replacement){
		string pattern = start+parameter+" ((\".*?\")|([^ ]*))";
		Regex rgx = new Regex(pattern);
		return rgx.Replace(input, start+parameter+" \""+replacement+"\" ");
	}

	public static string GetParametro(string start, string input, string parameter){
		string pattern = start+parameter+" ((\".*?\")|([^ ]*))";
		Regex rgx = new Regex(pattern);
		MatchCollection matches = rgx.Matches(input);
		if(matches.Count > 0){
			if(matches[0].Groups.Count > 1 && matches[0].Groups[1].Value != "")
				return matches[0].Groups[1].Value.Substring(1, matches[0].Groups[1].Value.Length-2);
			else
				return matches[0].Value;
		}
		return "";
	}

	public static string nombreArchivoDebug;
	public static Bloqueo lockWL = new Bloqueo(false);
	public static string WL(string txt){
		lock (lockWL) {
			lockWL.bloqueado = true;
			if (nombreArchivoDebug == null) {
				nombreArchivoDebug = "logs/" + nombreValidoParaArchivo ("DEBUG " + DateTime.Now + ".txt");
				if (!Directory.Exists (MainClass.relativePath + "/logs")) {
					Directory.CreateDirectory ("logs");
				}
			}
			using (StreamWriter sw = File.AppendText (@nombreArchivoDebug)) {
				sw.WriteLine (txt);
			}
			Console.WriteLine (txt);
			lockWL.bloqueado = false;
		}
		return txt;
	}

	public static string nombreValidoParaArchivo(string nombre){
		foreach (char c in System.IO.Path.GetInvalidFileNameChars())
		{
			nombre = nombre.Replace(c, '_');
		}
		return nombre;
	}

	public static Configs leerConfigs(){
		Debug.WriteLine(Utilidades.WL("Intentando leer configs"));
		if (File.Exists ("configs.bin")) {
			BinaryFormatter formatter = new BinaryFormatter ();
			using (FileStream stream = File.OpenRead ("configs.bin")) {
				return (Configs)formatter.Deserialize (stream);
			}
		}
		return new Configs ();
	}

	public static void escribirConfigs(Configs configs){
		Debug.WriteLine(Utilidades.WL("Intentando escribir configs"));
		BinaryFormatter formatter = new BinaryFormatter();
		using (FileStream stream = File.OpenWrite("configs.bin"))
		{
			formatter.Serialize(stream, configs);
		}
	}
}

[Serializable]
public class Configs{
	public string rutaDescargas;
	public string proxy;
}

public class Bloqueo{
	public bool bloqueado = false;
	public Bloqueo(bool valor){
		bloqueado = valor;
	}
}