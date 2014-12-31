using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

public class Descargador
{
	public string url = "a";
	public string nombre = "b";

	public double tiempoTotal = -1;
	public double tiempoActual = -1;
	public double horaInicio = -1;

	public int porcentajeInt = 0;
	public double porcentaje = 0;
	public string horaRestanteString = "";
	
	Boolean cancelado = false;
	public String estado = "";
	
	ProcessStartInfo procesoAdobeHDS;
	Process exeProcessProcesoAdobeHDS;

	
	public bool Comienza (string url, string nombre)
	{
		Debug.WriteLine(Utilidades.WL("Comenzando descarga = "+url));
		this.url = url;
		this.nombre = nombre;

		return download ();
	}

	public void Cancelar ()
	{
		Debug.WriteLine(Utilidades.WL("Cancelando descarga = "+url));
		if(exeProcessProcesoAdobeHDS!=null)
			if(!exeProcessProcesoAdobeHDS.HasExited)
				exeProcessProcesoAdobeHDS.Kill();
		cancelado = true;
		Console.WriteLine ("");
		Console.WriteLine ("Descarga cancelada.");
		Console.WriteLine ("");
		Debug.WriteLine(Utilidades.WL("descarga cancelada = "+url));
	}

	public bool download ()
	{

		MainClass.descargasEnProceso.Add (this);

		try {
			Console.WriteLine ("Descargando. Espere por favor...");

			if(nombre != ""){
				if(Utilidades.GetParametro("--", url, "outfile") == ""){
					url += " --outfile \""+nombre+"\" ";
				} else {
					url = Utilidades.ReemplazaParametro ("--", url, "outfile", nombre);
				}
			}

			if(Utilidades.GetParametro("--", url, "outdir") == ""){
				url += " --outdir \""+MainClass.configs.rutaDescargas.Replace("\\","\\\\")+"\" ";
			}

			if(MainClass.configs.proxy != null && MainClass.configs.proxy != ""){
				if(Utilidades.GetParametro("--", url, "proxy")!=""){
					url = Utilidades.ReemplazaParametro ("--", url, "proxy", MainClass.configs.proxy);
				}
				else{
					url += " --fproxy --proxy \""+MainClass.configs.proxy+"\" ";
				}
			}

			// borrar desde adobehds cuando se termine la descarga
			//url += " --delete";
			url += " --deleteend"; // Borrar los archivos temporales cuando la descarga esté completada, no antes.

			Debug.WriteLine(Utilidades.WL("Iniciando proceso AdobeHDS para = "+url));
			procesoAdobeHDS = new ProcessStartInfo ();
			procesoAdobeHDS.FileName = MainClass.adobeHDSFile;
			procesoAdobeHDS.Arguments = url;

			procesoAdobeHDS.UseShellExecute = false;
			procesoAdobeHDS.RedirectStandardOutput = true;
			procesoAdobeHDS.RedirectStandardError = true;
			procesoAdobeHDS.CreateNoWindow = true;


			exeProcessProcesoAdobeHDS = Process.Start (procesoAdobeHDS);
			Console.WriteLine ("");
			Console.WriteLine ("AdobeHDS lanzado con los parámetros:");
			Console.WriteLine ("AdobeHDS "+url);
			Debug.WriteLine(Utilidades.WL("AdobeHDS arrancado para = "+url));

			exeProcessProcesoAdobeHDS.OutputDataReceived += p_OutputDataReceived;
			exeProcessProcesoAdobeHDS.ErrorDataReceived += p_ErrorDataReceived;

			exeProcessProcesoAdobeHDS.BeginOutputReadLine();
			exeProcessProcesoAdobeHDS.BeginErrorReadLine();

			exeProcessProcesoAdobeHDS.WaitForExit ();
		} catch (Exception e) {
			//En caso de que FFmpeg falle no siempre dara excepcion (por ejemplo, cuando es necesario cambiar de proxy el server los enlaces no funcionan bien, pero no se activa este fallo)
			Console.WriteLine ("AdobeHDS ha fallado.");
			Debug.WriteLine(Utilidades.WL("AdobeHDS ha fallado"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
			estado = "AdobeHDS ha fallado";
			return false;
		}

		if (porcentajeInt == 0)
			estado = "Fallo";
		else {
			porcentaje = 100;
			porcentajeInt = 100;
			estado = "Terminado";
		}

		
		return !cancelado && estado == "Terminado";
	}

	public void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		//FFMPEG NO USA ESTO
		System.Diagnostics.Debug.WriteLine("Received from standard error: " + e.Data);
	}

	public void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine("Received from standard out: " + e.Data);
		//System.Diagnostics.Debug.WriteLine(e.Data);
		if (!String.IsNullOrEmpty(e.Data)) {
			if (e.Data.IndexOf ("Downloading ") != -1) {
				if (tiempoTotal == -1) {
					int inicio = e.Data.IndexOf ("/") +1;
					int final = e.Data.IndexOf (" ", inicio);
					tiempoTotal = int.Parse(e.Data.Substring(inicio, final - inicio));

					horaInicio = Utilidades.UnixTimestamp();
				} else {
					//Console.WriteLine(e.Data);

					int inicio = e.Data.IndexOf ("Downloading ") + 12;
					int final = e.Data.IndexOf ("/", inicio);
					//0.783 kB / 0.00 sec (0.0%)
					string tiempo = e.Data.Substring (inicio, final - inicio);

					tiempoActual = double.Parse (tiempo, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);

					System.Diagnostics.Debug.WriteLine (tiempo);
					System.Diagnostics.Debug.WriteLine (tiempoActual);

					porcentaje = Math.Round (tiempoActual / tiempoTotal * 100.0,
												2, MidpointRounding.AwayFromZero);

					porcentajeInt = (int)porcentaje;

					int horaActual = Utilidades.UnixTimestamp();

					int horaTranscurrida = (int)horaActual - (int)horaInicio;
					int horaRestante = (int)((horaTranscurrida/porcentaje)*(100-porcentaje));

					horaRestanteString = segundosATiempo (horaRestante);



					Console.Write (nombre + " - " + porcentajeInt + "%" + " - Quedan: " + horaRestanteString+"\r");

				}
			}
		}
	}

	public string segundosATiempo(int seg_ini) {
		int horas = (int)Math.Floor((double)(seg_ini/3600));
		int minutos = (int)Math.Floor((double)((seg_ini-(horas*3600))/60));
		int segundos = seg_ini-(horas*3600)-(minutos*60);
		if(horas > 0)
			return horas+" horas, "+minutos+"min, "+segundos+"seg";
		if(minutos > 0)
			return minutos+" min, "+segundos+" seg";
		return segundos+" seg";
	}
	
}
