using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;


public class NetworkServer
{
	TcpListener listener;

	public NetworkServer (){}

	public bool Abre(int puerto){
		try{
			Console.WriteLine ("Abriendo servidor en \"127.0.0.1:" + puerto + "\"...");
			listener = new TcpListener (IPAddress.Loopback, puerto);
			listener.Start ();

			Console.WriteLine ("Servidor abierto.");
			return true;
		}
		catch(Exception e){
			Console.WriteLine ("h0");
			Console.WriteLine (e);
			Debug.WriteLine(Utilidades.WL("h0"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
			return false;
		}
	}

	private string streamReadLine(Stream inputStream) {
		int next_char;
		string data = "";
		while (true) {
			next_char = inputStream.ReadByte();
			if (next_char == '\n') { break; }
			if (next_char == '\r') { continue; }
			//if (next_char == -1) { System.Threading.Thread.Sleep(1); continue; }
			if (next_char == -1) { break; }
			data += Convert.ToChar(next_char);
		}            
		return data;
	}

	private static byte[] GetBytes(string str)
	{
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}

	private void streamWrite(Stream inputStream, string text) {
		inputStream.Flush ();

		byte[] bytes = System.Text.Encoding.UTF8.GetBytes (text);
		inputStream.Write (bytes, 0, bytes.Length);
	}
	/*
	public bool puedeEscuchar (){
		return listener.Pending;
	}
	*/
	public RespuestaServer Escucha ()
	{

		try{
			/*if(client != null)
				return new RespuestaServer(new RespuestaHTTP(false), null, null);*/

			TcpClient client = listener.AcceptTcpClient ();
			//Console.WriteLine ("Conexion establecida");

			Stream stream = new BufferedStream(client.GetStream ());
			//stream = client.GetStream ();


			string GETurl = streamReadLine(stream);
			
			if(GETurl != null){
				string pattern = " (.*?) HTTP";
				MatchCollection matches = Regex.Matches (GETurl, pattern);

				string url="";

				if (matches.Count > 0) {
					GroupCollection gc = matches[0].Groups;
					CaptureCollection cc = gc[1].Captures;
					url = cc[0].Value;
				}
				//Console.WriteLine (url);


				pattern = "\\?(&?([^=^&]+?)=([^&]*))*";
				matches = Regex.Matches (url, pattern);
				//Utilidades.print_r_regex(matches);
				if (matches.Count > 0) {
					GroupCollection gc = matches[0].Groups;
					CaptureCollection variables = gc[2].Captures;
					CaptureCollection valores = gc[3].Captures;

					ParametroGet[] parametros = new ParametroGet[variables.Count];
					for(int i = 0; i < variables.Count; i++){
						parametros[i] = new ParametroGet(
											Uri.UnescapeDataString(variables[i].Value).Replace("+", " "),
											Uri.UnescapeDataString(valores[i].Value).Replace("+", " "));
					}
					return new RespuestaServer(new RespuestaHTTP(url, parametros), client, stream);
				}
				return new RespuestaServer(new RespuestaHTTP(url), client, stream);
			}
			return new RespuestaServer(new RespuestaHTTP(false), client, stream);

		}
		catch(Exception e){
			Console.WriteLine ("h1");
			Console.WriteLine (e);
			Debug.WriteLine(Utilidades.WL("h1"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
			//CierraCliente ();
			return new RespuestaServer(new RespuestaHTTP(false), null, null);
		}
	}

	public bool Envia (RespuestaServer respuestaServer, string que)
	{
		try {
			streamWrite(respuestaServer.stream, "HTTP/1.1 200 OK\r\n"+
				"Connection: Close\r\n"+
				"Content-Type: text/html; charset=utf-8\r\n"+
				"Connection: Close\r\n"+
				"\r\n"+que);


			CierraCliente (respuestaServer);
			return true;
		} catch (Exception e) {
			Console.WriteLine ("h2");
			Console.WriteLine (e);
			Debug.WriteLine(Utilidades.WL("h2"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
			return false;
		}
	}
	
	public bool EnviaRaw(RespuestaServer respuestaServer, String contentType, byte[] contenido){
		try {
			streamWrite(respuestaServer.stream, "HTTP/1.1 200 OK\r\n"+
				"Connection: Close\r\n"+
				"Content-Type: "+contentType+"\r\n"+
				"Connection: Close\r\n"+
				"\r\n");
			respuestaServer.stream.Write(contenido, 0, contenido.Length);

			CierraCliente (respuestaServer);
			return true;
		} catch (Exception e) {
			Console.WriteLine ("h10");
			Console.WriteLine (e);
			Debug.WriteLine(Utilidades.WL("h10"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
			return false;
		}
	}

	public bool EnviaLocation (RespuestaServer respuestaServer, String que)
	{
		try {
			streamWrite(respuestaServer.stream, "HTTP/1.1 301 OK\r\n");
			streamWrite(respuestaServer.stream, "Location: " + que+"\r\n");
			streamWrite(respuestaServer.stream, "Content-Length: 0\r\n");
			streamWrite(respuestaServer.stream, "Connection: Close\r\n");
			streamWrite(respuestaServer.stream, "\r\n");

			CierraCliente (respuestaServer);
			return true;
		} catch (Exception e) {
			Console.WriteLine("h3");
			Console.WriteLine (e);
			Debug.WriteLine(Utilidades.WL("h3"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
			return false;
		}
	}

	public void CierraCliente (RespuestaServer respuestaServer)
	{
		try{
			respuestaServer.stream.Dispose();
			respuestaServer.stream.Close ();
			respuestaServer.stream = null;
			//Console.WriteLine("Cliente cerrado.");

			respuestaServer.client.Close();
			respuestaServer.client = null;
		}
		catch(Exception e){
			Console.WriteLine("h4");
			Console.WriteLine (e);
			Debug.WriteLine(Utilidades.WL("h4"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
		}
	}

	public void Cierra ()
	{
		try{
			//CierraCliente();
			this.listener.Stop ();
			Console.WriteLine("Servidor cerrado.");
		}
		catch(Exception e){
			Console.WriteLine("h5");
			Console.WriteLine (e);
			Debug.WriteLine(Utilidades.WL("h5"));
			Debug.WriteLine(Utilidades.WL(e.ToString()));
		}
	}
}

public class RespuestaHTTP
{
	public String url;
	public String path;
	public ParametroGet[] parametros;
	bool _correcto;
	public bool correcto{
		get{
			return _correcto;
		}
		set{ }
	}

	public RespuestaHTTP (String url)
	{
		this._correcto = true;
		
		this.setURL(url);
	}

	public RespuestaHTTP (bool correcto)
	{
		this.correcto = correcto;
	}

	public RespuestaHTTP (String url, ParametroGet[] parametros)
	{
		this.setURL(url);
		this.parametros = parametros;
		this._correcto = true;
	}
	
	void setURL(String url){
		this.url = url;
		
		String pattern = "[^\\?]*";
		MatchCollection matches = Regex.Matches (url, pattern);
		//Utilidades.print_r_regex(matches);

		this.path = matches[0].Groups[0].Captures[0].Value;
	}

	public bool tieneParametros(){
		return parametros != null;
	}

	public bool existeParametro(String variable){
		if (!tieneParametros())
			return false;

		for(int i = 0; i < parametros.Length; i++){
			if (parametros[i].variable == variable)
				return true;
		}
		return false;
	}

	public String getParametro(String variable){
		if (!tieneParametros())
			return "";

		for (int i = 0; i < parametros.Length; i++) {
			if (parametros [i].variable == variable)
				return parametros [i].valor;
		}
		return "";
	}
}

public class ParametroGet{
	String _variable;
	String _valor;

	public String variable{
		get{
			return _variable;
		}
		set{ }
	}
	public String valor{
		get{
			return _valor;
		}
		set{ }
	}


	public ParametroGet (String variable, String valor)
	{
		this._variable = variable;
		this._valor = valor;
	}
}

public class RespuestaServer{
	RespuestaHTTP _respuestaHTTP;
	TcpClient _client;
	Stream _stream;

	public RespuestaHTTP respuestaHTTP{
		get{
			return _respuestaHTTP;
		}
		set{ }
	}

	public TcpClient client{
		get{
			return _client;
		}
		set{ }
	}

	public Stream stream{
		get{
			return _stream;
		}
		set{ }
	}

	public RespuestaServer (RespuestaHTTP respuestaHTTP, TcpClient client, Stream stream)
	{
		this._respuestaHTTP = respuestaHTTP;
		this._client = client;
		this._stream = stream;
	}
}