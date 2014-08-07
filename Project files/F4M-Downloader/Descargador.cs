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
	public String fallado = "";
	
	ProcessStartInfo procesoRTMPDUMP;
	Process exeProcessProcesoRTMPDUMP;

	
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
		if(exeProcessProcesoRTMPDUMP!=null)
			if(!exeProcessProcesoRTMPDUMP.HasExited)
				exeProcessProcesoRTMPDUMP.Kill();
		cancelado = true;
		Console.WriteLine ("");
		Console.WriteLine ("Descarga candelada.");
		Console.WriteLine ("");
		Debug.WriteLine(Utilidades.WL("descarga cancelada = "+url));
	}

	public bool download ()
	{

		MainClass.descargasEnProceso.Add (this);

		try {
			Console.WriteLine ("Descargando. Espere por favor...");

			if(nombre != "")
				url = Utilidades.ReemplazaParametro (url, "o", nombre);

			string parametroO = Utilidades.GetParametro(url, "o");
			if(parametroO.IndexOf(":\\") == -1){
				url = Utilidades.ReemplazaParametro (url, "o", MainClass.configs.rutaDescargas + parametroO);
			}

			if(MainClass.configs.proxy != null && MainClass.configs.proxy != ""){
				if(Utilidades.GetParametro(url, "S")!=""){
					url = Utilidades.ReemplazaParametro (url, "S", MainClass.configs.proxy);
				}
				else{
					url += " -S \""+MainClass.configs.proxy+"\"";
				}
			}

			Debug.WriteLine(Utilidades.WL("Iniciando proceso RTMPDump para = "+url));
			procesoRTMPDUMP = new ProcessStartInfo ();
			procesoRTMPDUMP.FileName = MainClass.rtmpdumpFile;
			procesoRTMPDUMP.Arguments = url;

			procesoRTMPDUMP.UseShellExecute = false;
			procesoRTMPDUMP.RedirectStandardOutput = true;
			procesoRTMPDUMP.RedirectStandardError = true;
			procesoRTMPDUMP.CreateNoWindow = true;


			exeProcessProcesoRTMPDUMP = Process.Start (procesoRTMPDUMP);
			Console.WriteLine ("");
			Console.WriteLine ("RTMPDump lanzado con los parámetros:");
			Console.WriteLine ("rtmpdump "+url);
			Debug.WriteLine(Utilidades.WL("RTMPDump arrancado para = "+url));

			exeProcessProcesoRTMPDUMP.OutputDataReceived += p_OutputDataReceived;
			exeProcessProcesoRTMPDUMP.ErrorDataReceived += p_ErrorDataReceived;

			exeProcessProcesoRTMPDUMP.BeginOutputReadLine();
			exeProcessProcesoRTMPDUMP.BeginErrorReadLine();

			exeProcessProcesoRTMPDUMP.WaitForExit ();
		} catch (Exception e) {
			//En caso de que FFmpeg falle no siempre dara excepcion (por ejemplo, cuando es necesario cambiar de proxy el server los enlaces no funcionan bien, pero no se activa este fallo)
			Console.WriteLine ("RTMPDump ha fallado.");
			Debug.WriteLine(Utilidades.WL("RTMPDump ha fallado"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
			fallado = "RTMPDump ha fallado";
			return false;
		}

		//if(!cancelado)
		//	return true;
		//else
		//	return true;

		if (porcentajeInt == 0)
			fallado = "Fallo";
		else {
			porcentaje = 100;
			porcentajeInt = 100;
		}

		
		return !cancelado;
	}

	public void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		//FFMPEG NO USA ESTO
		System.Diagnostics.Debug.WriteLine("Received from standard out: " + e.Data);
	}

	public void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine("Received from standard error: " + e.Data);
		//System.Diagnostics.Debug.WriteLine(e.Data);
		if (!String.IsNullOrEmpty(e.Data)) {
			if (e.Data.IndexOf ("kB / ") > 0) {
				if (tiempoTotal == -1) {
					tiempoTotal = 100;

					horaInicio = Utilidades.UnixTimestamp();
				} else {
					//Console.WriteLine(e.Data);

					int inicio = e.Data.IndexOf ("(") + 1;
					int final = e.Data.IndexOf ("%", inicio);
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
